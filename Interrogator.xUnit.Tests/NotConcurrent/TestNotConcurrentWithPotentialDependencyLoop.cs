using System.Threading.Tasks;
using Xunit;

namespace Interrogator.xUnit.Tests.NotConcurrent
{
	public class TestNotConcurrentWithPotentialDependencyLoop
	{
		public static ConcurrencyTestLock Lock { get; } = new ConcurrencyTestLock();

		[IntegrationTest]
		[NotConcurrent("Group1")]
		public async Task Group1_Method1()
		{
			using (Lock.Acquire())
				await Task.Delay(2000);
		}

		[IntegrationTest]
		[NotConcurrent("Group1")]
		[DependsOn(nameof(Group1_Method1))]
		[DependsOn(nameof(Group1_Method3))]
		public async Task Group1_Method2()
		{
			using (Lock.Acquire())
				await Task.Delay(2000);
		}

		[IntegrationTest]
		[NotConcurrent("Group1")]
		public async Task Group1_Method3()
		{
			using (Lock.Acquire())
				await Task.Delay(2000);
		}
	}
}