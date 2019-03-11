using System;
using Xunit.Sdk;

namespace Xunit.IntegrationTest
{
	[TestFrameworkDiscoverer("Xunit.IntegrationTest.IntegrationTestFrameworkTypeDiscoverer", "Xunit.IntegrationTest")]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public class UseIntegrationTestFrameworkAttribute : Attribute, ITestFrameworkAttribute
	{
	}
}
