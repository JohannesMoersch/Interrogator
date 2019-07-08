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
using System.Security;

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
			var disableParallelization = executionOptions.DisableParallelization() ?? false;
			var maxParallelThreads = disableParallelization ? 1 : (executionOptions.MaxParallelThreads() ?? 0);

			if (maxParallelThreads <= 0)
				maxParallelThreads = Environment.ProcessorCount;

			SynchronizationContext oldSynchronizationContext = null;
			if (maxParallelThreads > 1 && MaxConcurrencySyncContext.IsSupported)
			{
				oldSynchronizationContext = SynchronizationContext.Current;
				SetSynchronizationContext(new MaxConcurrencySyncContext(maxParallelThreads));
			}

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
					var methods = newTestStates
						.SelectMany(testState => testState.Dependencies)
						.ToArray();

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

				messageBus.QueueMessage(new TestAssemblyStarting(testCases, _testAssembly, DateTime.Now, $"Interrogator {(disableParallelization ? "non-parallel" : $"parallel ({maxParallelThreads} threads)")}", "Interrogator Test Framework"));

				ExecuteJobs(jobs, cancellationTokenSource).Wait();

				messageBus.QueueMessage(new TestAssemblyFinished(testCases, _testAssembly, 1.0m, 1, 0, 0));
			}

			if (oldSynchronizationContext != null)
				SetSynchronizationContext(oldSynchronizationContext);
		}

		private async Task ExecuteJobs(Dictionary<MethodInfo, ExecutionJob> jobs, CancellationTokenSource cancellationTokenSource)
		{
			var tasks = new List<Task<(MethodInfo method, Result<Option<object>, Unit> result)>>();
			while (true)
			{
				foreach (var ready in jobs.Where(s => s.Value.Status == ExecutionStatus.Ready))
					tasks.Add(ExecuteJob(ready.Key, ready.Value, cancellationTokenSource));

				if (!tasks.Any())
					break;

				var completedTask = await Task.WhenAny(tasks);

				tasks.Remove(completedTask);

				var completed = await completedTask;

				completed
					.result
					.Match
					(
						success =>
						{
							foreach (var state in jobs.Values)
							{
								state.SetParameterSuccess(completed.method, success);
								state.SetDependencyComplete(completed.method, true);
							}

							return Unit.Value;
						},
						failure =>
						{
							foreach (var state in jobs.Values)
								state.SetDependencyComplete(completed.method, false);

							return Unit.Value;
						}
					);
			}

			foreach (var state in jobs.Values.Where(state => state.Status == ExecutionStatus.NotReady))
				state.Abort(jobs);
		}

		private async Task<(MethodInfo method, Result<Option<object>, Unit> result)> ExecuteJob(MethodInfo method, ExecutionJob job, CancellationTokenSource cancellationTokenSource)
		{
			try
			{
				Task<Result<Option<object>, Unit>> result;
				if (SynchronizationContext.Current != null)
				{
					var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
					result = Task.Factory.StartNew(() => job.Execute(cancellationTokenSource), cancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap();
				}
				else
					result = Task.Run(() => job.Execute(cancellationTokenSource), cancellationTokenSource.Token);

				return (method, await result);
			}
			catch
			{
				return (method, Result.Failure<Option<object>, Unit>(Unit.Value));
			}
		}

		protected override ITestFrameworkDiscoverer CreateDiscoverer()
			=> new IntegrationTestFrameworkDiscoverer(_testAssembly.Assembly, SourceInformationProvider, DiagnosticMessageSink);

		public ITestFrameworkDiscoverer GetDiscoverer()
			=> CreateDiscoverer();

		private static IMessageBus CreateMessageBus(IMessageSink messageSink, ITestFrameworkExecutionOptions options)
		{
			if (options.SynchronousMessageReportingOrDefault())
				return new SynchronousMessageBus(messageSink);

			return new MessageBus(messageSink);
		}

		[SecuritySafeCritical]
		private static void SetSynchronizationContext(SynchronizationContext context)
			=> SynchronizationContext.SetSynchronizationContext(context);
	}
}
