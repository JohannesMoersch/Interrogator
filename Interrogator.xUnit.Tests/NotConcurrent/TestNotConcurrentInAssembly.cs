using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace Interrogator.xUnit.Tests.NotConcurrent
{
	public class TestNotConcurrentInAssembly
	{
		public static ConcurrencyTestLock Lock { get; } = new ConcurrencyTestLock();

		[IntegrationTest]
		[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.Assembly)]
		public async Task Group1_Method1()
		{
			using (Lock.Acquire())
				await Task.Delay(2000);
		}

		[IntegrationTest]
		[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.Assembly)]
		public async Task Group1_Method2()
		{
			using (Lock.Acquire())
				await Task.Delay(2000);
		}
	}
}

namespace Interrogator.xUnit.Tests.NotConcurrent2
{
	public class TestNotConcurrentInAssembly
	{
		[IntegrationTest]
		[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.Assembly)]
		public async Task Group1_Method3()
		{
			using (NotConcurrent.TestNotConcurrentInAssembly.Lock.Acquire())
				await Task.Delay(2000);
		}

		[IntegrationTest]
		[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.Assembly)]
		public async Task Group1_Method4()
		{
			using (NotConcurrent.TestNotConcurrentInAssembly.Lock.Acquire())
				await Task.Delay(2000);
		}
	}
}