using System;
using System.Net.Http;
using System.Threading.Tasks;
using Interrogator.xUnit;
using Xunit;

namespace Interrogator.Http.Tests
{
	public class WebApiTests
	{
		[IntegrationTest]
		public Task Test1([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("GetOkWithNoBody")
				.WithNoBody()
				.Send()
				.IsOK()
				.AssertBody(content => Task.CompletedTask);
	}
}
