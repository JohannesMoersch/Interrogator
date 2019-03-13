using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using Xunit.IntegrationTest.Infrastructure;
using Xunit.IntegrationTest.Utilities;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal class CombinationTestFrameworkExecutor : LongLivedMarshalByRefObject, ITestFrameworkExecutor
	{
		public CombinationTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink)
		{
			_xunitTestFrameworkExecutor = new XunitTestFrameworkExecutorWrapper(assemblyName, sourceInformationProvider, diagnosticMessageSink);
			_integrationTestFrameworkExector = new IntegrationTestFrameworkExecutor(assemblyName, sourceInformationProvider, diagnosticMessageSink);
		}

		private readonly XunitTestFrameworkExecutorWrapper _xunitTestFrameworkExecutor;
		private readonly IntegrationTestFrameworkExecutor _integrationTestFrameworkExector;

		public void Dispose()
		{
			_xunitTestFrameworkExecutor.Dispose();
			_integrationTestFrameworkExector.Dispose();
		}

		public void RunAll(IMessageSink executionMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
		{
			var testCases = GetTestCasesFromDiscoverer(_xunitTestFrameworkExecutor.GetDiscoverer(), discoveryOptions)
				.Concat(GetTestCasesFromDiscoverer(_integrationTestFrameworkExector.GetDiscoverer(), discoveryOptions))
				.ToArray();

			RunTests(testCases, executionMessageSink, executionOptions);
		}

		public void RunTests(IEnumerable<ITestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
		{
			using (var messageBus = new SynchronousMessageBus(executionMessageSink))
				messageBus.QueueMessage(new TestAssemblyStarting(testCases, _xunitTestFrameworkExecutor.TestAssembly, DateTime.Now, $"{ IntPtr.Size * 8 } - bit.NET", XunitTestFrameworkDiscoverer.DisplayName));

			_xunitTestFrameworkExecutor
				.RunTests
				(
					testCases.OfType<XunitTestCase>(), 
					new FilterableMessageSink
					(
						executionMessageSink, 
						message =>
						{
							if (message is TestAssemblyStarting)
								return null;
							if (message is TestAssemblyFinished)
							{
								_integrationTestFrameworkExector
									.RunTests
									(
										testCases.OfType<IntegrationTestCase>(), 
										new FilterableMessageSink
										(
											executionMessageSink, 
											m =>
											{
												if (m is TestAssemblyStarting)
													return null;
												if (m is TestAssemblyFinished)
													return new TestAssemblyFinished(testCases, _xunitTestFrameworkExecutor.TestAssembly, 1.0m, 2, 0, 0);
												return m;
											}), 
										executionOptions
									);

								return null;
							}
							return message;
						}
					), 
					executionOptions
				);
		}

		public ITestCase Deserialize(string value) 
			=> _xunitTestFrameworkExecutor.Deserialize(value);

		private IEnumerable<ITestCase> GetTestCasesFromDiscoverer(ITestFrameworkDiscoverer discoverer, ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			using (discoverer)
			{
				var discoverySink = new TestDiscoverySink();

				discoverer.Find(false, discoverySink, discoveryOptions);

				return discoverySink.GetTestCases().Result;
			}
		}

		private class XunitTestFrameworkExecutorWrapper : XunitTestFrameworkExecutor
		{
			public XunitTestFrameworkExecutorWrapper(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink) 
				: base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
			{
			}

			public new ITestAssembly TestAssembly => base.TestAssembly;

			public ITestFrameworkDiscoverer GetDiscoverer()
				=> CreateDiscoverer();
		}
	}
}
