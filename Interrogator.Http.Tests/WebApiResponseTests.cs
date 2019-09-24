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
				.Send()
				.IsOk();

		[IntegrationTest]
		public Task ReturnsCreated([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Created")
				.Send()
				.IsCreated();

		[IntegrationTest]
		public Task ReturnsAccepted([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Accepted")
				.Send()
				.IsAccepted();

		[IntegrationTest]
		public Task ReturnsBadRequest([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/BadRequest")
				.Send()
				.IsBadRequest();

		[IntegrationTest]
		public Task ReturnsUnauthorized([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Unauthorized")
				.Send()
				.IsUnauthorized();

		[IntegrationTest]
		public Task ReturnsForbidden([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Forbidden")
				.Send()
				.IsForbidden();

		[IntegrationTest]
		public Task ReturnsNotFound([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/NotFound")
				.Send()
				.IsNotFound();

		[IntegrationTest]
		public Task ReturnsConflict([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Conflict")
				.Send()
				.IsConflict();

		[IntegrationTest]
		public Task ReturnsSuccess([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Success")
				.Send()
				.IsSuccess();

		[IntegrationTest]
		public Task Returns3xx([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/3xx")
				.Send()
				.Is3xx();

		[IntegrationTest]
		public Task Returns4xx([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/4xx")
				.Send()
				.Is4xx();

		[IntegrationTest]
		public Task Returns5xx([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/5xx")
				.Send()
				.Is5xx();

		[IntegrationTest]
		public Task ReturnsBody([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Body")
				.Send()
				.IsOk()
				.AssertJsonBody(json => json.Should().Be(Constants.TestBodyJson))
				.ReturnFromBody(_ => _)
				.Should()
				.Be(Constants.TestBodyJson);

		[IntegrationTest]
		public Task ReturnsNoBody([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/NoBody")
				.Send()
				.IsOk()
				.DoesNotHaveBody();

		[IntegrationTest]
		public Task ReturnsHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/Header")
				.Send()
				.IsOk()
				.HasHeaderValues(Constants.TestHeaderName, Constants.TestHeaderValue);

		[IntegrationTest]
		public Task ReturnsMultiHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/MultiHeader")
				.Send()
				.IsOk()
				.HasHeaderValues(Constants.TestHeaderName, Constants.TestMultiHeaderValues);

		[IntegrationTest]
		public Task ReturnsNoHeader([RestClient]HttpClient httpClient)
			=> httpClient
				.BuildTest()
				.Get("Response/NoHeader")
				.Send()
				.IsOk()
				.DoesNotHaveHeader(Constants.TestHeaderName);
	}
}
