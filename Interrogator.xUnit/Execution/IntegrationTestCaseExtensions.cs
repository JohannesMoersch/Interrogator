using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Infrastructure;
using Interrogator.xUnit.Utilities;
using Xunit.Sdk;

namespace Interrogator.xUnit.Execution
{
	internal static class IntegrationTestCaseExtensions
	{
		public static async Task<Result<Option<object>, TestFailure>> Execute(this IntegrationTestCase testCase, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
		{
			var result = default(Result<Option<object>, TestFailure>?);

			var interceptor = new MessageBusWrapper(messageBus, message => 
				{
					if (result.HasValue)
						return;

					if (message is TestFailed failedTest)
						result = Result.Failure<Option<object>, TestFailure>(new TestFailure(new Exception(String.Join(Environment.NewLine, failedTest.Messages))));

					if (message is TestSkipped skippedTest)
						result = Result.Failure<Option<object>, TestFailure>(new TestFailure(skippedTest.Reason));
				});

			await new IntegrationTestCaseRunner(testCase, constructorArguments, testMethodArguments, interceptor, aggregator, cancellationTokenSource, value => result = value.MapFailure(ex => new TestFailure(ex))).RunAsync();

			return result.Value;
		}
	}
}
