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
		public enum TestStatus
		{
			NotReady,
			Ready,
			NotComplete,
			Complete
		}

		private IntegrationTestState(IntegrationTestCase testCase, MethodInfo[] parameterMethods)
		{
			TestCase = testCase;
			Status = parameterMethods.Length == 0 ? TestStatus.Ready : TestStatus.NotReady;

			_parameterMethods = parameterMethods;

			_values = new Option<object>[_parameterMethods.Length];
		}

		private IntegrationTestState(IntegrationTestCase testCase, string errorMessage)
		{
			TestCase = testCase;
			Status = TestStatus.Ready;

			_errorMessage = errorMessage ?? String.Empty;
		}

		public IntegrationTestCase TestCase { get; }

		public TestStatus Status { get; private set; }

		private MethodInfo[] _parameterMethods;

		private Option<object>[] _values;

		private string _errorMessage;

		public void SetParameter(MethodInfo methodInfo, object value)
		{
			if (Status != TestStatus.NotReady)
				return;

			bool ready = true;
			for (int i = 0; i < _parameterMethods.Length; ++i)
			{
				if (_parameterMethods[i] == methodInfo && _values[i] == Option.None<object>())
					_values[i] = Option.Some(value);

				if (_values[i] == Option.None<object>())
					ready = false;
			}

			if (ready)
				Status = TestStatus.Ready;
		}

		public async Task<object> Execute(IMessageBus messageBus, ExceptionAggregator exceptionAggregator, CancellationTokenSource cancellationTokenSource)
		{
			if (Status != TestStatus.Ready)
				throw new Exception("Test prerequisites not met.");

			var test = new IntegrationTest(TestCase);

			messageBus.QueueMessage(new TestCollectionStarting(new[] { TestCase }, TestCase.TestMethod.TestClass.TestCollection));
			messageBus.QueueMessage(new TestClassStarting(new[] { TestCase }, TestCase.TestMethod.TestClass));
			messageBus.QueueMessage(new TestMethodStarting(new[] { TestCase }, TestCase.TestMethod));

			object returnValue = null;

			if (_errorMessage != null)
			{
				messageBus.QueueMessage(new TestCaseStarting(TestCase));
				messageBus.QueueMessage(new TestStarting(test));

				messageBus.QueueMessage(new TestFailed(test, 0, null, new[] { typeof(InvalidOperationException).FullName }, new[] { _errorMessage }, new[] { "" }, new[] { -1 }));

				messageBus.QueueMessage(new TestFinished(test, 0, null));
				messageBus.QueueMessage(new TestCaseFinished(TestCase, 0, 1, 1, 0));

				Status = TestStatus.NotComplete;
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
								value => { returnValue = value; Status = TestStatus.Complete; },
								() => Status = TestStatus.Complete
							),
						failure => Status = TestStatus.NotComplete
					);
			}

			messageBus.QueueMessage(new TestMethodFinished(new[] { TestCase }, TestCase.TestMethod, 0, 1, 1, 0));
			messageBus.QueueMessage(new TestClassFinished(new[] { TestCase }, TestCase.TestMethod.TestClass, 0, 1, 1, 0));
			messageBus.QueueMessage(new TestCollectionFinished(new[] { TestCase }, TestCase.TestMethod.TestClass.TestCollection, 0, 1, 1, 0));

			return returnValue;
		}

		public Task Abort(IMessageBus messageBus, ExceptionAggregator exceptionAggregator, CancellationTokenSource cancellationTokenSource)
		{
			_errorMessage = "Stuff";

			return Execute(messageBus, exceptionAggregator, cancellationTokenSource);
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
							parameter.ParameterType.IsAssignableFrom(method.ReturnType),
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
	}
}
