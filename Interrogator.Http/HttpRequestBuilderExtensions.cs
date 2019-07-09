using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Interrogator.Http
{
	public static class HttpRequestBuilderExtensions
	{
		public static HttpRequestBuilder WithHeader(this HttpRequestBuilder requestBuilder, string header, string value)
			=> new HttpRequestBuilder(requestBuilder.Client, requestBuilder.Method, requestBuilder.Headers.Append(new HttpHeader(header, value)));

		public static HttpRequestBuilder WithAuthorization(this HttpRequestBuilder requestBuilder, string value)
			=> requestBuilder.WithHeader("Authorization", value);

		public static HttpRequestBuilder WithAcceptEncoding(this HttpRequestBuilder requestBuilder, string value)
			=> requestBuilder.WithHeader("Accept-Encoding", value);

		public static HttpRequestBuilder WithAcceptLanguage(this HttpRequestBuilder requestBuilder, string value)
			=> requestBuilder.WithHeader("Accept-Language", value);

		public static HttpRequestBuilderWithBody WithBody(this HttpRequestBuilder requestBuilder, HttpContent content)
			=> new HttpRequestBuilderWithBody(requestBuilder.Client, requestBuilder.Method, requestBuilder.Headers, content);

		public static HttpRequestBuilderWithBody WithStringBody(this HttpRequestBuilder requestBuilder, string content, string contentType)
		{
			var stringContent = new StringContent(content);

			stringContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

			return new HttpRequestBuilderWithBody(requestBuilder.Client, requestBuilder.Method, requestBuilder.Headers, stringContent);
		}

		public static HttpRequestBuilderWithBody WithJsonBody(this HttpRequestBuilder requestBuilder, string json)
			=> requestBuilder.WithStringBody(json, "application/json");
	}
}
