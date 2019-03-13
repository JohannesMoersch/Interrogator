using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.IntegrationTest.Common;

namespace Xunit.IntegrationTest
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public class FromAttribute : Attribute
	{
		private readonly Type _type;

		private readonly string _methodName;

		private Type[] _parameterTypes;

		public FromAttribute(string methodName) 
			=> _methodName = methodName ?? String.Empty;

		public FromAttribute(Type type, string methodName)
			: this(methodName)
			=> _type = type;

		public FromAttribute(string methodName, params Type[] parameterTypes)
			: this(methodName)
			=> _parameterTypes = parameterTypes ?? Array.Empty<Type>();

		public FromAttribute(Type type, string methodName, params Type[] parameterTypes)
			: this(type, methodName)
			=> _parameterTypes = parameterTypes ?? Array.Empty<Type>();

		internal Option<MethodInfo> TryGetMethod(Type containingType)
			=> Option
				.FromNullable((_type ?? containingType)
					.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
					.FirstOrDefault(method => method.Name == _methodName && method.GetParameters().Select(p => p.ParameterType).SequenceEqual(_parameterTypes))
				);
	}
}
