using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.IntegrationTest.Common;
using Xunit.IntegrationTest.Infrastructure;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal static class IntegrationTestCaseExtensions
	{
		public static async Task<Result<Option<object>, Exception>> Execute(this IntegrationTestCase testCase, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
		{
			var result = default(Result<Option<object>, Exception>);

			await new IntegrationTestCaseRunner(testCase, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource, value => result = value).RunAsync();

			return result;
		}
	}
}
