using System;
using System.Threading.Tasks;
using Xunit;

namespace Xunit.IntegrationTest.Tests
{
	public class UnitTest1
	{
		private static int Source()
			=> 200;

		[IntegrationTest]
		public static async Task<int> Test1([From(nameof(Source))]int source)
		{
			await Task.Delay(source);
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
