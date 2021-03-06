﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	public class HttpRequestBuilderWithoutBodySupport
	{
		public HttpClient Client { get; }

		public HttpMethod Method { get; }

		public string Address { get; }

		public IReadOnlyList<HttpHeader> Headers { get; }

		internal HttpRequestBuilderWithoutBodySupport(HttpClient client, HttpMethod method, string address, IEnumerable<HttpHeader> headers)
		{
			Client = client;
			Method = method;
			Address = address;
			Headers = headers.ToArray();
		}
	}
}
