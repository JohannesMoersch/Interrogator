using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.Http
{
	internal static class StringExtensions
	{
		public static string WrapInQuotesOrStringNull(this string value)
			=> value != null ? $"\"{value}\"" : "null";
	}
}
