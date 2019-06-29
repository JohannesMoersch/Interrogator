using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Utilities;

namespace Interrogator.xUnit
{
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false)]
	public class DependsOnAttribute : Attribute
	{
		private readonly Type _type;

		private readonly string _methodName;

		private Type[] _parameterTypes;

		public DependsOnAttribute(string methodName)
			: this(null, methodName, null)
		{ }

		public DependsOnAttribute(Type type, string methodName)
			: this(type, methodName, null)
		{ }

		public DependsOnAttribute(string methodName, params Type[] parameterTypes)
			: this(null, methodName, parameterTypes)
		{ }

		public DependsOnAttribute(Type type, string methodName, params Type[] parameterTypes)
		{
			_type = type;
			_methodName = methodName;
			_parameterTypes = parameterTypes;
		}
		internal Result<MethodInfo, string> TryGetMethod(Type containingType)
			=> (_type ?? containingType)
				.TryGetMethod(_methodName, _parameterTypes);
	}
}
