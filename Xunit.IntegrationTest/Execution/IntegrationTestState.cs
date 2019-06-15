using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.IntegrationTest.Common;
using Xunit.IntegrationTest.Infrastructure;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal class IntegrationTestState
	{
		private IntegrationTestState(IntegrationTestCase testCase, MethodInfo[] parameterMethods)
		{
			TestCase = testCase;
			Status = parameterMethods.Length == 0 ? ExecutionStatus.Ready : ExecutionStatus.NotReady;

			ParameterMethods = parameterMethods;

			_values = new Option<object>[ParameterMethods.Count];
		}

		private IntegrationTestState(IntegrationTestCase testCase, string errorMessage)
		{
			TestCase = testCase;
			Status = ExecutionStatus.Ready;

			_errorMessage = errorMessage ?? String.Empty;
		}

		public IntegrationTestCase TestCase { get; }

		public ExecutionStatus Status { get; private set; }

		public IReadOnlyList<MethodInfo> ParameterMethods { get; }

		private Option<object>[] _values;

		private string _errorMessage;

		public void SetParameter(MethodInfo methodInfo, object value)
		{
			if (Status != ExecutionStatus.NotReady)
				return;

			bool ready = true;
			for (int i = 0; i < ParameterMethods.Count; ++i)
			{
				if (ParameterMethods[i] == methodInfo && _values[i] == Option.None<object>())
					_values[i] = Option.Some(value);

				if (_values[i] == Option.None<object>())
					ready = false;
			}

			if (ready)
				Status = ExecutionStatus.Ready;
		}

		public async Task<object> Execute(IMessageBus messageBus, ExceptionAggregator exceptionAggregator, CancellationTokenSource cancellationTokenSource)
		{
			if (Status != ExecutionStatus.Ready)
				throw new Exception("Test prerequisites not met.");

			var test = new IntegrationTest(TestCase);

			SendStartMessages(messageBus);

			object returnValue = null;

			if (_errorMessage != null)
			{
				messageBus.QueueMessage(new TestCaseStarting(TestCase));
				messageBus.QueueMessage(new TestStarting(test));

				messageBus.QueueMessage(new TestFailed(test, 0, null, new[] { typeof(InvalidOperationException).FullName }, new[] { _errorMessage }, new[] { "" }, new[] { -1 }));

				messageBus.QueueMessage(new TestFinished(test, 0, null));
				messageBus.QueueMessage(new TestCaseFinished(TestCase, 0, 1, 1, 0));

				Status = ExecutionStatus.NotComplete;
			}
			else
			{
				var result = await TestCase.Execute(Array.Empty<object>(), _values.Select(o => o.Match(_ => _, () => default)).ToArray(), messageBus, new ExceptionAggregator(exceptionAggregator), cancellationTokenSource);

				result
					.Apply
					(
						success => success
							.Apply
							(
								value => { returnValue = value; Status = ExecutionStatus.Complete; },
								() => Status = ExecutionStatus.Complete
							),
						failure => Status = ExecutionStatus.NotComplete
					);
			}

			SendStartMessages(messageBus);

			return returnValue;
		}

		private void SendStartMessages(IMessageBus messageBus)
		{
			messageBus.QueueMessage(new TestCollectionStarting(new[] { TestCase }, TestCase.TestMethod.TestClass.TestCollection));
			messageBus.QueueMessage(new TestClassStarting(new[] { TestCase }, TestCase.TestMethod.TestClass));
			messageBus.QueueMessage(new TestMethodStarting(new[] { TestCase }, TestCase.TestMethod));
		}

		private void SendStopMessages(IMessageBus messageBus)
		{
			messageBus.QueueMessage(new TestMethodFinished(new[] { TestCase }, TestCase.TestMethod, 0, 1, 1, 0));
			messageBus.QueueMessage(new TestClassFinished(new[] { TestCase }, TestCase.TestMethod.TestClass, 0, 1, 1, 0));
			messageBus.QueueMessage(new TestCollectionFinished(new[] { TestCase }, TestCase.TestMethod.TestClass.TestCollection, 0, 1, 1, 0));
		}

		public void Abort(IMessageBus messageBus, ExceptionAggregator exceptionAggregator, CancellationTokenSource cancellationTokenSource)
		{
			var test = new IntegrationTest(TestCase);

			SendStartMessages(messageBus);

			messageBus.QueueMessage(new TestCaseStarting(TestCase));
			messageBus.QueueMessage(new TestStarting(test));

			messageBus.QueueMessage(new TestSkipped(test, GetAbortMessage()));

			messageBus.QueueMessage(new TestFinished(test, 0, null));
			messageBus.QueueMessage(new TestCaseFinished(TestCase, 0, 1, 1, 0));

			SendStopMessages(messageBus);
		}

		private string GetAbortMessage()
		{
			var parameters = TestCase.Method.ToRuntimeMethod().GetParameters();

			var missingParameters = _values
				.Select((o, i) => o.Match(_ => null, () => (int?)i))
				.Where(i => i != null)
				.Select(i => (parameter: parameters[i.Value], source: ParameterMethods[i.Value]))
				.Select(set => $"{set.parameter.Name} <- {set.source.DeclaringType.FullName}.{set.source.Name}({String.Join(",", set.source.GetParameters().Select(p => p.ParameterType.Name))})")
				.Select(str => $"{Environment.NewLine}\t{str}");

			return $"The sources for the following parameters failed:{String.Join("", missingParameters)}";
		}

		public static IntegrationTestState Create(IntegrationTestCase testCase)
		{
			if (testCase is ErrorIntegrationTestCase errorTestCase)
				return new IntegrationTestState(errorTestCase, errorTestCase.ErrorMessage);

			var testMethod = testCase
				.TestMethod
				.Method
				.ToRuntimeMethod();

			return testMethod
				.GetParameters()
				.Select(parameter => parameter
					.GetFromAttribute()
					.Bind(att => att.TryGetMethod(testMethod.DeclaringType))
					.Bind(method => Result
						.Create
						(
							IsReturnTypeCompatible(method.ReturnType, parameter.ParameterType),
							() => method,
							() => $"Cannot cast return type of source method '{method.DeclaringType.Name}.{method.Name}' from '{method.ReturnType.Name}' to '{parameter.ParameterType.Name}' for parameter '{parameter.Name}' on method '{parameter.Member.DeclaringType.Name}.{parameter.Member.Name}'"
						)
					)
				)
				.TakeUntilFailure()
				.Match
				(
					methods => new IntegrationTestState(testCase, methods),
					error => new IntegrationTestState(testCase, error)
				);
		}

		private static bool IsReturnTypeCompatible(Type returnType, Type targetType)
		{
			if (returnType.IsConstructedGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
				returnType = returnType.GetGenericArguments()[0];

			return targetType.IsAssignableFrom(returnType);
		}
	}
}
