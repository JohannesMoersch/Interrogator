using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.Http
{
	public struct HttpHeader
	{
		public string Name { get; }

		public IReadOnlyList<string> Values { get; }

		internal HttpHeader(string name, string[] value)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Values = value ?? throw new ArgumentNullException(nameof(value));
		}
	}
}
