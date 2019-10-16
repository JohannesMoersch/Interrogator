using System;
using System.Threading;
using System.Threading.Tasks;

namespace Interrogator.xUnit.Tests
{
	public class MiscellaneousTests
	{
		private readonly string _message;

		public MiscellaneousTests([From(typeof(Stuff), nameof(Stuff.Message))]string message)
			=> _message = message;

		private int Source()
			=> 1000;

		[IntegrationTest]
		public async Task<int> Test1([From(nameof(Source))]int source)
		{
			await Task.Delay(source);
			return 1;
		}

		[IntegrationTest]
		[DependsOn(nameof(Test1), ContinueOnDependencyFailure = true)]
		public void Blah()
			=> Thread.Sleep(2000);

		[IntegrationTest]
		[DependsOn(nameof(Blah))]
		public void Things()
		{
		}

		[IntegrationTest]
		public static float Test2([From(nameof(Test3))] int test)
			=> 1.0f;

		[IntegrationTest]
		public static int Test3([From(nameof(Test1))]int stuff)
		{
			return 0;
		}

		[IntegrationTest]
		public static Task TestTask()
			=> Task.CompletedTask;

		[IntegrationTest]
		public static void DependsOnTask_ShouldFail([From(nameof(TestTask))]int input)
		{
		}

		[IntegrationTest]
		public static void TestVoid()
		{
		}

		[IntegrationTest]
		public static void DependsOnVoid_ShouldFail([From(nameof(TestVoid))]int input)
		{
		}
	}
}
