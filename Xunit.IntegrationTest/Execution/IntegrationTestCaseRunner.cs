using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.IntegrationTest.Common;
using Xunit.IntegrationTest.Infrastructure;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal class IntegrationTestCaseRunner : XunitTestCaseRunner
	{
		private readonly Action<Result<Option<object>, Unit>> _resultCallback;

		public IntegrationTestCaseRunner(IXunitTestCase testCase, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, Action<Result<Option<object>, Unit>> resultCallback)
			: base(testCase, testCase.DisplayName, testCase.SkipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource)
		{
			_resultCallback = resultCallback;
		}

		protected override ITest CreateTest(IXunitTestCase testCase, string displayName)
			=> new IntegrationTest((IntegrationTestCase)testCase);

		protected override XunitTestRunner CreateTestRunner(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
			=> new IntegrationTestRunner(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, beforeAfterAttributes, new ExceptionAggregator(aggregator), cancellationTokenSource, _resultCallback);
	}
}
