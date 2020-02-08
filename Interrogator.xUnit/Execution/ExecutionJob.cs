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

		private ExecutionJob(
			MethodInfo method, 
			ExecutionData executionData, 
			Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, TestFailure>>> execute, 
			Action<string> abort)
		{
			Method = method;
			_executionData = executionData;
			Status = _executionData.ParameterMethods.Count == 0 && _executionData.ConstructorParameterMethods.Count == 0 && _executionData.MethodDependencies.Count == 0 && _executionData.ConstructorDependencies.Count == 0 ? ExecutionStatus.Ready : ExecutionStatus.NotReady;

			_execute = execute;
			_abort = abort;
			_methodArguments = new Option<Option<object>>[_executionData.ParameterMethods.Count];
			_constructorArguments = new Option<Option<object>>[_executionData.ConstructorParameterMethods.Count];
			_methodDependenciesMet = new bool[_executionData.MethodDependencies.Count];
			_constructorDependenciesMet = new bool[_executionData.ConstructorDependencies.Count];
		}

		public Option<ConstructorInfo> Constructor { get; }

		public MethodInfo Method { get; }

		public ExecutionStatus Status { get; private set; }

		public IReadOnlyList<string> NotConcurrentGroupKeys => _executionData.NotConcurrentGroupKeys;

		private readonly ExecutionData _executionData;

		public IEnumerable<MethodInfo> Dependencies
			=> _executionData.ParameterMethods
				.Concat(_executionData.ConstructorParameterMethods)
				.Concat(_executionData.MethodDependencies.Select(set => set.method))
				.Concat(_executionData.ConstructorDependencies.Select(set => set.method));

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

			for (int i = 0; i < _executionData.ParameterMethods.Count; ++i)
			{
				if (_executionData.ParameterMethods[i] == methodInfo && _methodArguments[i] == Option.None<Option<object>>())
					_methodArguments[i] = Option.Some(value);
			}

			for (int i = 0; i < _executionData.ConstructorParameterMethods.Count; ++i)
			{
				if (_executionData.ConstructorParameterMethods[i] == methodInfo && _constructorArguments[i] == Option.None<Option<object>>())
					_constructorArguments[i] = Option.Some(value);
			}

			if (CheckIsReady())
				Status = ExecutionStatus.Ready;
		}

		public void SetDependencyComplete(MethodInfo methodInfo, bool isSuccess)
		{
			if (Status != ExecutionStatus.NotReady)
				return;

			for (int i = 0; i < _executionData.MethodDependencies.Count; ++i)
			{
				if (_executionData.MethodDependencies[i].method == methodInfo && (isSuccess || _executionData.MethodDependencies[i].continueOnDependencyFailure) && !_methodDependenciesMet[i])
					_methodDependenciesMet[i] = true;
			}

			for (int i = 0; i < _executionData.ConstructorDependencies.Count; ++i)
			{
				if (_executionData.ConstructorDependencies[i].method == methodInfo && (isSuccess || _executionData.ConstructorDependencies[i].continueOnDependencyFailure) &&!_constructorDependenciesMet[i])
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

			Result<Option<object>, TestFailure> result;
			if (SynchronizationContext.Current != null)
			{
				var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
				result = await Task.Factory.StartNew(() => _execute.Invoke((methodArguments, constructorArguments), cancellationTokenSource), cancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap();
			}
			else
				result = await Task.Run(() => _execute.Invoke((methodArguments, constructorArguments), cancellationTokenSource), cancellationTokenSource.Token);

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
					  constructor => GetParametersAbortMessage(jobs, constructor.GetParameters(), _constructorArguments, _executionData.ConstructorParameterMethods, "Constructor Parameter", methodStack)
						.Concat(GetDependenciesAbortMessage(jobs, _constructorDependenciesMet, _executionData.ConstructorDependencies.Select(set => set.method).ToArray(), "Constructor Dependency", methodStack)),
					  Enumerable.Empty<string>
				  )
				  .Concat(GetParametersAbortMessage(jobs, Method.GetParameters(), _methodArguments, _executionData.ParameterMethods, "Method Parameter", methodStack))
				  .Concat(GetDependenciesAbortMessage(jobs, _methodDependenciesMet, _executionData.MethodDependencies.Select(set => set.method).ToArray(), "Method Dependency", methodStack))
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

		public static ExecutionJob Create(MethodInfo method, ExecutionData executionData, Func<(object[] methodArguments, object[] constructorArguments), CancellationTokenSource, Task<Result<Option<object>, TestFailure>>> execute, Action<string> abort)
			=> new ExecutionJob(method, executionData, (input, cts) => execute.Invoke((input.methodArguments, input.constructorArguments), cts), abort);
	}
}
