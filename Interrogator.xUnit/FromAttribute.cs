﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Utilities;

namespace Interrogator.xUnit
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
			=> (_type ?? containingType)
				.TryGetMethod(_methodName, _parameterTypes);
	}
}
