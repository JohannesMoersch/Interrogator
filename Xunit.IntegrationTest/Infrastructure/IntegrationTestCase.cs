using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.IntegrationTest.Execution;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Infrastructure
{
	internal class IntegrationTestCase : XunitTestCase
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public IntegrationTestCase()
		{ }

		public IntegrationTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod) 
			: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, null)
		{
		}

		public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
			=> throw new NotImplementedException();
	}
}
