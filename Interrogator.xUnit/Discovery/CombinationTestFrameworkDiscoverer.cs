using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Utilities;
using Xunit.Sdk;
using Xunit;

namespace Interrogator.xUnit.Discovery
{
	internal class CombinationTestFrameworkDiscoverer : LongLivedMarshalByRefObject, ITestFrameworkDiscoverer
	{
		public CombinationTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink)
		{
			_xunitTestFrameworkDiscoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, sourceProvider, diagnosticMessageSink);
			_integrationTestFrameworkDiscoverer = new IntegrationTestFrameworkDiscoverer(assemblyInfo, sourceProvider, diagnosticMessageSink);
		}

		private readonly XunitTestFrameworkDiscoverer _xunitTestFrameworkDiscoverer;
		private readonly IntegrationTestFrameworkDiscoverer _integrationTestFrameworkDiscoverer;

		public string TargetFramework => _integrationTestFrameworkDiscoverer.TargetFramework;

		public string TestFrameworkDisplayName => _integrationTestFrameworkDiscoverer.TestFrameworkDisplayName;

		public void Dispose()
		{
			_xunitTestFrameworkDiscoverer.Dispose();
			_integrationTestFrameworkDiscoverer.Dispose();
		}

		public void Find(bool includeSourceInformation, IMessageSink discoveryMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			_xunitTestFrameworkDiscoverer
				.Find
				(
					includeSourceInformation, 
					new FilterableMessageSink
					(
						discoveryMessageSink, 
						message =>
						{
							if (message is TestCaseDiscoveryMessage discoveryMessage && discoveryMessage.TestMethod.Method.IsIntegrationTest())
								return null;
							if (message is DiscoveryCompleteMessage)
							{
								_integrationTestFrameworkDiscoverer.Find(includeSourceInformation, discoveryMessageSink, discoveryOptions);

								return null;
							}
							return message;
						}
					), 
					discoveryOptions
				);
		}

		public void Find(string typeName, bool includeSourceInformation, IMessageSink discoveryMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			_xunitTestFrameworkDiscoverer.Find
				(
					typeName,
					includeSourceInformation,
					new FilterableMessageSink
					(
						discoveryMessageSink,
						message =>
						{
							if (message is TestCaseDiscoveryMessage discoveryMessage && discoveryMessage.TestMethod.Method.IsIntegrationTest())
								return null;
							if (message is DiscoveryCompleteMessage)
							{
								_integrationTestFrameworkDiscoverer.Find(typeName, includeSourceInformation, discoveryMessageSink, discoveryOptions);

								return null;
							}
							return message;
						}
					),
					discoveryOptions
				);
		}

		public string Serialize(ITestCase testCase)
			=> _xunitTestFrameworkDiscoverer.Serialize(testCase);
	}
}
