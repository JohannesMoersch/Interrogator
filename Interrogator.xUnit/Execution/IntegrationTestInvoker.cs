using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Interrogator.xUnit.Common;
using Xunit.Sdk;

namespace Interrogator.xUnit.Execution
{
	internal class IntegrationTestInvoker : XunitTestInvoker
	{
		private readonly Action<Result<Option<object>, Exception>> _resultCallback;

		public IntegrationTestInvoker(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, Action<Result<Option<object>, Exception>> resultCallback)
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
							_resultCallback.Invoke(Result.Failure<Option<object>, Exception>(t.Exception));
						else if (t.IsCanceled)
							_resultCallback.Invoke(Result.Failure<Option<object>, Exception>(new TaskCanceledException()));
						else if (IsGenericTaskType(t.GetType()))
							_resultCallback.Invoke(Result.Success<Option<object>, Exception>(Option.FromNullable((object)((dynamic)t).Result)));
						else
							_resultCallback.Invoke(Result.Success<Option<object>, Exception>(Option.None<object>()));

						return t;
					});
				}

				_resultCallback.Invoke(Result.Success<Option<object>, Exception>(Option.FromNullable(result)));

				return result;
			}
			catch (Exception ex)
			{
				_resultCallback.Invoke(Result.Failure<Option<object>, Exception>(ex));

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
