using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	public static class HttpClientExtensions
	{
		public static UntypedHttpRequestBuilder BuildTest(this HttpClient client)
			=> new UntypedHttpRequestBuilder(client);
	}
}
