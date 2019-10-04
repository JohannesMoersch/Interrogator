using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Utilities;

namespace Interrogator.xUnit
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class NotConcurrentAttribute : DependsOnAttribute
	{
		private static readonly Dictionary<Type, MethodInfo[]> _methodInfoDictionary = new Dictionary<Type, MethodInfo[]>();

		public enum ConcurrencyScope
		{
			Class,
			ClassHierarchy,
			Namespace,
			Assembly
		}

		public string GroupName { get; }
		public ConcurrencyScope Scope { get; }

		public NotConcurrentAttribute() : this("", ConcurrencyScope.Class) { }
		public NotConcurrentAttribute(string groupName) : this(groupName, ConcurrencyScope.Class) { }

		public NotConcurrentAttribute(ConcurrencyScope scope) : this("", scope) { }

		public NotConcurrentAttribute(string groupName, ConcurrencyScope scope) : base(null)
		{
			GroupName = $"{groupName}_{scope}"; // Adding scope to group name ensures that different scoped attributes don't collide
			Scope = scope;
		}

		internal override Result<Option<MethodInfo>, string> TryGetMethod(Type containingType, MemberInfo member)
		{
			var result = (Type ?? containingType)
				.TryGetMethod(member.Name, ParameterTypes)
				.Select(currentMethod => GetPreviousInChain(currentMethod, containingType, Scope, GroupName));
			return result;
		}

		private static Option<MethodInfo> GetPreviousInChain(MethodInfo currentMethod, Type type, ConcurrencyScope scope, string groupName)
		{
			var group = GetSameGroupMethods(scope, type, groupName);
			return GetPreviousMethod(currentMethod, group);
		}

		private static MethodInfo[] GetSameGroupMethods(ConcurrencyScope scope, Type type, string groupName)
		{
			if (!_methodInfoDictionary.ContainsKey(type))
			{
				switch (scope)
				{
					case ConcurrencyScope.Class:
						_methodInfoDictionary[type] = GetGroupMethodsInClass(type, groupName).ToArray();
						break;
					case ConcurrencyScope.Assembly:
						_methodInfoDictionary[type] = GetGroupMethodsInAssembly(type, groupName).ToArray();
						break;
					case ConcurrencyScope.Namespace:
						_methodInfoDictionary[type] = GetGroupMethodsInNamespace(type, groupName).ToArray();
						break;
					case ConcurrencyScope.ClassHierarchy:
						_methodInfoDictionary[type] = GetGroupMethodsInClassHierarchy(type, groupName).ToArray();
						break;
					default:
						_methodInfoDictionary[type] = Array.Empty<MethodInfo>();
						break;
				}
			}

			return _methodInfoDictionary[type];
		}

		private static IEnumerable<MethodInfo> GetGroupMethodsInClassHierarchy(Type type, string groupName)
		{
			foreach (var t in GetTypesInClassHierarchy(type))
			{
				foreach (var method in GetGroupMethodsInClass(t, groupName))
					yield return method;
			}
		}

		private static IEnumerable<Type> GetTypesInClassHierarchy(Type type)
		{
			var nextParent = type;
			while (nextParent != typeof(object) && nextParent != null)
			{
				yield return nextParent;
				nextParent = nextParent.DeclaringType;
			};
		}

		private static IEnumerable<MethodInfo> GetGroupMethodsInNamespace(Type type, string groupName)
			=> AppDomain
				.CurrentDomain
				.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(t => t.Namespace == type.Namespace)
				.SelectMany(t => GetGroupMethodsInClass(t, groupName));

		private static IEnumerable<MethodInfo> GetGroupMethodsInAssembly(Type type, string groupName)
			=> type
				.Assembly
				.GetTypes()
				.OrderBy(t => t.FullName)
				.SelectMany(t => GetGroupMethodsInClass(t, groupName));

		private static IEnumerable<MethodInfo> GetGroupMethodsInClass(Type type, string groupName)
			=> type
				.GetMethods()
				.Where(m => m.GetCustomAttributes(true).OfType<NotConcurrentAttribute>().Any(x => x.GroupName == groupName))
				.OrderBy(m => m.Name);

		private static Option<MethodInfo> GetPreviousMethod(MethodBase currentMethod, MethodInfo[] sameGroupMethods)
		{
			for (var i = 0; i < sameGroupMethods.Length; i++)
			{
				if (sameGroupMethods[i] == currentMethod)
					return Option.Create(i > 0, () => sameGroupMethods[i - 1]);
			}
			return Option.None<MethodInfo>();
		}
	}
}