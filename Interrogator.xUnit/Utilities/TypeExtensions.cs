using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Interrogator.xUnit.Common;

namespace Interrogator.xUnit.Utilities
{
	internal static class TypeExtensions
	{
		public static Result<MethodInfo, string> TryGetMethod(this Type type, string methodName, Type[] parameterTypes)
			=> Option
				.FromNullable(type
					.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
					.FirstOrDefault(method => method.Name == methodName && (parameterTypes == null || method.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes)))
				)
				.ToResult(() => $"Could not find source method '{type.Name}.{methodName}({String.Join(", ", parameterTypes.Select(t => t.Name))})'.");
	}
}
