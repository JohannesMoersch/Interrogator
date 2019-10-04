using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Utilities;

namespace Interrogator.xUnit
{
	internal static class TypeExtensions
	{
		public static IOrderedEnumerable<Type> OrderTypes(this IEnumerable<Type> types)
			=> types.OrderBy(type => type.FullName);
	}

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

		internal override Result<Option<MethodInfo>, string> TryGetMethod(Type containingType, MemberInfo member, MethodInfo[] testMethods)
		{
			var result = (Type ?? containingType)
				.TryGetMethod(member.Name, ParameterTypes)
				.Select(currentMethod => GetPreviousInChain(currentMethod, containingType, Scope, GroupName, testMethods));
			return result;
		}

		private static Option<MethodInfo> GetPreviousInChain(MethodInfo currentMethod, Type type, ConcurrencyScope scope, string groupName, MethodInfo[] executingMethods)
		{
			var methodSet = new HashSet<MemberInfo>(executingMethods);
			var methodsToExecute = GetSameGroupMethods(scope, type, groupName).Where(methodSet.Contains).ToArray();

			return GetPreviousMethod(currentMethod, methodsToExecute);
		}

		private static MethodInfo[] GetSameGroupMethods(ConcurrencyScope scope, Type type, string groupName)
		{
			if (!_methodInfoDictionary.ContainsKey(type))
			{
				IEnumerable<Type> types;
				switch (scope)
				{
					case ConcurrencyScope.Class:
						types = new[] { type };
						break;
					case ConcurrencyScope.Assembly:
						types = GetTypesInAssembly(type);
						break;
					case ConcurrencyScope.Namespace:
						types = GetTypesInNamespace(type);
						break;
					case ConcurrencyScope.ClassHierarchy:
						types = GetTypesInClassHierarchy(type);
						break;
					default:
						types = Array.Empty<Type>();
						break;
				}
				_methodInfoDictionary[type] = GetMethodsInTypes(groupName, types.ToArray());
			}

			return _methodInfoDictionary[type];
		}

		private static MethodInfo[] GetMethodsInTypes(string groupName, params Type[] types)
			=> types
				.OrderTypes()
				.SelectMany(t => GetGroupMethodsInType(t, groupName))
				.ToArray();

		private static IEnumerable<MethodInfo> GetGroupMethodsInType(Type type, string groupName)
			=> type
				.GetMethods()
				.Where(m => m.GetCustomAttributes(true).OfType<NotConcurrentAttribute>().Any(x => x.GroupName == groupName))
				.OrderBy(GetMethodKey);

		private static string GetMethodKey(MethodInfo method)
			=> $"{method.Name}-{String.Join("-", method.GetParameters().Select(p => p.ParameterType.FullName))}";

		private static IEnumerable<Type> GetTypesInClassHierarchy(Type type)
		{
			var nextParent = type;
			while (nextParent != typeof(object) && nextParent != null)
			{
				yield return nextParent;
				nextParent = nextParent.DeclaringType;
			}

			foreach (var t in type.GetNestedTypes())
				yield return t;
		}

		private static IEnumerable<Type> GetTypesInNamespace(Type type)
			=> AppDomain
				.CurrentDomain
				.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(t => t.Namespace == type.Namespace);

		private static IEnumerable<Type> GetTypesInAssembly(Type type)
			=> type
				.Assembly
				.GetTypes();

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