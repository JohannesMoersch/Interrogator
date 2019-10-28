using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interrogator.xUnit.Utilities
{
	internal static class HashSetExtensions
	{
		public static bool AddSet<T>(this HashSet<T> hashSet, IEnumerable<T> values)
		{
			if (values.Any(v => hashSet.Contains(v)))
				return false;

			foreach (var value in values)
				hashSet.Add(value);

			return true;
		}

		public static void RemoveSet<T>(this HashSet<T> hashSet, IEnumerable<T> values)
		{
			foreach (var value in values)
				hashSet.Remove(value);
		}
	}
}
