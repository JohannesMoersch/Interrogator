using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using Xunit.IntegrationTest.Discovery;
using Xunit.IntegrationTest.Infrastructure;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal class IntegrationTestFrameworkExecutor : TestFrameworkExecutor<IntegrationTestCase>
	{
		private readonly ITestAssembly _testAssembly;

		public IntegrationTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink)
			: base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
			=> _testAssembly = new TestAssembly(Reflector.Wrap(Assembly.Load(assemblyName)), null, assemblyName.Version);

		public override void RunAll(IMessageSink executionMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
		{
			base.RunAll(executionMessageSink, discoveryOptions, executionOptions);
		}

		protected override void RunTestCases(IEnumerable<IntegrationTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
		{
			using (var messageBus = CreateMessageBus(executionMessageSink, executionOptions))
			{
				messageBus.QueueMessage(new TestAssemblyStarting(testCases, _testAssembly, DateTime.Now, "Test Environment", "Test Framework Display Name"));
				foreach (var testCase in testCases)
				{
					IntegrationTest
						.Create(testCase)
						.Match
						(
							test =>
							{
								messageBus.QueueMessage(new TestCollectionStarting(new[] { testCase }, testCase.TestMethod.TestClass.TestCollection));
								messageBus.QueueMessage(new TestClassStarting(new[] { testCase }, testCase.TestMethod.TestClass));
								messageBus.QueueMessage(new TestMethodStarting(new[] { testCase }, testCase.TestMethod));
								messageBus.QueueMessage(new TestCaseStarting(testCase));
								messageBus.QueueMessage(new TestStarting(test));
								messageBus.QueueMessage(new TestClassConstructionStarting(test)); // Only do when not static
								messageBus.QueueMessage(new TestClassConstructionFinished(test)); // Only do when not static

								//await new ExceptionAggregator().RunAsync(() => test.TestCase.TestMethod.Method.ToRuntimeMethod().Invoke(null, null) as Task);

								messageBus.QueueMessage(new TestPassed(test, 1.0m, "Success!"));

								messageBus.QueueMessage(new TestFinished(test, 1.0m, "Success!"));
								messageBus.QueueMessage(new TestCaseFinished(testCase, 1.0m, 1, 1, 0));
								messageBus.QueueMessage(new TestMethodFinished(new[] { testCase }, testCase.TestMethod, 1.0m, 1, 1, 0));
								messageBus.QueueMessage(new TestClassFinished(new[] { testCase }, testCase.TestMethod.TestClass, 1.0m, 1, 1, 0));
								messageBus.QueueMessage(new TestCollectionFinished(new[] { testCase }, testCase.TestMethod.TestClass.TestCollection, 1.0m, 1, 1, 0));

								return 0;
							},
							error =>
							{
								var test = new ErrorIntegrationTest(testCase);

								messageBus.QueueMessage(new TestCollectionStarting(new[] { testCase }, testCase.TestMethod.TestClass.TestCollection));
								messageBus.QueueMessage(new TestClassStarting(new[] { testCase }, testCase.TestMethod.TestClass));
								messageBus.QueueMessage(new TestMethodStarting(new[] { testCase }, testCase.TestMethod));
								messageBus.QueueMessage(new TestCaseStarting(testCase));
								messageBus.QueueMessage(new TestStarting(test));
								messageBus.QueueMessage(new TestClassConstructionStarting(test)); // Only do when not static
								messageBus.QueueMessage(new TestClassConstructionFinished(test)); // Only do when not static

								//await new ExceptionAggregator().RunAsync(() => test.TestCase.TestMethod.Method.ToRuntimeMethod().Invoke(null, null) as Task);

								messageBus.QueueMessage(new TestFailed(test, 0, null, new[] { typeof(InvalidOperationException).FullName }, new[] { error }, new[] { "" }, new[] { -1 }));

								messageBus.QueueMessage(new TestFinished(test, 1.0m, "Success!"));
								messageBus.QueueMessage(new TestCaseFinished(testCase, 1.0m, 1, 1, 0));
								messageBus.QueueMessage(new TestMethodFinished(new[] { testCase }, testCase.TestMethod, 1.0m, 1, 1, 0));
								messageBus.QueueMessage(new TestClassFinished(new[] { testCase }, testCase.TestMethod.TestClass, 1.0m, 1, 1, 0));
								messageBus.QueueMessage(new TestCollectionFinished(new[] { testCase }, testCase.TestMethod.TestClass.TestCollection, 1.0m, 1, 1, 0));

								return 0;
							}
						);

					
				}
				messageBus.QueueMessage(new TestAssemblyFinished(testCases, _testAssembly, 1.0m, 1, 0, 0));
			}
		}

		static IMessageBus CreateMessageBus(IMessageSink messageSink, ITestFrameworkExecutionOptions options)
		{
			if (options.SynchronousMessageReportingOrDefault())
				return new SynchronousMessageBus(messageSink);

			return new MessageBus(messageSink);
		}

		protected override ITestFrameworkDiscoverer CreateDiscoverer()
			=> new IntegrationTestFrameworkDiscoverer(_testAssembly.Assembly, SourceInformationProvider, DiagnosticMessageSink);

		public ITestFrameworkDiscoverer GetDiscoverer()
			=> CreateDiscoverer();
	}
}
