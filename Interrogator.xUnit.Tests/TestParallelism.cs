using System.Threading;
using System.Threading.Tasks;

namespace Interrogator.xUnit.Tests
{
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