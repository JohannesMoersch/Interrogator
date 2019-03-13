using System;
using Xunit;

namespace Xunit.IntegrationTest.Tests
{
	public class UnitTest1
	{
		[IntegrationTest]
		public static int Test1()
		{
			return 0;
		}

		[Fact]
		public static void Test2()
		{
		}

		[IntegrationTest]
		public static void Test3([From(nameof(Test1))]int stuff)
		{
		}
	}
}
