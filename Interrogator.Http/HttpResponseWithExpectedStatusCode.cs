using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	public class HttpResponseWithExpectedStatusCode
	{
		public IReadOnlyList<HttpHeader> Headers { get; }

		public HttpContent Content { get; }

		public TimeSpan RequestDuration { get; }

		internal HttpResponseWithExpectedStatusCode(IEnumerable<HttpHeader> headers, HttpContent content, TimeSpan requestDuration)
		{
			Headers = headers.ToArray();
			Content = content;
			RequestDuration = requestDuration;
		}
	}
}
