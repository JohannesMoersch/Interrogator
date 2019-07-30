using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interrogator.Http
{
	internal static class StringEnumerableExtensions
	{
		public static string ToStringValueList(this IReadOnlyList<string> strings)
		{
			if (strings.Count == 0)
				return String.Empty;

			if (strings.Count == 1)
				return strings[0].WrapInQuotesOrStringNull() ?? String.Empty;

			if (strings.Count == 2)
				return $"{strings[0].WrapInQuotesOrStringNull()} and {strings[1].WrapInQuotesOrStringNull()}";

			return $"{String.Join(", ", strings.Take(strings.Count - 1).Select(s => s.WrapInQuotesOrStringNull()))}, and {strings[strings.Count - 1].WrapInQuotesOrStringNull()}";
		}
	}
}
