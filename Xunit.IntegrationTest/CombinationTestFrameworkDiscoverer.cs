using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.IntegrationTest
{
	internal class CombinationTestFrameworkDiscoverer : ITestFrameworkDiscoverer
	{
		public CombinationTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink)
		{
			_integrationTestFrameworkDiscoverer = new IntegrationTestFrameworkDiscoverer(assemblyInfo, sourceProvider, diagnosticMessageSink);
			_xunitTestFrameworkDiscoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, sourceProvider, diagnosticMessageSink);
		}

		private readonly IntegrationTestFrameworkDiscoverer _integrationTestFrameworkDiscoverer;
		private readonly XunitTestFrameworkDiscoverer _xunitTestFrameworkDiscoverer;

		public string TargetFramework => _integrationTestFrameworkDiscoverer.TargetFramework;

		public string TestFrameworkDisplayName => _integrationTestFrameworkDiscoverer.TestFrameworkDisplayName;

		public void Dispose()
		{
			_integrationTestFrameworkDiscoverer.Dispose();
			_xunitTestFrameworkDiscoverer.Dispose();
		}

		public void Find(bool includeSourceInformation, IMessageSink discoveryMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			_integrationTestFrameworkDiscoverer.Find(includeSourceInformation, new FilterableMessageSink(discoveryMessageSink, message => !(message is DiscoveryCompleteMessage)), discoveryOptions);
			_xunitTestFrameworkDiscoverer.Find(includeSourceInformation, discoveryMessageSink, discoveryOptions);
		}

		public void Find(string typeName, bool includeSourceInformation, IMessageSink discoveryMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			_integrationTestFrameworkDiscoverer.Find(typeName, includeSourceInformation, new FilterableMessageSink(discoveryMessageSink, message => !(message is DiscoveryCompleteMessage)), discoveryOptions);
			_xunitTestFrameworkDiscoverer.Find(typeName, includeSourceInformation, discoveryMessageSink, discoveryOptions);
		}

		public string Serialize(ITestCase testCase)
		{
			if (testCase is IntegrationTestCase integrationTestCase)
				return _integrationTestFrameworkDiscoverer.Serialize(integrationTestCase);

			return _xunitTestFrameworkDiscoverer.Serialize(testCase);
		}
	}
}
