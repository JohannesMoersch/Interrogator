using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	public class HttpRequestBuilderWithBody
	{
		internal HttpClient Client { get; }

		internal HttpMethod Method { get; }

		internal IReadOnlyList<HttpHeader> Headers { get; }

		internal HttpContent Content { get; }

		internal HttpRequestBuilderWithBody(HttpClient client, HttpMethod method, IEnumerable<HttpHeader> headers, HttpContent content)
		{
			Client = client;
			Method = method;
			Headers = headers.ToArray();
			Content = content;
		}
	}
}
