using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Interrogator.xUnit.Utilities
{
#pragma warning disable xUnit1000 // Test classes must be public
	internal class MethodInfoWrapper : IMethodInfo
#pragma warning restore xUnit1000 // Test classes must be public
	{
		[Fact]
		private static void FactMethod() { }

		private readonly IMethodInfo _methodInfo;

		public MethodInfoWrapper(IMethodInfo methodInfo)
			=> _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

		public bool IsAbstract => _methodInfo.IsAbstract;

		public bool IsGenericMethodDefinition => _methodInfo.IsGenericMethodDefinition;

		public bool IsPublic => _methodInfo.IsPublic;

		public bool IsStatic => _methodInfo.IsStatic;

		public string Name => _methodInfo.Name;

		public ITypeInfo ReturnType => _methodInfo.ReturnType;

		public ITypeInfo Type => new ReflectionTypeInfo(_methodInfo.ToRuntimeMethod().DeclaringType);

		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
		{
			if (assemblyQualifiedAttributeTypeName == typeof(FactAttribute).AssemblyQualifiedName)
			{
				var attribute = CustomAttributeData
					.GetCustomAttributes(typeof(MethodInfoWrapper).GetMethod(nameof(FactMethod), BindingFlags.NonPublic | BindingFlags.Static))
					.First();

				return new IAttributeInfo[] { new ReflectionAttributeInfo(attribute) };
			}

			return _methodInfo.GetCustomAttributes(assemblyQualifiedAttributeTypeName);
		}

		public IEnumerable<ITypeInfo> GetGenericArguments()
			=> _methodInfo.GetGenericArguments();

		public IEnumerable<IParameterInfo> GetParameters()
			=> _methodInfo.GetParameters();

		public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments)
			=> _methodInfo.MakeGenericMethod(typeArguments);
	}
}
