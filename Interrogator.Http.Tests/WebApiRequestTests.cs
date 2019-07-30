using System;
using System.Net.Http;
using System.Threading.Tasks;
using Interrogator.Http.TestApi;
using Interrogator.xUnit;
using Xunit;

namespace Interrogator.Http.Tests
{
	public class WebApiRequestTests
	{
		[IntegrationTest]
		public Task GetWithBody([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Request/GetWithBody")
				.WithJsonBody(Constants.TestBodyJson)
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task GetWithNoBody([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Request/GetWithNoBody")
				.WithNoBody()
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task GetWithHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Request/GetWithHeader")
				.WithHeader(Constants.TestHeaderName, Constants.TestHeaderValue)
				.WithNoBody()
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task GetWithMultiHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Request/GetWithMultiHeader")
				.WithHeader(Constants.TestHeaderName, Constants.TestMultiHeaderValues)
				.WithNoBody()
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task GetWithNoHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Request/GetWithNoHeader")
				.WithNoBody()
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task PostWithBody([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Post("Request/PostWithBody")
				.WithJsonBody(Constants.TestBodyJson)
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task PostWithNoBody([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Post("Request/PostWithNoBody")
				.WithNoBody()
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task PostWithHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Post("Request/PostWithHeader")
				.WithHeader(Constants.TestHeaderName, Constants.TestHeaderValue)
				.WithNoBody()
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task PostWithMultiHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Post("Request/PostWithMultiHeader")
				.WithHeader(Constants.TestHeaderName, Constants.TestMultiHeaderValues)
				.WithNoBody()
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task PostWithNoHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Post("Request/PostWithNoHeader")
				.WithNoBody()
				.Send()
				.IsOk();
	}
}
