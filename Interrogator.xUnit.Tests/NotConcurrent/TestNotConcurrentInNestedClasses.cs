using System.Threading.Tasks;
using Xunit;

namespace Interrogator.xUnit.Tests.NotConcurrent
{
	public class TestNotConcurrentInNestedClasses
	{
		public static ConcurrencyTestLock Lock { get; } = new ConcurrencyTestLock();

		public class Outer
		{
			[IntegrationTest]
			[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.ClassHierarchy)]
			public async Task Group1_Method1()
			{
				using (Lock.Acquire())
					await Task.Delay(2000);
			}

			[IntegrationTest]
			[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.ClassHierarchy)]
			public async Task Group1_Method2()
			{
				using (Lock.Acquire())
					await Task.Delay(2000);
			}

			public class Nested
			{
				[IntegrationTest]
				[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.ClassHierarchy)]
				public async Task Group1_Method1()
				{
					using (Lock.Acquire())
						await Task.Delay(2000);
				}

				[IntegrationTest]
				[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.ClassHierarchy)]
				public async Task Group1_Method2()
				{
					using (Lock.Acquire())
						await Task.Delay(2000);
				}
			}
		}
	}
}