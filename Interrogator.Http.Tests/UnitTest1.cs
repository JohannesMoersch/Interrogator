using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Interrogator.Http.Tests
{
	public class UnitTest1
	{
		[Fact]
		public Task Test1()
			=> ((HttpClient)null)
				.BuildTest()
				.Get("/stuff/12")
				.WithHeader("Stuff", "Value")
				.WithJsonBody("Some Json")
				.Send()
				.IsOK()
				.HasHeader("stuff")
				.AssertJsonBody(json => { });
	}
}
