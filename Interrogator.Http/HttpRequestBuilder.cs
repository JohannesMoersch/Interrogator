using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	public class HttpRequestBuilder
	{
		internal HttpClient Client { get; }

		internal HttpMethod Method { get; }

		internal IReadOnlyList<HttpHeader> Headers { get; }

		internal HttpRequestBuilder(HttpClient client, HttpMethod method, IEnumerable<HttpHeader> headers)
		{
			Client = client;
			Method = method;
			Headers = headers.ToArray();
		}
	}
}
