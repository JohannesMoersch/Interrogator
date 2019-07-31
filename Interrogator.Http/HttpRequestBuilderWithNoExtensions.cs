using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Interrogator.Http
{
	public static class HttpRequestBuilderWithNoBodyExtensions
	{
		public static Task<HttpResponse> Send(this HttpRequestBuilderWithNoBody requestBuilder)
			=> requestBuilder
				.Client
				.Send(CreateRequest(requestBuilder));

		private static HttpRequestMessage CreateRequest(HttpRequestBuilderWithNoBody requestBuilder)
		{
			var request = new HttpRequestMessage(requestBuilder.Method, requestBuilder.Address);

			request.AddHeaders(requestBuilder.Headers);

			return request;
		}
	}
}
