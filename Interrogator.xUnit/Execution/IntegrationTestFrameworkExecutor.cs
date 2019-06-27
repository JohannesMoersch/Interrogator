using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Discovery;
using Interrogator.xUnit.Infrastructure;
using Xunit.Sdk;

namespace Interrogator.xUnit.Execution
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

				var jobs = testCases
					.Select(testCase => (method: testCase.Method.ToRuntimeMethod(), state: IntegrationTestExecutionJob.Create(testCase, messageBus)))
					.ToDictionary(set => set.method, set => set.state);

				var newTestStates = new List<ExecutionJob>(jobs.Values);

				while (newTestStates.Any())
				{
					var methods = newTestStates.SelectMany(testState => testState.ParameterMethods.Concat(testState.ConstructorParameterMethods)).ToArray();

					newTestStates.Clear();

					foreach (var method in methods)
					{
						if (!jobs.ContainsKey(method))
						{
							var executionJob = MethodExecutionJob.Create(method);
							newTestStates.Add(executionJob);
							jobs.Add(method, executionJob);
						}
					}
				}

				messageBus.QueueMessage(new TestAssemblyStarting(testCases, _testAssembly, DateTime.Now, "Test Environment", "Test Framework Display Name"));

				while (true)
				{
					var set = jobs.FirstOrDefault(s => s.Value.Status == ExecutionStatus.Ready);

					if (set.Value == null)
						break;

					var result = set.Value.Execute(cancellationTokenSource).Result;

					result
						.Match
						(
							success =>
							{
								foreach (var state in jobs.Values)
									state.SetParameter(set.Key, success);

								return Unit.Value;
							},
							failure => Unit.Value
						);
				}

				foreach (var state in jobs.Values.Where(state => state.Status == ExecutionStatus.NotReady))
					state.Abort(jobs);

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
