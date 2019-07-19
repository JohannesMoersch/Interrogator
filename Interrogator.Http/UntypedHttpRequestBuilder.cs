using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	public class UntypedHttpRequestBuilder
	{
		public HttpClient Client { get; }

		public UntypedHttpRequestBuilder(HttpClient client) 
			=> Client = client;
	}
}
