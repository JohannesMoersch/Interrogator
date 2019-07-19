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
			((HttpClient)null)
				.Build()
				.Get("/stuff/12")
				.WithHeader("Stuff", "Value")
				.WithJsonBody("Some Json")
				.Send()
				.IsOK();
				
		}
	}
}
