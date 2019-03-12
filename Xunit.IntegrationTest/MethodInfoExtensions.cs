using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace Xunit.IntegrationTest
{
	internal static class MethodInfoExtensions
	{
		public static bool IsIntegrationTest(this IMethodInfo method)
			=> method.GetCustomAttributes(typeof(IntegrationTestAttribute)).Any();
	}
}
