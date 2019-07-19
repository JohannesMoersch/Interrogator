using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	public class HttpResponse
	{
		private readonly HttpResponseMessage _response;

		public HttpResponse(HttpResponseMessage response) 
			=> _response = response;
	}
}
