using System.Threading.Tasks;
using Xunit;

namespace Interrogator.xUnit.Tests.NotConcurrent
{
	public class WrappingTestClass
	{
		public static ConcurrencyTestLock Lock { get; } = new ConcurrencyTestLock();

		public class TestNotConcurrentInNamespace
		{
			[IntegrationTest]
			[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.Namespace)]
			public async Task Group1_Method1()
			{
				using (Lock.Acquire())
					await Task.Delay(2000);
			}

			[IntegrationTest]
			[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.Namespace)]
			public async Task Group1_Method2()
			{
				using (Lock.Acquire())
					await Task.Delay(2000);
			}
		}

		public class TestNotConcurrentInNamespace2
		{
			[IntegrationTest]
			[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.Namespace)]
			public async Task Group1_Method3()
			{
				using (Lock.Acquire())
					await Task.Delay(2000);
			}

			[IntegrationTest]
			[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.Namespace)]
			public async Task Group1_Method4()
			{
				using (Lock.Acquire())
					await Task.Delay(2000);
			}
		}
	}
}