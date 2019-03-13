using System;
using Xunit;

namespace Xunit.IntegrationTest.Tests
{
	public class UnitTest1
	{
		[IntegrationTest]
		public static void Test1()
		{
		}

		[Fact]
		public static void Test2()
		{
		}

		[IntegrationTest]
		public static void Test3(int stuff)
		{
		}
	}
}
