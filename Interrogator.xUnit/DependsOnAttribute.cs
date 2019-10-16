using System;
using System.Reflection;
using System.Text;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Utilities;

namespace Interrogator.xUnit
{
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true)]
	public class DependsOnAttribute : Attribute
	{
		public Type Type { get; }

		private readonly string _methodName;

		public Type[] ParameterTypes { get; }

		public bool ContinueOnDependencyFailure { get; set; }

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
			Type = type;
			_methodName = methodName;
			ParameterTypes = parameterTypes;
		}

		internal Result<Option<MethodInfo>, string> TryGetMethod(Type containingType, MemberInfo member)
			=> (Type ?? containingType)
				.TryGetMethod(_methodName, ParameterTypes)
				.Select(Option.Some);
	}
}
