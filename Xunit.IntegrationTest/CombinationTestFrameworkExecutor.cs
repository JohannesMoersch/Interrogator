using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.IntegrationTest
{
	public class CombinationTestFrameworkExecutor : LongLivedMarshalByRefObject, ITestFrameworkExecutor
	{
		public CombinationTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink)
		{
			_integrationTestFrameworkExector = new IntegrationTestFrameworkExecutor(assemblyName, sourceInformationProvider, diagnosticMessageSink);
			_xunitTestFrameworkExecutor = new XunitTestFrameworkExecutorWrapper(assemblyName, sourceInformationProvider, diagnosticMessageSink);
		}

		private readonly IntegrationTestFrameworkExecutor _integrationTestFrameworkExector;
		private readonly XunitTestFrameworkExecutorWrapper _xunitTestFrameworkExecutor;

		public void Dispose()
		{
			_integrationTestFrameworkExector.Dispose();
			_xunitTestFrameworkExecutor.Dispose();
		}

		public void RunAll(IMessageSink executionMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
		{
			Record("Run All");

			var testCases = GetTestCasesFromDiscoverer(_integrationTestFrameworkExector.GetDiscoverer(), discoveryOptions)
				.Concat(GetTestCasesFromDiscoverer(_xunitTestFrameworkExecutor.GetDiscoverer(), discoveryOptions))
				.ToArray();

			RunTests(testCases, executionMessageSink, executionOptions);
		}

		public void RunTests(IEnumerable<ITestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
		{
			Record("------------------------------");
			Record("Run Tests");

			foreach (var testCase in testCases)
			{
				Record(testCase.GetType().Name + " - " + testCase.DisplayName);
			}

			executionMessageSink = new Stuff(executionMessageSink, "output.txt");

			using (var messageBus = new SynchronousMessageBus(executionMessageSink))
				messageBus.QueueMessage(new TestAssemblyStarting(testCases, _xunitTestFrameworkExecutor.TestAssembly, DateTime.Now, $"{ IntPtr.Size * 8 } - bit.NET", XunitTestFrameworkDiscoverer.DisplayName));

			_integrationTestFrameworkExector
				.RunTests
				(
					testCases.OfType<IntegrationTestCase>(), 
					new FilterableMessageSink
					(
						executionMessageSink, 
						message =>
						{
							if (message is TestAssemblyStarting)
								return null;
							if (message is TestAssemblyFinished)
							{
								_xunitTestFrameworkExecutor
									.RunTests
									(
										testCases.OfType<XunitTestCase>(), 
										new FilterableMessageSink
										(
											executionMessageSink, 
											m =>
											{
												if (m is TestAssemblyStarting)
													return null;
												if (m is TestAssemblyFinished)
													new TestAssemblyFinished(testCases, _xunitTestFrameworkExecutor.TestAssembly, 1.0m, 2, 0, 0);
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

		// -------------------------------------------------- //

		public static void Record(string message, string fileName = "output.txt")
		{
			while (true)
			{
				try
				{
					System.IO.File.AppendAllText(@"\\WANSHITONG\Johannes\Git\xunit.integrationtest\Xunit.IntegrationTest\bin\Debug\netstandard2.0\" + fileName, DateTime.Now.ToLongTimeString() + ": " + message + Environment.NewLine);
					return;
				}
				catch { }
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

	public class Stuff : IMessageSink
	{
		private readonly IMessageSink _messageSink;
		private readonly string _fileName;

		public Stuff(IMessageSink messageSink, string fileName)
		{
			_messageSink = messageSink;
			_fileName = fileName;
		}

		public bool OnMessage(IMessageSinkMessage message)
		{
			if (message is ErrorMessage error)
				CombinationTestFrameworkExecutor.Record($"ErrorMesssage - {String.Join(", ", error.Messages ?? Array.Empty<string>())} - {error.StackTraces.FirstOrDefault() ?? ""}", _fileName);
			else
				CombinationTestFrameworkExecutor.Record(message.ToString(), _fileName);

			return _messageSink.OnMessage(message);
		}
	}
}
