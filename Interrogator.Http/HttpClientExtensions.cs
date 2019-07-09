using System;
using System.Linq;
using System.Net.Http;

namespace Interrogator.Http
{
	public static class HttpClientExtensions
	{
		public static HttpRequestBuilder Get(this HttpClient httpClient)
			=> new HttpRequestBuilder(httpClient, HttpMethod.Get, Enumerable.Empty<HttpHeader>());

		public static HttpRequestBuilder Post(this HttpClient httpClient)
			=> new HttpRequestBuilder(httpClient, HttpMethod.Post, Enumerable.Empty<HttpHeader>());

		public static HttpRequestBuilder Put(this HttpClient httpClient)
			=> new HttpRequestBuilder(httpClient, HttpMethod.Put, Enumerable.Empty<HttpHeader>());

		public static HttpRequestBuilder Patch(this HttpClient httpClient)
			=> new HttpRequestBuilder(httpClient, HttpMethod.Patch, Enumerable.Empty<HttpHeader>());

		public static HttpRequestBuilder Delete(this HttpClient httpClient)
			=> new HttpRequestBuilder(httpClient, HttpMethod.Delete, Enumerable.Empty<HttpHeader>());
	}
}
