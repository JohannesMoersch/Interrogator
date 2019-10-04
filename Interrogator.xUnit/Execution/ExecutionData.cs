using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Interrogator.xUnit.Common;

namespace Interrogator.xUnit.Execution
{
	internal class ExecutionData
	{
		public static ExecutionData Empty { get; } = new ExecutionData(default, Array.Empty<MethodInfo>(), Array.Empty<MethodInfo>(), Array.Empty<(MethodInfo, bool)>(), Array.Empty<(MethodInfo, bool)>());

		public ExecutionData(Option<ConstructorInfo> constructor, IReadOnlyList<MethodInfo> parameterMethods, IReadOnlyList<MethodInfo> constructorParameterMethods, IReadOnlyList<(MethodInfo method, bool continueOnDependencyFailure)> methodDependencies, IReadOnlyList<(MethodInfo method, bool continueOnDependencyFailure)> constructorDependencies)
		{
			Constructor = constructor;
			ParameterMethods = parameterMethods ?? throw new ArgumentNullException(nameof(parameterMethods));
			ConstructorParameterMethods = constructorParameterMethods ?? throw new ArgumentNullException(nameof(constructorParameterMethods));
			MethodDependencies = methodDependencies ?? throw new ArgumentNullException(nameof(methodDependencies));
			ConstructorDependencies = constructorDependencies ?? throw new ArgumentNullException(nameof(constructorDependencies));
		}

		public Option<ConstructorInfo> Constructor { get; }

		public IReadOnlyList<MethodInfo> ParameterMethods { get; }

		public IReadOnlyList<MethodInfo> ConstructorParameterMethods { get; }

		public IReadOnlyList<(MethodInfo method, bool continueOnDependencyFailure)> MethodDependencies { get; }

		public IReadOnlyList<(MethodInfo method, bool continueOnDependencyFailure)> ConstructorDependencies { get; }

		public static Result<ExecutionData, string> Create(MethodInfo method)
			=> GetParameters(method.DeclaringType, method.GetParameters())
				.Bind
				(
					methodParameters => GetDependencies(method.DeclaringType, method)
						.Bind(methodDependencies => GetConstructorParameters(method)
							.Select(info => new ExecutionData(info.constructor, methodParameters, info.constructorParameters, methodDependencies, info.constructorDependencies)
						)
					)
				);

		private static Result<(Option<ConstructorInfo> constructor, MethodInfo[] constructorParameters, (MethodInfo method, bool continueOnDependencyFailure)[] constructorDependencies), string> GetConstructorParameters(MethodInfo testMethod)
		{
			if (testMethod.IsStatic)
				return Result.Success<(Option<ConstructorInfo>, MethodInfo[], (MethodInfo method, bool continueOnDependencyFailure)[]), string>((Option.None<ConstructorInfo>(), Array.Empty<MethodInfo>(), Array.Empty<(MethodInfo method, bool continueOnDependencyFailure)>()));

			var constructors = testMethod
				.DeclaringType
				.GetConstructors();

			if (constructors.Length == 0)
				return Result.Success<(Option<ConstructorInfo>, MethodInfo[], (MethodInfo method, bool continueOnDependencyFailure)[]), string>((Option.None<ConstructorInfo>(), Array.Empty<MethodInfo>(), Array.Empty<(MethodInfo method, bool continueOnDependencyFailure)>()));

			if (constructors.Length > 1)
				return Result.Failure<(Option<ConstructorInfo>, MethodInfo[], (MethodInfo method, bool continueOnDependencyFailure)[]), string>("Only one constructor can be defined on the class.");

			var constructor = constructors.First();

			return GetParameters(testMethod.DeclaringType, constructor.GetParameters())
				.Bind(methods => Option
					.FromNullable(methods
						.Where(method => !method.IsStatic && method.DeclaringType == testMethod.DeclaringType)
						.Select(method => $"Constructor parameter from method \"{method.Name}\" is an instance method on the same class. This is unsupported.")
						.FirstOrDefault()
					)
					.Match
					(
						Result.Failure<(Option<ConstructorInfo>, MethodInfo[], (MethodInfo method, bool continueOnDependencyFailure)[]), string>,
						() => GetDependencies(testMethod.DeclaringType, constructor)
							.Select(constructorDependencies => (Option.Some(constructor), methods, constructorDependencies))
					)
				);
		}

		private static Result<MethodInfo[], string> GetParameters(Type classType, ParameterInfo[] parameters)
			=> parameters
				.Select(parameter => parameter
					.GetFromAttribute()
					.Bind(att => att.TryGetMethod(classType))
					.Bind(method => Result
						.Create
						(
							IsReturnTypeCompatible(method.ReturnType, parameter.ParameterType),
							() => method,
							() => $"Cannot cast return type of source method '{method.DeclaringType.Name}.{method.Name}' from '{method.ReturnType.Name}' to '{parameter.ParameterType.Name}' for parameter '{parameter.Name}' on method '{parameter.Member.DeclaringType.Name}.{parameter.Member.Name}'"
						)
					)
				)
				.TakeUntilFailure();

		private static bool IsReturnTypeCompatible(Type returnType, Type targetType)
		{
			if (returnType.IsConstructedGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
				returnType = returnType.GetGenericArguments()[0];

			return targetType.IsAssignableFrom(returnType);
		}

		private static Result<(MethodInfo method, bool continueOnDependencyFailure)[], string> GetDependencies(Type classType, MemberInfo member)
			=> member
				.GetCustomAttributes<DependsOnAttribute>()
				.Select(att => att.TryGetMethod(classType, member).Select(method => (method, att.ContinueOnDependencyFailure)))
				.TakeUntilFailure()
				.Select(option => option.Where(tuple => tuple.method.Match(_ => true, () => false)).Select(tuple => (tuple.method.Match(_ => _, () => default), tuple.ContinueOnDependencyFailure)).ToArray());
	}
}
