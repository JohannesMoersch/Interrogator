using System;
using Xunit.Sdk;

namespace Interrogator.xUnit
{
	[TestFrameworkDiscoverer("Interrogator.xUnit.Infrastructure.IntegrationTestFrameworkTypeDiscoverer", "Interrogator.xUnit")]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public class UseIntegrationTestFrameworkAttribute : Attribute, ITestFrameworkAttribute
	{
	}
}
