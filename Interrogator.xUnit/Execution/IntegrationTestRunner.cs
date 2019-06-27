using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Infrastructure;
using Xunit.Sdk;

namespace Interrogator.xUnit.Execution
{
	internal class IntegrationTestRunner : XunitTestRunner
	{
		private readonly Action<Result<Option<object>, Exception>> _resultCallback;

		public IntegrationTestRunner(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, Action<Result<Option<object>, Exception>> resultCallback)
			: base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
		{
			_resultCallback = resultCallback;
		}

		protected override Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
			=> new IntegrationTestInvoker(Test, MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments, BeforeAfterAttributes, aggregator, CancellationTokenSource, _resultCallback).RunAsync();
	}
}
