using System;
using System.Threading.Tasks;
using Xunit;

namespace Xunit.IntegrationTest.Tests
{
	public class UnitTest1
	{
		[IntegrationTest]
		public static async Task<int> Test1()
		{
			await Task.Delay(200);
			return 1;
		}

		[Fact]
		public static void Test2()
		{
		}

		[IntegrationTest]
		public static void Test3([From(nameof(Test1))]int stuff)
		{
		}
	}
}
