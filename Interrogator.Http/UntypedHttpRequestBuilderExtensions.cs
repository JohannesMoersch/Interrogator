using System;
using System.Linq;
using System.Net.Http;

namespace Interrogator.Http
{
	public static class UntypedHttpRequestBuilderExtensions
	{
		public static HttpRequestBuilder Get(this UntypedHttpRequestBuilder requestBuilder, string address)
			=> new HttpRequestBuilder(requestBuilder.Client, HttpMethod.Get, address, Enumerable.Empty<HttpHeader>());

		public static HttpRequestBuilder Post(this UntypedHttpRequestBuilder requestBuilder, string address)
			=> new HttpRequestBuilder(requestBuilder.Client, HttpMethod.Post, address, Enumerable.Empty<HttpHeader>());

		public static HttpRequestBuilder Put(this UntypedHttpRequestBuilder requestBuilder, string address)
			=> new HttpRequestBuilder(requestBuilder.Client, HttpMethod.Put, address, Enumerable.Empty<HttpHeader>());

		public static HttpRequestBuilder Delete(this UntypedHttpRequestBuilder requestBuilder, string address)
			=> new HttpRequestBuilder(requestBuilder.Client, HttpMethod.Delete, address, Enumerable.Empty<HttpHeader>());

		public static HttpRequestBuilder Custom(this UntypedHttpRequestBuilder requestBuilder, string method, string address)
			=> new HttpRequestBuilder(requestBuilder.Client, new HttpMethod(method), address, Enumerable.Empty<HttpHeader>());
	}
}
