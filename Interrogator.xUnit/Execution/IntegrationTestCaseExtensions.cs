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
		public static async Task<Result<Option<object>, Exception>> Execute(this IntegrationTestCase testCase, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
		{
			var result = default(Result<Option<object>, Exception>?);

			var interceptor = new MessageBusWrapper(messageBus, message => { if (message is TestFailed test && !result.HasValue) result = Result.Failure<Option<object>, Exception>(new Exception(String.Join(Environment.NewLine, test.Messages))); });

			await new IntegrationTestCaseRunner(testCase, constructorArguments, testMethodArguments, interceptor, aggregator, cancellationTokenSource, value => result = value).RunAsync();

			return result.Value;
		}
	}
}
