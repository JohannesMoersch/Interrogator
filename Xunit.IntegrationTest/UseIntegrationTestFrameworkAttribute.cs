using System;
using Xunit.Sdk;

namespace Xunit.IntegrationTest
{
	[TestFrameworkDiscoverer("Xunit.IntegrationTest.Infrastructure.IntegrationTestFrameworkTypeDiscoverer", "Xunit.IntegrationTest")]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public class UseIntegrationTestFrameworkAttribute : Attribute, ITestFrameworkAttribute
	{
	}
}
