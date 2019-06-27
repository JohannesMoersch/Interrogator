using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;

namespace Interrogator.xUnit.Common
{
	internal static class ReflectionExtensions
	{
		public static bool IsIntegrationTest(this IMethodInfo method)
			=> method
				.GetCustomAttributes(typeof(IntegrationTestAttribute))
				.Any();

		public static bool TryGetSkipReason(this MethodInfo method, out string skipReason)
		{
			var attribute = method
				.GetCustomAttributes(typeof(IntegrationTestAttribute))
				.Cast<IntegrationTestAttribute>()
				.FirstOrDefault(att => !String.IsNullOrEmpty(att.Skip));

			if (attribute != null)
			{
				skipReason = attribute.Skip;
				return true;
			}

			skipReason = null;
			return false;
		}

		public static Result<FromAttribute, string> GetFromAttribute(this ParameterInfo parameter)
			=> Option
				.FromNullable(parameter
					.GetCustomAttributes<FromAttribute>()
					.FirstOrDefault()
				)
				.ToResult(() => $"Parameter '{parameter.Name}' on test method '{parameter.Member.DeclaringType.Name}.{parameter.Member.Name}' requires a [From] attribute.");
	}
}
