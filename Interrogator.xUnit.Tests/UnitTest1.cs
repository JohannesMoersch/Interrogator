using System;
using System.Threading.Tasks;
using Xunit;

namespace Interrogator.xUnit.Tests
{
	public class Stuff
	{
		public async Task<string> Message()
		{
			await Task.Delay(500);

			return "abc123";
		}
	}

	public class SkipTestAttribute : IntegrationTestAttribute
	{
		public SkipTestAttribute()
		{
			Skip = "Skip This Test";
		}
	}

	public class UnitTest1
	{
		private readonly string _message;

		public UnitTest1([From(typeof(Stuff), nameof(Stuff.Message))]string message)
			=> _message = message;

		private int Source()
			=> 10;

		[SkipTest]
		public async Task<int> Test1([From(nameof(Source))]int source)
		{
			await Task.Delay(source);
			return 1;
		}

		[IntegrationTest]
		public void Blah([From(nameof(Test1))]int blah)
		{
		}

		[IntegrationTest]
		public static float Test2([From(nameof(Test3))] int test)
			=> throw new Exception("!!!");

		[IntegrationTest]
		public static int Test3([From(nameof(Test1))]int stuff, [From(nameof(Test2))]float things)
		{
			return 0;
		}
	}
}
