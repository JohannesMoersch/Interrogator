using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.IntegrationTest
{
	internal class IntegrationTestFrameworkExecutor : TestFrameworkExecutor<IntegrationTestCase>
	{
		private readonly ITestAssembly _testAssembly;

		public IntegrationTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink)
			: base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
			=> _testAssembly = new TestAssembly(Reflector.Wrap(Assembly.Load(assemblyName)));

		public override void RunAll(IMessageSink executionMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
		{
			base.RunAll(executionMessageSink, discoveryOptions, executionOptions);
		}

		protected override async void RunTestCases(IEnumerable<IntegrationTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
		{
			/*
			using (var messageBus = CreateMessageBus(executionMessageSink, executionOptions))
			{
				messageBus.QueueMessage(new TestAssemblyStarting(testCases, _testAssembly, DateTime.Now, "Test Environment", "Test Framework Display Name"));
				foreach (var testCase in testCases)
				{
					var test = new TestTest(testCase);

					messageBus.QueueMessage(new TestCollectionStarting(new[] { testCase }, testCase));
					messageBus.QueueMessage(new TestClassStarting(new[] { testCase }, testCase));
					messageBus.QueueMessage(new TestMethodStarting(new[] { testCase }, testCase));
					messageBus.QueueMessage(new TestCaseStarting(testCase));
					messageBus.QueueMessage(new TestStarting(test));
					messageBus.QueueMessage(new TestClassConstructionStarting(test));
					messageBus.QueueMessage(new TestClassConstructionFinished(test));

					await new ExceptionAggregator().RunAsync(() => test.TestCase.TestMethod.Method.ToRuntimeMethod().Invoke(null, null) as Task);

					messageBus.QueueMessage(new TestPassed(test, 1.0m, "Success!"));
					messageBus.QueueMessage(new TestFinished(test, 1.0m, "Success!"));
					messageBus.QueueMessage(new TestCaseFinished(testCase, 1.0m, 1, 0, 0));
					messageBus.QueueMessage(new TestMethodFinished(new[] { testCase }, testCase, 1.0m, 1, 0, 0));
					messageBus.QueueMessage(new TestClassFinished(new[] { testCase }, testCase, 1.0m, 1, 0, 0));
					messageBus.QueueMessage(new TestCollectionFinished(new[] { testCase }, testCase, 1.0m, 1, 0, 0));
				}
				messageBus.QueueMessage(new TestAssemblyFinished(testCases, _testAssembly, 1.0m, 1, 0, 0));
			}
			*/
		}

		static IMessageBus CreateMessageBus(IMessageSink messageSink, ITestFrameworkExecutionOptions options)
		{
			if (options.SynchronousMessageReportingOrDefault())
				return new SynchronousMessageBus(messageSink);

			return new MessageBus(messageSink);
		}

		protected override ITestFrameworkDiscoverer CreateDiscoverer()
			=> new IntegrationTestFrameworkDiscoverer(_testAssembly.Assembly, SourceInformationProvider, DiagnosticMessageSink);
	}
}
