using System;
using System.Net.Http;
using Xunit;

namespace Interrogator.Http.Tests
{
	public class UnitTest1
	{
		[Fact]
		public void Test1()
		{
			HttpClient a;
			a.Get().WithAuthorization("Bearer ashfjashkfa").WithJsonBody("{\"abc\": 10}");
		}
	}
}
