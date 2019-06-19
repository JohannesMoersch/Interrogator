using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.IntegrationTest.Common;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal class ExecutionJob
	{
		private ExecutionJob(MethodInfo method, MethodInfo[] parameterMethods, Func<object[], CancellationTokenSource, Task<Result<Option<object>, Exception>>> execute, Action<string> abort)
		{
			Method = method;
			Status = parameterMethods.Length == 0 ? ExecutionStatus.Ready : ExecutionStatus.NotReady;
			ParameterMethods = parameterMethods;
			ErrorMessage = Option.None<string>();

			_execute = execute;
			_abort = abort;
			_values = new Option<Option<object>>[ParameterMethods.Count];
		}

		private ExecutionJob(MethodInfo method, string errorMessage, Func<object[], CancellationTokenSource, Task<Result<Option<object>, Exception>>> execute, Action<string> abort)
		{
			Method = method;
			Status = ExecutionStatus.Ready;
			ParameterMethods = new MethodInfo[0];
			ErrorMessage = Option.Some(errorMessage ?? throw new ArgumentNullException(nameof(errorMessage)));

			_execute = execute;
			_abort = abort;
		}

		public MethodInfo Method { get; }

		public ExecutionStatus Status { get; private set; }

		public IReadOnlyList<MethodInfo> ParameterMethods { get; }

		public Option<string> ErrorMessage { get; }

		private readonly Option<Option<object>>[] _values;

		private readonly Func<object[], CancellationTokenSource, Task<Result<Option<object>, Exception>>> _execute;

		private readonly Action<string> _abort;

		private Option<Exception> _exception;

		public void SetParameter(MethodInfo methodInfo, Option<object> value)
		{
			if (Status != ExecutionStatus.NotReady)
				return;

			bool ready = true;
			for (int i = 0; i < ParameterMethods.Count; ++i)
			{
				if (ParameterMethods[i] == methodInfo && _values[i] == Option.None<Option<object>>())
					_values[i] = Option.Some(value);

				if (_values[i] == Option.None<Option<object>>())
					ready = false;
			}

			if (ready)
				Status = ExecutionStatus.Ready;
		}

		public async Task<Result<Option<object>, Unit>> Execute(CancellationTokenSource cancellationTokenSource)
		{
			if (Status != ExecutionStatus.Ready)
				throw new Exception("Test prerequisites not met.");

			var result = await _execute.Invoke(_values.Select(obj => obj.Match(o => o.Match(_ => _, () => default), () => default)).ToArray(), cancellationTokenSource);

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
			=> _abort.Invoke(GetAbortMessage(jobs));

		private string GetAbortMessage(Dictionary<MethodInfo, ExecutionJob> jobs)
		{
			var parameters = Method.GetParameters();

			var missingParameters = _values
				.Select((o, i) => o.Match(_ => null, () => (int?)i))
				.Where(i => i != null)
				.Select(i => (parameter: parameters[i.Value], source: ParameterMethods[i.Value]))
				.Select(set => $"{set.parameter.Name} <- {set.source.DeclaringType.FullName}.{set.source.Name}({String.Join(",", set.source.GetParameters().Select(p => p.ParameterType.Name))}) - {jobs[set.source]._exception.ToString()}")
				.Select(str => $"{Environment.NewLine}\t{str}");

			return $"The sources for the following parameters failed:{String.Join("", missingParameters)}";
		}

		public static ExecutionJob Create(MethodInfo testMethod, string errorMessage, Func<object[], CancellationTokenSource, Task<Result<Option<object>, Exception>>> execute, Action<string> abort) 
			=> new ExecutionJob(testMethod, errorMessage, execute, abort);

		public static ExecutionJob Create(MethodInfo testMethod, Func<object[], CancellationTokenSource, Task<Result<Option<object>, Exception>>> execute, Action<string> abort)
			=> testMethod
				.GetParameters()
				.Select(parameter => parameter
					.GetFromAttribute()
					.Bind(att => att.TryGetMethod(testMethod.DeclaringType))
					.Bind(method => Result
						.Create
						(
							IsReturnTypeCompatible(method.ReturnType, parameter.ParameterType),
							() => method,
							() => $"Cannot cast return type of source method '{method.DeclaringType.Name}.{method.Name}' from '{method.ReturnType.Name}' to '{parameter.ParameterType.Name}' for parameter '{parameter.Name}' on method '{parameter.Member.DeclaringType.Name}.{parameter.Member.Name}'"
						)
					)
				)
				.TakeUntilFailure()
				.Match
				(
					methods => new ExecutionJob(testMethod, methods, execute, abort),
					error => new ExecutionJob(testMethod, error, execute, abort)
				);

		private static bool IsReturnTypeCompatible(Type returnType, Type targetType)
		{
			if (returnType.IsConstructedGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
				returnType = returnType.GetGenericArguments()[0];

			return targetType.IsAssignableFrom(returnType);
		}
	}
}
