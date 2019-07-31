using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Interrogator.Http
{
	public static class HttpRequestBuilderWithoutBodySupportExtensions
	{
		public static HttpRequestBuilderWithoutBodySupport WithHeader(this HttpRequestBuilderWithoutBodySupport requestBuilder, string name, params string[] values)
			=> new HttpRequestBuilderWithoutBodySupport(requestBuilder.Client, requestBuilder.Method, requestBuilder.Address, requestBuilder.Headers.Append(new HttpHeader(name, values)));

		public static HttpRequestBuilderWithoutBodySupport WithAuthorization(this HttpRequestBuilderWithoutBodySupport requestBuilder, string value)
			=> requestBuilder.WithHeader("Authorization", value);

		public static HttpRequestBuilderWithoutBodySupport WithAcceptEncoding(this HttpRequestBuilderWithoutBodySupport requestBuilder, params string[] values)
			=> requestBuilder.WithHeader("Accept-Encoding", values);

		public static HttpRequestBuilderWithoutBodySupport WithAcceptLanguage(this HttpRequestBuilderWithoutBodySupport requestBuilder, params string[] values)
			=> requestBuilder.WithHeader("Accept-Language", values);

		public static Task<HttpResponse> Send(this HttpRequestBuilderWithoutBodySupport requestBuilder)
			=> requestBuilder
				.Client
				.Send(CreateRequest(requestBuilder));

		private static HttpRequestMessage CreateRequest(HttpRequestBuilderWithoutBodySupport requestBuilder)
		{
			var request = new HttpRequestMessage(requestBuilder.Method, requestBuilder.Address);

			request.AddHeaders(requestBuilder.Headers);

			return request;
		}
	}
}
