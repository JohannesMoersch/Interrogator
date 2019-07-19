using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Interrogator.Http
{
	public static class HttpRequestBuilderWithBodyExtensions
	{
		public static async Task<HttpResponse> Send(this HttpRequestBuilderWithBody requestBuilder)
		{
			var response = await requestBuilder
				.Client
				.SendAsync(CreateRequest(requestBuilder));

			return new HttpResponse(response);
		}

		private static HttpRequestMessage CreateRequest(HttpRequestBuilderWithBody requestBuilder)
		{
			var request = new HttpRequestMessage(requestBuilder.Method, requestBuilder.Address);

			request.AddHeaders(requestBuilder.Headers);

			request.Content = request.Content;

			return request;
		}
	}
}
