using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Interrogator.Http
{
	public class HttpAssertionException : Exception
	{
		internal HttpAssertionException(string message)
			: base(message)
		{
		}

		internal async Task<HttpAssertionException> AddResponseBody(HttpContent content)
			=> new HttpAssertionException(Message + $"\n\nResponse body:\n{await content.ReadAsStringAsync()}");
	}
}
