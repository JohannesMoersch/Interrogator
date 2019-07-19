using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Interrogator.Http
{
	public static class HttpRequestBuilderWithoutBodyExtensions
	{
		public static async Task<HttpResponse> Send(this HttpRequestBuilderWithoutBody requestBuilder)
		{
			var response = await requestBuilder
				.Client
				.SendAsync(CreateRequest(requestBuilder));

			return new HttpResponse(response);
		}

		private static HttpRequestMessage CreateRequest(HttpRequestBuilderWithoutBody requestBuilder)
		{
			var request = new HttpRequestMessage(requestBuilder.Method, requestBuilder.Address);

			request.AddHeaders(requestBuilder.Headers);

			return request;
		}
	}
}
