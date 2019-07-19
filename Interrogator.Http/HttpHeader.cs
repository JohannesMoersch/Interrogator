using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.Http
{
	public struct HttpHeader
	{
		public string Name { get; }

		public IReadOnlyList<string> Value { get; }

		internal HttpHeader(string name, string[] value)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}
	}
}
