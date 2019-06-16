using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.IntegrationTest.Common;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal class IntegrationTestInvoker : XunitTestInvoker
	{
		private readonly Action<Result<Option<object>, Unit>> _resultCallback;

		public IntegrationTestInvoker(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, Action<Result<Option<object>, Unit>> resultCallback)
			: base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterAttributes, aggregator, cancellationTokenSource)
		{
			_resultCallback = resultCallback;
		}

		protected override object CallTestMethod(object testClassInstance)
		{
			try
			{
				var result = base.CallTestMethod(testClassInstance);

				var task = GetTaskFromResult(result);

				if (task != null)
				{
					return task.ContinueWith(t =>
					{
						if (t.IsFaulted)
							_resultCallback.Invoke(Result.Failure<Option<object>, Unit>(Unit.Value));
						else if (t.IsCanceled)
							_resultCallback.Invoke(Result.Failure<Option<object>, Unit>(Unit.Value));
						else if (IsGenericTaskType(t.GetType()))
							_resultCallback.Invoke(Result.Success<Option<object>, Unit>(Option.FromNullable((object)((dynamic)t).Result)));
						else
							_resultCallback.Invoke(Result.Success<Option<object>, Unit>(Option.None<object>()));

						return t;
					});
				}

				_resultCallback.Invoke(Result.Success<Option<object>, Unit>(Option.FromNullable(result)));

				return result;
			}
			catch (Exception ex)
			{
				_resultCallback.Invoke(Result.Failure<Option<object>, Unit>(Unit.Value));

				ExceptionDispatchInfo.Capture(ex).Throw();

				throw;
			}
		}

		private static bool IsGenericTaskType(Type taskType)
		{
			while (taskType != null && (!taskType.IsConstructedGenericType || taskType.GetGenericTypeDefinition() != typeof(Task<>)))
				taskType = taskType.BaseType;

			return taskType != null;
		}
	}
}
