using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;

namespace Xunit.IntegrationTest.Common
{
	internal static class ReflectionExtensions
	{
		public static bool IsIntegrationTest(this IMethodInfo method)
			=> method
				.GetCustomAttributes(typeof(IntegrationTestAttribute))
				.Any();

		public static Option<FromAttribute> TryGetFromAttribute(this ParameterInfo parameter)
			=> Option
				.FromNullable(parameter
					.GetCustomAttributes<FromAttribute>()
					.FirstOrDefault()
				);
	}
}
