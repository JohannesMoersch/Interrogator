using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
				var aggregator = new ExceptionAggregator();

				var cancellationTokenSource = new CancellationTokenSource();

				var testStates = testCases
					.Select(testCase => (method: testCase.Method.ToRuntimeMethod(), state: IntegrationTestExecutionJob.Create(testCase, messageBus)))
					.ToDictionary(set => set.method, set => set.state);

				var newTestStates = new List<ExecutionJob>(testStates.Values);

				foreach (var testState in testStates)
				{
				}

				messageBus.QueueMessage(new TestAssemblyStarting(testCases, _testAssembly, DateTime.Now, "Test Environment", "Test Framework Display Name"));

				while (true)
				{
					var set = testStates.FirstOrDefault(s => s.Value.Status == ExecutionStatus.Ready);

					if (set.Value == null)
						break;

					var result = set.Value.Execute(cancellationTokenSource).Result;

					foreach (var state in testStates.Values)
						state.SetParameter(set.Key, result);
				}

				foreach (var state in testStates.Values.Where(state => state.Status == ExecutionStatus.NotReady))
					state.Abort();

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
