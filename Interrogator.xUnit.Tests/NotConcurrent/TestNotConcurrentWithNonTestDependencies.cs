using System.Threading.Tasks;
using Xunit;

namespace Interrogator.xUnit.Tests.NotConcurrent
{
	public class TestNotConcurrentWithNonTestDependencies
	{
		public static ConcurrencyTestLock Lock { get; } = new ConcurrencyTestLock();

		[NotConcurrent("Group1")]
		private async Task Group1_Before_Method1()
		{
			using (Lock.Acquire())
				await Task.Delay(2000);
		}

		[IntegrationTest]
		[NotConcurrent("Group1")]
		[DependsOn(nameof(Group1_Before_Method1))]
		public async Task Group1_Method1()
		{
			using (Lock.Acquire())
				await Task.Delay(2000);
		}

		[NotConcurrent("Group1")]
		private async Task Group1_Before_Method2()
		{
			using (Lock.Acquire())
				await Task.Delay(2000);
		}

		[IntegrationTest]
		[NotConcurrent("Group1")]
		[DependsOn(nameof(Group1_Before_Method2))]
		public async Task Group1_Method2()
		{
			using (Lock.Acquire())
				await Task.Delay(2000);
		}
	}
}