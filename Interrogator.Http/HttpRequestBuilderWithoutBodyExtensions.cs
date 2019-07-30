using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Interrogator.Http
{
	public static class HttpRequestBuilderWithoutBodyExtensions
	{
		public static Task<HttpResponse> Send(this HttpRequestBuilderWithoutBody requestBuilder)
			=> requestBuilder
				.Client
				.Send(CreateRequest(requestBuilder));

		private static HttpRequestMessage CreateRequest(HttpRequestBuilderWithoutBody requestBuilder)
		{
			var request = new HttpRequestMessage(requestBuilder.Method, requestBuilder.Address);

			request.AddHeaders(requestBuilder.Headers);

			return request;
		}
	}
}
