﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interrogator.xUnit.Common;
using Xunit.Sdk;

namespace Interrogator.xUnit.Execution
{
	internal class ExecutionJob
	{
		private const string _exceptionBoundries = "********************************************************************************";

		private ExecutionJob(MethodInfo method, MethodInfo[] parameterMethods, Option<ConstructorInfo> constructor, MethodInfo[] constructorParameterMethods, (MethodInfo method, bool continueOnDependencyFailure)[] methodDependencies, (MethodInfo method, bool continueOnDependencyFailure)[] constructorDependencies, Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, TestFailure>>> execute, Action<string> abort)
		{
			Method = method;
			Status = parameterMethods.Length == 0 && constructorParameterMethods.Length == 0 && methodDependencies.Length == 0 && constructorDependencies.Length == 0 ? ExecutionStatus.Ready : ExecutionStatus.NotReady;
			_parameterMethods = parameterMethods;
			Constructor = constructor;
			_constructorParameterMethods = constructorParameterMethods;
			_methodDependencies = methodDependencies;
			_constructorDependencies = constructorDependencies;
			ErrorMessage = Option.None<string>();

			_execute = execute;
			_abort = abort;
			_methodArguments = new Option<Option<object>>[_parameterMethods.Count];
			_constructorArguments = new Option<Option<object>>[_constructorParameterMethods.Count];
			_methodDependenciesMet = new bool[_methodDependencies.Count];
			_constructorDependenciesMet = new bool[_constructorDependencies.Count];
		}

		private ExecutionJob(MethodInfo method, string errorMessage, Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, TestFailure>>> execute, Action<string> abort)
		{
			Method = method;
			Status = ExecutionStatus.Ready;
			_parameterMethods = new MethodInfo[0];
			ErrorMessage = Option.Some(errorMessage ?? throw new ArgumentNullException(nameof(errorMessage)));

			_execute = execute;
			_abort = abort;
		}

		public Option<ConstructorInfo> Constructor { get; }

		public MethodInfo Method { get; }

		public ExecutionStatus Status { get; private set; }

		private readonly IReadOnlyList<MethodInfo> _parameterMethods;

		private readonly IReadOnlyList<MethodInfo> _constructorParameterMethods;

		private readonly IReadOnlyList<(MethodInfo method, bool continueOnDependencyFailure)> _methodDependencies;

		private readonly IReadOnlyList<(MethodInfo method, bool continueOnDependencyFailure)> _constructorDependencies;

		public IEnumerable<MethodInfo> Dependencies
			=> _parameterMethods
				.Concat(_constructorParameterMethods)
				.Concat(_methodDependencies.Select(set => set.method))
				.Concat(_constructorDependencies.Select(set => set.method));

		public Option<string> ErrorMessage { get; }

		private readonly Option<Option<object>>[] _methodArguments;

		private readonly Option<Option<object>>[] _constructorArguments;

		private readonly bool[] _methodDependenciesMet;

		private readonly bool[] _constructorDependenciesMet;

		private readonly Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, TestFailure>>> _execute;

		private readonly Action<string> _abort;

		private Option<TestFailure> _failure;

		public void SetParameterSuccess(MethodInfo methodInfo, Option<object> value)
		{
			if (Status != ExecutionStatus.NotReady)
				return;

			for (int i = 0; i < _parameterMethods.Count; ++i)
			{
				if (_parameterMethods[i] == methodInfo && _methodArguments[i] == Option.None<Option<object>>())
					_methodArguments[i] = Option.Some(value);
			}

			for (int i = 0; i < _constructorParameterMethods.Count; ++i)
			{
				if (_constructorParameterMethods[i] == methodInfo && _constructorArguments[i] == Option.None<Option<object>>())
					_constructorArguments[i] = Option.Some(value);
			}

			if (CheckIsReady())
				Status = ExecutionStatus.Ready;
		}

		public void SetDependencyComplete(MethodInfo methodInfo, bool isSuccess)
		{
			if (Status != ExecutionStatus.NotReady)
				return;

			for (int i = 0; i < _methodDependencies.Count; ++i)
			{
				if (_methodDependencies[i].method == methodInfo && (isSuccess || _methodDependencies[i].continueOnDependencyFailure) && !_methodDependenciesMet[i])
					_methodDependenciesMet[i] = true;
			}

			for (int i = 0; i < _constructorDependencies.Count; ++i)
			{
				if (_constructorDependencies[i].method == methodInfo && (isSuccess || _constructorDependencies[i].continueOnDependencyFailure) &&!_constructorDependenciesMet[i])
					_constructorDependenciesMet[i] = true;
			}

			if (CheckIsReady())
				Status = ExecutionStatus.Ready;
		}

		private bool CheckIsReady()
			=> _methodArguments.All(o => o != Option.None<Option<object>>())
			&& _constructorArguments.All(o => o != Option.None<Option<object>>())
			&& _methodDependenciesMet.All(o => o)
			&& _constructorDependenciesMet.All(o => o);

		public async Task<Result<Option<object>, Unit>> Execute(CancellationTokenSource cancellationTokenSource)
		{
			if (Status != ExecutionStatus.Ready)
				throw new Exception("Test prerequisites not met.");

			var methodArguments = _methodArguments
				.Select(obj => obj.Match(o => o.Match(_ => _, () => default), () => default))
				.ToArray();

			var constructorArguments = _constructorArguments
				.Select(obj => obj.Match(o => o.Match(_ => _, () => default), () => default))
				.ToArray();

			Status = ExecutionStatus.Executing;

			var result = await _execute.Invoke((methodArguments, constructorArguments), cancellationTokenSource);

			return result
				.Match
				(
					value =>
					{
						Status = ExecutionStatus.Complete;

						return Result.Success<Option<object>, Unit>(value);
					}, 
					failure =>
					{
						Status = ExecutionStatus.NotComplete;

						_failure = Option.Some(failure);

						return Result.Failure<Option<object>, Unit>(Unit.Value);
					}
				);
		}

