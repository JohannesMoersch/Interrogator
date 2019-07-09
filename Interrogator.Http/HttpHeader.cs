using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.Http
{
	internal struct HttpHeader
	{
		public string Header { get; }

		public string Value { get; }

		public HttpHeader(string header, string value)
		{
			Header = header;
			Value = value;
		}
	}
}
