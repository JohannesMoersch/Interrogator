using System;
using System.Threading.Tasks;
using Xunit;

namespace Interrogator.xUnit.Tests.NotConcurrent
{
	public class TestNotConcurrentContinuesOnFailure
	{
		[IntegrationTest]
		[NotConcurrent("Group1")]
		public void Group1_Method1_ShouldFail() 
			=> throw new Exception();

		[IntegrationTest]
		[NotConcurrent("Group1")]
		public void Group1_Method2()
		{
		}

		[IntegrationTest]
		[NotConcurrent("Group1")]
		public void Group1_Method3_ShouldFail() 
			=> throw new Exception();
	}
}