using System.Threading.Tasks;
using Xunit;

namespace Interrogator.xUnit.Tests.NotConcurrent
{
	public class TestNotConcurrentInClass
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
		public async Task Group1_Method2()
		{
			using (Lock.Acquire())
				await Task.Delay(2000);
		}
	}
}