using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	public class HttpRequestBuilderWithBody
	{
		public HttpClient Client { get; }

		public HttpMethod Method { get; }

		public string Address { get; }

		public IReadOnlyList<HttpHeader> Headers { get; }

		public HttpContent Content { get; }

		internal HttpRequestBuilderWithBody(HttpClient client, HttpMethod method, string address, IEnumerable<HttpHeader> headers, HttpContent content)
		{
			Client = client;
			Method = method;
			Address = address;
			Headers = headers.ToArray();
			Content = content;
		}
	}
}
