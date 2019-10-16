using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Interrogator.xUnit.Common;

namespace Interrogator.xUnit
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class NotConcurrentAttribute : Attribute
	{
		public enum ConcurrencyScope
		{
			Class,
			ClassHierarchy,
			Namespace,
			Assembly
		}

		public string GroupName { get; }

		public ConcurrencyScope Scope { get; }

		public NotConcurrentAttribute() : this(ConcurrencyScope.Class, String.Empty) { }

		public NotConcurrentAttribute(string groupName) : this(groupName, ConcurrencyScope.Class) { }

		public NotConcurrentAttribute(ConcurrencyScope scope) : this(scope, String.Empty) { }

		public NotConcurrentAttribute(string groupName, ConcurrencyScope scope) : this(scope, groupName)
		{
			if (String.IsNullOrWhiteSpace(groupName))
				throw new InvalidGroupNameException(groupName);
		}

		private NotConcurrentAttribute(ConcurrencyScope scope, string groupName)
		{
			GroupName = groupName;
			Scope = scope;
		}

		internal string GetGroupKey(Type containingType)
		{
			var scopeName = String.Empty;

			switch (Scope)
			{
				case ConcurrencyScope.Assembly:
					scopeName = containingType.Assembly.FullName;
					break;
				case ConcurrencyScope.Namespace:
					scopeName = containingType.Namespace;
					break;
				case ConcurrencyScope.ClassHierarchy:
					scopeName = GetRootType(containingType).FullName;
					break;
				case ConcurrencyScope.Class:
					scopeName = containingType.FullName;
					break;
			}

			return $"{GroupName}_{Scope}_{scopeName}";
		}

		private static Type GetRootType(Type type)
		{
			var root = type;
			var nextParent = type?.DeclaringType;
			while (nextParent != typeof(object) && nextParent != null)
			{
				root = nextParent;
				nextParent = nextParent.DeclaringType;
			}
			return root;
		}
	}
}