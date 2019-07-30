using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Interrogator.Http.TestApi;
using Interrogator.xUnit;
using Xunit;

namespace Interrogator.Http.Tests
{
	public class WebApiResponseTests
	{
		[IntegrationTest]
		public Task ReturnsOK([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/OK")
				.WithNoBody()
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task ReturnsCreated([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Created")
				.WithNoBody()
				.Send()
				.IsCreated();

		[IntegrationTest]
		public Task ReturnsAccepted([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Accepted")
				.WithNoBody()
				.Send()
				.IsAccepted();

		[IntegrationTest]
		public Task ReturnsBadRequest([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/BadRequest")
				.WithNoBody()
				.Send()
				.IsBadRequest();

		[IntegrationTest]
		public Task ReturnsUnauthorized([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Unauthorized")
				.WithNoBody()
				.Send()
				.IsUnauthorized();

		[IntegrationTest]
		public Task ReturnsForbidden([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Forbidden")
				.WithNoBody()
				.Send()
				.IsForbidden();

		[IntegrationTest]
		public Task ReturnsNotFound([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/NotFound")
				.WithNoBody()
				.Send()
				.IsNotFound();

		[IntegrationTest]
		public Task ReturnsConflict([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Conflict")
				.WithNoBody()
				.Send()
				.IsConflict();

		[IntegrationTest]
		public Task ReturnsSuccess([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Success")
				.WithNoBody()
				.Send()
				.IsSuccess();

		[IntegrationTest]
		public Task Returns3xx([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/3xx")
				.WithNoBody()
				.Send()
				.Is3xx();

		[IntegrationTest]
		public Task Returns4xx([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/4xx")
				.WithNoBody()
				.Send()
				.Is4xx();

		[IntegrationTest]
		public Task Returns5xx([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/5xx")
				.WithNoBody()
				.Send()
				.Is5xx();

		[IntegrationTest]
		public Task ReturnsBody([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Body")
				.WithNoBody()
				.Send()
				.IsOk()
				.AssertJsonBody(json => json.Should().Be(Constants.TestBodyJson));

		[IntegrationTest]
		public Task ReturnsNoBody([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/NoBody")
				.WithNoBody()
				.Send()
				.IsOk()
				.DoesNotHaveBody();

		[IntegrationTest]
		public Task ReturnsHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Header")
				.WithNoBody()
				.Send()
				.IsOk()
				.HasHeaderValues(Constants.TestHeaderName, Constants.TestHeaderValue);

		[IntegrationTest]
		public Task ReturnsMultiHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/MultiHeader")
				.WithNoBody()
				.Send()
				.IsOk()
				.HasHeaderValues(Constants.TestHeaderName, Constants.TestMultiHeaderValues);

		[IntegrationTest]
		public Task ReturnsNoHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/NoHeader")
				.WithNoBody()
				.Send()
				.IsOk()
				.DoesNotHaveHeader(Constants.TestHeaderName);
	}
}
