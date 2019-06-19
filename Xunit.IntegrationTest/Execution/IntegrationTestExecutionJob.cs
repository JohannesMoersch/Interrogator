using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.IntegrationTest.Common;
using Xunit.IntegrationTest.Infrastructure;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal static class IntegrationTestExecutionJob
	{
		public static ExecutionJob Create(IntegrationTestCase testCase, IMessageBus messageBus)
		{
			if (testCase is ErrorIntegrationTestCase errorTestCase)
				return ExecutionJob.Create(testCase.Method.ToRuntimeMethod(), errorTestCase.ErrorMessage, (arguments, cancellationTokenSource) => FailTest(testCase, messageBus, errorTestCase.ErrorMessage), abortMessage => AbortTest(testCase, messageBus, abortMessage));

			return ExecutionJob.Create(testCase.Method.ToRuntimeMethod(), (arguments, cancellationTokenSource) => ExecuteTest(testCase, messageBus, arguments, cancellationTokenSource), abortMessage => AbortTest(testCase, messageBus, abortMessage));
		}

		private static void SendStartMessages(IntegrationTestCase testCase, IMessageBus messageBus)
		{
			messageBus.QueueMessage(new TestCollectionStarting(new[] { testCase }, testCase.TestMethod.TestClass.TestCollection));
			messageBus.QueueMessage(new TestClassStarting(new[] { testCase }, testCase.TestMethod.TestClass));
			messageBus.QueueMessage(new TestMethodStarting(new[] { testCase }, testCase.TestMethod));
		}

		private static void SendStopMessages(IntegrationTestCase testCase, IMessageBus messageBus)
		{
			messageBus.QueueMessage(new TestMethodFinished(new[] { testCase }, testCase.TestMethod, 0, 1, 1, 0));
			messageBus.QueueMessage(new TestClassFinished(new[] { testCase }, testCase.TestMethod.TestClass, 0, 1, 1, 0));
			messageBus.QueueMessage(new TestCollectionFinished(new[] { testCase }, testCase.TestMethod.TestClass.TestCollection, 0, 1, 1, 0));
		}

		private static async Task<Result<Option<object>, Exception>> ExecuteTest(IntegrationTestCase testCase, IMessageBus messageBus, object[] arguments, CancellationTokenSource cancellationTokenSource)
		{
			SendStartMessages(testCase, messageBus);

			var result = await testCase.Execute(Array.Empty<object>(), arguments, messageBus, new ExceptionAggregator(), cancellationTokenSource);

			SendStopMessages(testCase, messageBus);

			return result;
		}

		private static Task<Result<Option<object>, Exception>> FailTest(IntegrationTestCase testCase, IMessageBus messageBus, string errorMessage)
		{
			var test = new IntegrationTest(testCase);

			SendStartMessages(testCase, messageBus);

			messageBus.QueueMessage(new TestCaseStarting(testCase));
			messageBus.QueueMessage(new TestStarting(test));

			messageBus.QueueMessage(new TestFailed(test, 0, null, new[] { typeof(InvalidOperationException).FullName }, new[] { errorMessage }, new[] { "" }, new[] { -1 }));

			messageBus.QueueMessage(new TestFinished(test, 0, null));
			messageBus.QueueMessage(new TestCaseFinished(testCase, 0, 1, 1, 0));

			SendStopMessages(testCase, messageBus);

			return Task.FromResult(Result.Failure<Option<object>, Exception>(new InvalidOperationException(errorMessage)));
		}

		private static void AbortTest(IntegrationTestCase testCase, IMessageBus messageBus, string abortMessage)
		{
			var test = new IntegrationTest(testCase);

			SendStartMessages(testCase, messageBus);

			messageBus.QueueMessage(new TestCaseStarting(testCase));
			messageBus.QueueMessage(new TestStarting(test));

			messageBus.QueueMessage(new TestSkipped(test, abortMessage));

			messageBus.QueueMessage(new TestFinished(test, 0, null));
			messageBus.QueueMessage(new TestCaseFinished(testCase, 0, 1, 1, 0));

			SendStopMessages(testCase, messageBus);
		}
	}
}
