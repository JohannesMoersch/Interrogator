using System;
using System.Threading.Tasks;
using Xunit;

namespace Xunit.IntegrationTest.Tests
{
	public class UnitTest1
	{
		private static int Source()
			=> throw new Exception("Test Exception");

		[IntegrationTest]
		public static async Task<int> Test1([From(nameof(Source))]int source)
		{
			await Task.Delay(source);
			return 1;
		}

		[IntegrationTest]
		public static float Test2()
			=> throw new Exception("!!!");

		[IntegrationTest]
		public static void Test3([From(nameof(Test1))]int stuff, [From(nameof(Test2))]float things)
		{
		}
	}
}