		public void Abort(Dictionary<MethodInfo, ExecutionJob> jobs) 
			=> _abort.Invoke($"The following dependencies failed:{String.Join("", GetAbortMessage(jobs, new HashSet<MethodInfo>()).Select(str => $"{Environment.NewLine}{str}"))}");

		private IEnumerable<string> GetAbortMessage(Dictionary<MethodInfo, ExecutionJob> jobs, HashSet<MethodInfo> methodStack)
		{
			if (!methodStack.Add(Method))
				return new[] { "\tCycle Detected" };

			return Constructor
				  .Match
				  (
					  constructor => GetParametersAbortMessage(jobs, constructor.GetParameters(), _constructorArguments, _constructorParameterMethods, "Constructor Parameter", methodStack)
						.Concat(GetDependenciesAbortMessage(jobs, _constructorDependenciesMet, _constructorDependencies.Select(set => set.method).ToArray(), "Constructor Dependency", methodStack)),
					  Enumerable.Empty<string>
				  )
				  .Concat(GetParametersAbortMessage(jobs, Method.GetParameters(), _methodArguments, _parameterMethods, "Method Parameter", methodStack))
				  .Concat(GetDependenciesAbortMessage(jobs, _methodDependenciesMet, _methodDependencies.Select(set => set.method).ToArray(), "Method Dependency", methodStack))
				  .Concat(_failure.Match(failure => GetFailureMessage(failure), () => Enumerable.Empty<string>()));
		}

		private static IEnumerable<string> GetParametersAbortMessage(Dictionary<MethodInfo, ExecutionJob> jobs, ParameterInfo[] parameters, Option<Option<object>>[] parameterArguments, IReadOnlyList<MethodInfo> parameterMethods, string prefix, HashSet<MethodInfo> methodStack)
			=> parameterArguments
				.Select((o, i) => o.Match(_ => null, () => (int?)i))
				.Where(i => i != null)
				.Select(i => (parameter: parameters[i.Value], source: parameterMethods[i.Value]))
				.SelectMany(set => GetMissingParameterMessages(jobs, set.parameter, set.source, prefix, methodStack))
				.Select(str => str.FirstOrDefault() != '*' ? $"\t{str}" : str);

		private static IEnumerable<string> GetMissingParameterMessages(Dictionary<MethodInfo, ExecutionJob> jobs, ParameterInfo parameter, MethodInfo source, string prefix, HashSet<MethodInfo> methodStack)
			=> new[] 
			{
				$"{prefix}: {parameter.Name} <- {source.DeclaringType.FullName}.{source.Name}({String.Join(",", source.GetParameters().Select(p => p.ParameterType.Name))})"
			}
			.Concat(jobs[source].GetAbortMessage(jobs, methodStack));

		private static IEnumerable<string> GetDependenciesAbortMessage(Dictionary<MethodInfo, ExecutionJob> jobs, bool[] parameterMet, IReadOnlyList<MethodInfo> parameterMethods, string prefix, HashSet<MethodInfo> methodStack)
			=> parameterMet
				.Select((o, i) => !o ? (int?)i : null)
				.Where(i => i != null)
				.SelectMany(i => GetMissingDependencyMessages(jobs, parameterMethods[i.Value], prefix, methodStack))
				.Select(str => str.FirstOrDefault() != '*' ? $"\t{str}" : str);

		private static IEnumerable<string> GetMissingDependencyMessages(Dictionary<MethodInfo, ExecutionJob> jobs, MethodInfo source, string prefix, HashSet<MethodInfo> methodStack)
			=> new[]
			{
				$"{prefix}: {source.DeclaringType.FullName}.{source.Name}({String.Join(",", source.GetParameters().Select(p => p.ParameterType.Name))})"
			}
			.Concat(jobs[source].GetAbortMessage(jobs, methodStack));

		private static IEnumerable<string> GetFailureMessage(TestFailure testFailure)
			=> testFailure
				.Match
				(
					ex => new[] { $"{_exceptionBoundries}{Environment.NewLine}{GetExceptionMessage(ex)}{Environment.NewLine}{_exceptionBoundries}" },
					skip => new[] { $"\tTest was skipped: {skip}" }
				);

		private static string GetExceptionMessage(Exception exception)
		{
			if (exception is TargetInvocationException invocationException)
				exception = invocationException.InnerException;

			if (exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1)
				exception = aggregateException.InnerException;

			return exception.ToString();
		}

		public static ExecutionJob Create(MethodInfo testMethod, string errorMessage, Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, TestFailure>>> execute, Action<string> abort) 
			=> new ExecutionJob(testMethod, errorMessage, execute, abort);

		public static ExecutionJob Create(MethodInfo testMethod, Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, TestFailure>>> execute, Action<string> abort)
			=> GetParameters(testMethod.DeclaringType, testMethod.GetParameters())
				.Bind
				(
					methodParameters => GetDependencies(testMethod.DeclaringType, testMethod)
						.Bind(methodDependencies => GetConstructorParameters(testMethod)
							.Select(info => new ExecutionJob(testMethod, methodParameters, info.constructor, info.constructorParameters, methodDependencies, info.constructorDependencies, execute, abort)
						)
					)
				)
				.Match
				(
					_ => _,
					error => new ExecutionJob(testMethod, error, execute, abort)
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
				.Select(att => att.TryGetMethod(classType).Select(method => (method, att.ContinueOnDependencyFailure)))
				.TakeUntilFailure();
	}
}
