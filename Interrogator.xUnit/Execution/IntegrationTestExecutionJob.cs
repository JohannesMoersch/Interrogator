using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Infrastructure;
using Xunit.Sdk;

namespace Interrogator.xUnit.Execution
{
	internal static class IntegrationTestExecutionJob
	{
		public static ExecutionJob Create(IntegrationTestCase testCase, IMessageBus messageBus)
		{
			var method = testCase.Method.ToRuntimeMethod();

			return Result
				.Create(!(testCase is ErrorIntegrationTestCase), () => method, () => (testCase as ErrorIntegrationTestCase).ErrorMessage)
				.Bind(ExecutionData.Create)
				.Match
				(
					executionData => ExecutionJob.Create(method, executionData, (arguments, cts) => ExecuteTest(testCase, messageBus, arguments.methodArguments, arguments.constructorArguments, cts), abortMessage => AbortTest(testCase, messageBus, abortMessage)),
					errorMessage => ExecutionJob.Create(method, ExecutionData.Empty, (arguments, cts) => FailTest(testCase, messageBus, errorMessage), abortMessage => AbortTest(testCase, messageBus, abortMessage))
				);
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

		private static async Task<Result<Option<object>, TestFailure>> ExecuteTest(IntegrationTestCase testCase, IMessageBus messageBus, object[] methodArguments, object[] constructorArguments, CancellationTokenSource cancellationTokenSource)
		{
			SendStartMessages(testCase, messageBus);

			var result = await testCase.Execute(constructorArguments, methodArguments, messageBus, new ExceptionAggregator(), cancellationTokenSource);

			SendStopMessages(testCase, messageBus);

			return result;
		}

		private static Task<Result<Option<object>, TestFailure>> FailTest(IntegrationTestCase testCase, IMessageBus messageBus, string errorMessage)
		{
			var test = new IntegrationTest(testCase);

			SendStartMessages(testCase, messageBus);

			messageBus.QueueMessage(new TestCaseStarting(testCase));
			messageBus.QueueMessage(new TestStarting(test));

			messageBus.QueueMessage(new TestFailed(test, 0, null, new[] { typeof(InvalidOperationException).FullName }, new[] { errorMessage }, new[] { "" }, new[] { -1 }));

			messageBus.QueueMessage(new TestFinished(test, 0, null));
			messageBus.QueueMessage(new TestCaseFinished(testCase, 0, 1, 1, 0));

			SendStopMessages(testCase, messageBus);

			return Task.FromResult(Result.Failure<Option<object>, TestFailure>(new TestFailure(new InvalidOperationException(errorMessage))));
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
