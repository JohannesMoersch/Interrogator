using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.IntegrationTest.Common;

namespace Xunit.IntegrationTest.Execution
{
	internal class ExecutionJob
	{
		private ExecutionJob(MethodInfo method, MethodInfo[] parameterMethods)
		{
			Method = method;
			Status = parameterMethods.Length == 0 ? ExecutionStatus.Ready : ExecutionStatus.NotReady;
			ParameterMethods = parameterMethods;
			ErrorMessage = Option.None<string>();

			_values = new Option<object>[ParameterMethods.Count];
		}

		private ExecutionJob(MethodInfo method, string errorMessage)
		{
			Method = method;
			Status = ExecutionStatus.Ready;
			ParameterMethods = new MethodInfo[0];
			ErrorMessage = Option.Some(errorMessage ?? throw new ArgumentNullException(nameof(errorMessage)));
		}

		public MethodInfo Method { get; }

		public ExecutionStatus Status { get; private set; }

		public IReadOnlyList<MethodInfo> ParameterMethods { get; }

		public Option<string> ErrorMessage { get; }

		private Option<object>[] _values;

		public void SetParameter(MethodInfo methodInfo, object value)
		{
			if (Status != ExecutionStatus.NotReady)
				return;

			bool ready = true;
			for (int i = 0; i < ParameterMethods.Count; ++i)
			{
				if (ParameterMethods[i] == methodInfo && _values[i] == Option.None<object>())
					_values[i] = Option.Some(value);

				if (_values[i] == Option.None<object>())
					ready = false;
			}

			if (ready)
				Status = ExecutionStatus.Ready;
		}

		public static ExecutionJob Create(MethodInfo testMethod, string errorMessage) 
			=> new ExecutionJob(testMethod, errorMessage);

		public static ExecutionJob Create(MethodInfo testMethod)
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
					methods => new ExecutionJob(testMethod, methods),
					error => new ExecutionJob(testMethod, error)
				);

		private static bool IsReturnTypeCompatible(Type returnType, Type targetType)
		{
			if (returnType.IsConstructedGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
				returnType = returnType.GetGenericArguments()[0];

			return targetType.IsAssignableFrom(returnType);
		}
	}
}
