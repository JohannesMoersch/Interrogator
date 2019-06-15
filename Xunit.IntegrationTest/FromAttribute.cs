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
			: this(null, methodName, null)
		{ }

		public FromAttribute(Type type, string methodName)
			: this(type, methodName, null)
		{ }

		public FromAttribute(string methodName, params Type[] parameterTypes)
			: this(null, methodName, parameterTypes)
		{ }

		public FromAttribute(Type type, string methodName, params Type[] parameterTypes)
		{
			_type = type;
			_methodName = methodName;
			_parameterTypes = parameterTypes;
		}

		internal Result<MethodInfo, string> TryGetMethod(Type containingType)
			=> Option
				.FromNullable((_type ?? containingType)
					.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
					.FirstOrDefault(method => method.Name == _methodName && (_parameterTypes == null || method.GetParameters().Select(p => p.ParameterType).SequenceEqual(_parameterTypes)))
				)
				.ToResult(() => $"Could not find source method '{(_type ?? containingType).Name}.{_methodName}({String.Join(", ", _parameterTypes.Select(t => t.Name))})'.");
	}
}
