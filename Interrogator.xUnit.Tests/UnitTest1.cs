using System;
using System.Threading;
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
	}

	public class TestParallelism
	{
		[IntegrationTest]
		public bool SyncSource()
		{
			Thread.Sleep(2000);

			return true;
		}

		[IntegrationTest]
		public async Task<bool> AsyncSource()
		{
			await Task.Delay(2000);

			return true;
		}

		[IntegrationTest]
		public void SyncTest1([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Thread.Sleep(2000);

		[IntegrationTest]
		public void SyncTest2([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Thread.Sleep(2000);

		[IntegrationTest]
		public void SyncTest3([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Thread.Sleep(2000);

		[IntegrationTest]
		public void SyncTest4([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Thread.Sleep(2000);

		[IntegrationTest]
		public void SyncTest5([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Thread.Sleep(2000);

		[IntegrationTest]
		public void SyncTest6([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Thread.Sleep(2000);

		[IntegrationTest]
		public void SyncTest7([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Thread.Sleep(2000);

		[IntegrationTest]
		public void SyncTest8([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Thread.Sleep(2000);

		[IntegrationTest]
		public Task AsyncTest1([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Task.Delay(2000);

		[IntegrationTest]
		public Task AsyncTest2([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Task.Delay(2000);

		[IntegrationTest]
		public Task AsyncTest3([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Task.Delay(2000);

		[IntegrationTest]
		public Task AsyncTest4([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Task.Delay(2000);

		[IntegrationTest]
		public Task AsyncTest5([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Task.Delay(2000);

		[IntegrationTest]
		public Task AsyncTest6([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Task.Delay(2000);

		[IntegrationTest]
		public Task AsyncTest7([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Task.Delay(2000);

		[IntegrationTest]
		public Task AsyncTest8([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> Task.Delay(2000);
	}
}
