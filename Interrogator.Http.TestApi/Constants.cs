using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interrogator.Http.TestApi
{
	public static class Constants
	{
		public static readonly string[] TestBody = new[] { "A1", "B2", "C3" };

		public const string TestBodyJson = "[\"A1\",\"B2\",\"C3\"]";

		public const string TestHeaderName = "TestHeader";

		public const string TestHeaderValue = "TestValue";

		public static readonly string[] TestMultiHeaderValues = new[] { "One", "Two", "Three" };
	}
}
