using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	public class HttpResponse
	{
		public HttpStatusCode StatusCode { get; }

		public IReadOnlyList<HttpHeader> Headers { get; }

		public HttpContent Content { get; }

		public TimeSpan RequestDuration { get; }

		internal HttpResponse(HttpStatusCode statusCode, IEnumerable<HttpHeader> headers, HttpContent content, TimeSpan requestDuration)
		{
			StatusCode = statusCode;
			Headers = headers.ToArray();
			Content = content;
			RequestDuration = requestDuration;
		}
	}
}
