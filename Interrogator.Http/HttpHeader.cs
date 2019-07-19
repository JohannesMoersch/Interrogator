using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.Http
{
	public struct HttpHeader
	{
		public string Name { get; }

		public string Value { get; }

		public HttpHeader(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}
}
