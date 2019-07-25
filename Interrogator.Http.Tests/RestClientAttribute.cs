using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using Interrogator.Http.TestApi;
using Interrogator.xUnit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Interrogator.Http.Tests
{
	public class RestClientAttribute : FromAttribute
	{
		public RestClientAttribute()
			: base(typeof(RestClientAttribute), nameof(GetHttpClient))
		{
		}

		private static readonly WebApplicationFactory<Startup> _clientFactory = new WebApplicationFactory<Startup>();

		private static readonly Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(CreateHttpClient, LazyThreadSafetyMode.ExecutionAndPublication);

		private static HttpClient GetHttpClient()
			=> _httpClient.Value;

		private static HttpClient CreateHttpClient()
			=> _clientFactory.CreateClient();
	}
}
