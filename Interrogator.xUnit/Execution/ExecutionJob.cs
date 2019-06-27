using System;
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

		private ExecutionJob(MethodInfo method, MethodInfo[] parameterMethods, Option<ConstructorInfo> constructor, MethodInfo[] constructorParameterMethods, Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, Exception>>> execute, Action<string> abort)
		{
			Method = method;
			Status = parameterMethods.Length == 0 && constructorParameterMethods.Length == 0 ? ExecutionStatus.Ready : ExecutionStatus.NotReady;
			ParameterMethods = parameterMethods;
			Constructor = constructor;
			ConstructorParameterMethods = constructorParameterMethods;
			ErrorMessage = Option.None<string>();

			_execute = execute;
			_abort = abort;
			_methodArguments = new Option<Option<object>>[ParameterMethods.Count];
			_constructorArguments = new Option<Option<object>>[ConstructorParameterMethods.Count];
		}

		private ExecutionJob(MethodInfo method, string errorMessage, Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, Exception>>> execute, Action<string> abort)
		{
			Method = method;
			Status = ExecutionStatus.Ready;
			ParameterMethods = new MethodInfo[0];
			ErrorMessage = Option.Some(errorMessage ?? throw new ArgumentNullException(nameof(errorMessage)));

			_execute = execute;
			_abort = abort;
		}

		public Option<ConstructorInfo> Constructor { get; }

		public MethodInfo Method { get; }

		public ExecutionStatus Status { get; private set; }

		public IReadOnlyList<MethodInfo> ParameterMethods { get; }

		public IReadOnlyList<MethodInfo> ConstructorParameterMethods { get; }

		public Option<string> ErrorMessage { get; }

		private readonly Option<Option<object>>[] _methodArguments;

		private readonly Option<Option<object>>[] _constructorArguments;

		private readonly Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, Exception>>> _execute;

		private readonly Action<string> _abort;

		private Option<Exception> _exception;

		public void SetParameter(MethodInfo methodInfo, Option<object> value)
		{
			if (Status != ExecutionStatus.NotReady)
				return;

			bool ready = true;
			for (int i = 0; i < ParameterMethods.Count; ++i)
			{
				if (ParameterMethods[i] == methodInfo && _methodArguments[i] == Option.None<Option<object>>())
					_methodArguments[i] = Option.Some(value);

				if (_methodArguments[i] == Option.None<Option<object>>())
					ready = false;
			}

			for (int i = 0; i < ConstructorParameterMethods.Count; ++i)
			{
				if (ConstructorParameterMethods[i] == methodInfo && _constructorArguments[i] == Option.None<Option<object>>())
					_constructorArguments[i] = Option.Some(value);

				if (_constructorArguments[i] == Option.None<Option<object>>())
					ready = false;
			}

			if (ready)
				Status = ExecutionStatus.Ready;
		}

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

			var result = await _execute.Invoke((methodArguments, constructorArguments), cancellationTokenSource);

			return result
				.Match
				(
					value =>
					{
						Status = ExecutionStatus.Complete;

						return Result.Success<Option<object>, Unit>(value);
					}, 
					ex =>
					{
						Status = ExecutionStatus.NotComplete;

						_exception = Option.Some(ex);

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
					  constructor => GetParametersAbortMessage(jobs, constructor.GetParameters(), _constructorArguments, ConstructorParameterMethods, "Constructor Parameter", methodStack),
					  Enumerable.Empty<string>
				  )
				  .Concat(GetParametersAbortMessage(jobs, Method.GetParameters(), _methodArguments, ParameterMethods, "Method Parameter", methodStack))
				  .Concat(_exception.Match(ex => new[] { $"{_exceptionBoundries}{Environment.NewLine}{GetExceptionMessage(ex)}{Environment.NewLine}{_exceptionBoundries}" }, () => Array.Empty<string>()));
		}

		private static IEnumerable<string> GetParametersAbortMessage(Dictionary<MethodInfo, ExecutionJob> jobs, ParameterInfo[] parameters, Option<Option<object>>[] parameterArguments, IReadOnlyList<MethodInfo> parameterMethods, string prefix, HashSet<MethodInfo> methodStack)
			=> parameterArguments
				.Select((o, i) => o.Match(_ => null, () => (int?)i))
				.Where(i => i != null)
				.Select(i => (parameter: parameters[i.Value], source: parameterMethods[i.Value]))
				.SelectMany(set => GetMissingParameterMessages(jobs, set.parameter, set.source, prefix, methodStack))
				.Select(str => str.FirstOrDefault() != '*' ? $"\t{str}" : str);

		private static IEnumerable<string> GetMissingParameterMessages(Dictionary<MethodInfo, ExecutionJob> jobs, ParameterInfo parameter, MethodInfo source, string prefix, HashSet<MethodInfo> methodStack)
		{
			return new[] 
			{
				$"{prefix}: {parameter.Name} <- {source.DeclaringType.FullName}.{source.Name}({String.Join(",", source.GetParameters().Select(p => p.ParameterType.Name))})"
			}
			.Concat(jobs[source].GetAbortMessage(jobs, methodStack));
		}

		private static string GetExceptionMessage(Exception exception)
		{
			if (exception is TargetInvocationException invocationException)
				exception = invocationException.InnerException;

			if (exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1)
				exception = aggregateException.InnerException;

			return exception.ToString();
		}

		public static ExecutionJob Create(MethodInfo testMethod, string errorMessage, Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, Exception>>> execute, Action<string> abort) 
			=> new ExecutionJob(testMethod, errorMessage, execute, abort);

		public static ExecutionJob Create(MethodInfo testMethod, Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, Exception>>> execute, Action<string> abort)
			=> GetParameters(testMethod.DeclaringType, testMethod.GetParameters())
				.Match
				(
					methodParameters => GetConstructorParameters(testMethod)
						.Match
						(
							info => new ExecutionJob(testMethod, methodParameters, info.constructor, info.constructorParameters, execute, abort),
							error => new ExecutionJob(testMethod, error, execute, abort)
						),
					error => new ExecutionJob(testMethod, error, execute, abort)
				);

		private static Result<(Option<ConstructorInfo> constructor, MethodInfo[] constructorParameters), string> GetConstructorParameters(MethodInfo testMethod)
		{
			if (testMethod.IsStatic)
				return Result.Success<(Option<ConstructorInfo>, MethodInfo[]), string>((Option.None<ConstructorInfo>(), Array.Empty<MethodInfo>()));

			var constructors = testMethod
				.DeclaringType
				.GetConstructors();

			if (constructors.Length == 0)
				return Result.Success<(Option<ConstructorInfo>, MethodInfo[]), string>((Option.None<ConstructorInfo>(), Array.Empty<MethodInfo>()));

			if (constructors.Length > 1)
				return Result.Failure<(Option<ConstructorInfo>, MethodInfo[]), string>("Only one constructor can be defined on the class.");

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
						Result.Failure<(Option<ConstructorInfo>, MethodInfo[]), string>,
						() => Result.Success<(Option<ConstructorInfo>, MethodInfo[]), string>((Option.Some(constructor), methods))
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
	}
}
