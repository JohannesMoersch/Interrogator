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
	public class NotConcurrentAttribute : Attribute, IDependsOnAttribute
	{
		private static readonly Dictionary<(Type Type, string GroupName), MethodInfo[]> _methodInfoDictionary = new Dictionary<(Type Type, string GroupName), MethodInfo[]>();

		static NotConcurrentAttribute()
			=> _methodInfoDictionary =
			(
				from assembly in AppDomain.CurrentDomain.GetAssemblies()
				where assembly.GetCustomAttributes<UseIntegrationTestFrameworkAttribute>().Any()
				from type in assembly.GetTypes()
				from method in GetMethodsInTypes(type)
				from attribute in method.GetCustomAttributes<NotConcurrentAttribute>()
				let key = (type, groupName: attribute.GroupName)
				group method by key
			)
			.ToDictionary(g => g.Key, g => g.ToArray());

		public enum ConcurrencyScope
		{
			Class,
			ClassHierarchy,
			Namespace,
			Assembly
		}

		public string GroupName { get; }

		public ConcurrencyScope Scope { get; }

		bool IDependsOnAttribute.ContinueOnDependencyFailure => true;

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
			GroupName = $"{groupName}_{scope}"; // Adding scope to group name ensures that different scoped attributes don't collide
			Scope = scope;
		}

		Result<Option<MethodInfo>, string> IDependsOnAttribute.TryGetMethod(Type containingType, MemberInfo member, MethodInfo[] testMethods)
		{
			if (member is MethodInfo currentMethod)
				return Result.Success<Option<MethodInfo>, string>(GetPreviousInChain(currentMethod, containingType, Scope, GroupName, testMethods));

			return Result.Success<Option<MethodInfo>, string>(Option.None<MethodInfo>());
		}

		private static Option<MethodInfo> GetPreviousInChain(MethodInfo currentMethod, Type type, ConcurrencyScope scope, string groupName, MethodInfo[] executingMethods)
		{
			var methodSet = new HashSet<MemberInfo>(executingMethods);
			var methodsToExecute = GetSameGroupMethods(scope, type, groupName).Where(methodSet.Contains).ToArray();

			return GetPreviousMethod(currentMethod, methodsToExecute);
		}

		private static MethodInfo[] GetSameGroupMethods(ConcurrencyScope scope, Type type, string groupName)
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

			var list = new List<MethodInfo>();
			foreach (var t in types)
			{
				var key = (t, groupName);
				if (_methodInfoDictionary.ContainsKey(key))
					list.AddRange(_methodInfoDictionary[key]);
			}

			return list.ToArray();
		}

		private static MethodInfo[] GetMethodsInTypes(params Type[] types)
			=> types
				.OrderTypes()
				.SelectMany(GetMethodsInType)
				.ToArray();

		private static IEnumerable<MethodInfo> GetMethodsInType(Type type)
			=> type
				.GetMethods()
				.Where(m => m.GetCustomAttributes(true).OfType<NotConcurrentAttribute>().Any())
				.OrderBy(GetMethodKey);

		private static string GetMethodKey(MethodInfo method)
			=> $"{method.Name}-{String.Join("-", method.GetParameters().Select(p => p.ParameterType.FullName))}";

		private static IEnumerable<Type> GetTypesInClassHierarchy(Type type)
			=> GetNestedTypes(GetRootType(type));

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

		private static IEnumerable<Type> GetNestedTypes(Type type)
		{
			var nestedTypes = type.GetNestedTypes();
			yield return type;
			foreach (var nestedType in nestedTypes.SelectMany(GetNestedTypes))
				yield return nestedType;
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