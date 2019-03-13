using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Infrastructure
{
	internal class IntegrationTestFrameworkTypeDiscoverer : ITestFrameworkTypeDiscoverer
	{
		public Type GetTestFrameworkType(IAttributeInfo attribute)
			=> typeof(IntegrationTestFramework);
	}
}
