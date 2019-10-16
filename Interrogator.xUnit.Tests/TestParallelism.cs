using System;
using System.Threading;
using System.Threading.Tasks;

namespace Interrogator.xUnit.Tests
{
	public class TestParallelism
	{
		private static int _concurrencyCount;

		private static bool _executionIsConcurrent;

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
			=> SyncTest();

		[IntegrationTest]
		public void SyncTest2([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> SyncTest();

		[IntegrationTest]
		public void SyncTest3([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> SyncTest();

		[IntegrationTest]
		public void SyncTest4([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> SyncTest();

		[IntegrationTest]
		public void SyncTest5([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> SyncTest();

		[IntegrationTest]
		public void SyncTest6([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> SyncTest();

		[IntegrationTest]
		public void SyncTest7([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> SyncTest();

		[IntegrationTest]
		public void SyncTest8([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> SyncTest();

		private void SyncTest()
		{
			if (Interlocked.Increment(ref _concurrencyCount) > 1)
				_executionIsConcurrent = true;

			Thread.Sleep(2000);

			Interlocked.Decrement(ref _concurrencyCount);
		}

		[IntegrationTest]
		public Task AsyncTest1([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> AsyncTest();

		[IntegrationTest]
		public Task AsyncTest2([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> AsyncTest();

		[IntegrationTest]
		public Task AsyncTest3([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> AsyncTest();

		[IntegrationTest]
		public Task AsyncTest4([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> AsyncTest();

		[IntegrationTest]
		public Task AsyncTest5([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> AsyncTest();

		[IntegrationTest]
		public Task AsyncTest6([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> AsyncTest();

		[IntegrationTest]
		public Task AsyncTest7([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> AsyncTest();

		[IntegrationTest]
		public Task AsyncTest8([From(nameof(SyncSource))]bool sync, [From(nameof(AsyncSource))]bool async)
			=> AsyncTest();

		private async Task AsyncTest()
		{
			if (Interlocked.Increment(ref _concurrencyCount) > 1)
				_executionIsConcurrent = true;

			await Task.Delay(2000);

			Interlocked.Decrement(ref _concurrencyCount);
		}

		[IntegrationTest]
		[DependsOn(nameof(SyncTest1))]
		[DependsOn(nameof(SyncTest2))]
		[DependsOn(nameof(SyncTest3))]
		[DependsOn(nameof(SyncTest4))]
		[DependsOn(nameof(SyncTest5))]
		[DependsOn(nameof(SyncTest6))]
		[DependsOn(nameof(SyncTest7))]
		[DependsOn(nameof(SyncTest8))]
		[DependsOn(nameof(AsyncTest1))]
		[DependsOn(nameof(AsyncTest2))]
		[DependsOn(nameof(AsyncTest3))]
		[DependsOn(nameof(AsyncTest4))]
		[DependsOn(nameof(AsyncTest5))]
		[DependsOn(nameof(AsyncTest6))]
		[DependsOn(nameof(AsyncTest7))]
		[DependsOn(nameof(AsyncTest8))]
		public void TestParallelismEnabledInRunner()
		{
			if (!_executionIsConcurrent)
				throw new Exception("Please enable test parallelism.");
		}
	}
}