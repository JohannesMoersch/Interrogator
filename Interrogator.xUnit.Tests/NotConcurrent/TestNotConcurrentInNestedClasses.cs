using System.Threading.Tasks;
using Xunit;

namespace Interrogator.xUnit.Tests.NotConcurrent
{
	public class TestNotConcurrentInNestedClasses
	{
		public class Outer
		{
			private static string _results = "";

			[IntegrationTest]
			[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.ClassHierarchy)]
			public Task Group1_Method1()
			{
				_results += nameof(Group1_Method1);
				return Task.Delay(2000);
			}

			[IntegrationTest]
			[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.ClassHierarchy)]
			public Task Group1_Method2()
			{
				_results += nameof(Group1_Method2);
				return Task.Delay(2000);
			}

			[IntegrationTest]
			[DependsOn(nameof(Group1_Method1))]
			[DependsOn(nameof(Group1_Method2))]
			public Task ConfirmResultCorrect()
			{
				Assert.Equal(nameof(Group1_Method1) + nameof(Group1_Method2), _results);
				return Task.CompletedTask;
			}

			public class Nested
			{
				private static string _results = "";

				[IntegrationTest]
				[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.ClassHierarchy)]
				public Task Group1_Method1()
				{
					_results += nameof(Group1_Method1);
					return Task.Delay(2000);
				}

				[IntegrationTest]
				[NotConcurrent("Group1", NotConcurrentAttribute.ConcurrencyScope.ClassHierarchy)]
				public Task Group1_Method2()
				{
					_results += nameof(Group1_Method2);
					return Task.Delay(2000);
				}

				[IntegrationTest]
				[DependsOn(nameof(Group1_Method1))]
				[DependsOn(nameof(Group1_Method2))]
				public Task ConfirmResultCorrect()
				{
					Assert.Equal(nameof(Group1_Method1) + nameof(Group1_Method2), _results);
					return Task.CompletedTask;
				}
			}
		}
	}
}