using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.IntegrationTest.Common;
using Xunit.IntegrationTest.Infrastructure;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal class Job
	{
		private readonly MethodInfo _methodInfo;

		private readonly MethodInfo[] _parameterSources;

		private readonly object[] _arguments;

		private int _numberOfDependenciesMet;

		public bool ReadyToExecute => _numberOfDependenciesMet == _parameterSources.Length; 

		private Job(MethodInfo methodInfo, MethodInfo[] parameterSourceMethods)
		{
			_methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			_parameterSources = parameterSourceMethods ?? throw new ArgumentNullException(nameof(parameterSourceMethods));
			_arguments = new object[parameterSourceMethods.Length];
		}

		public void SetDependency(MethodInfo method, object value)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			for (int i = 0; i < _parameterSources.Length; ++i)
			{
				if (_parameterSources[i] == method)
				{
					_parameterSources[i] = null;
					_arguments[i] = value;
					++_numberOfDependenciesMet;
				}
			}
		}

		public static Result<Job, string> Create(ITestCase testCase)
		{
			if (testCase is ErrorIntegrationTestCase errorTestCase)
				return Result.Failure<Job, string>(errorTestCase.ErrorMessage);

			var testMethod = testCase
				.TestMethod
				.Method
				.ToRuntimeMethod();

			return testMethod
				.GetParameters()
				.Select(parameter => parameter
					.GetFromAttribute()
					.Bind(att => att.TryGetMethod(testMethod.DeclaringType))
					.Bind(method => Result
						.Create
						(
							parameter.ParameterType.IsAssignableFrom(method.ReturnType),
							() => method,
							() => $"Cannot cast return type of source method '{method.DeclaringType.Name}.{method.Name}' from '{method.ReturnType.Name}' to '{parameter.ParameterType.Name}' for parameter '{parameter.Name}' on method '{parameter.Member.DeclaringType.Name}.{parameter.Member.Name}'"
						)
					)
				)
				.TakeUntilFailure()
				.Select(methods => new Job(testMethod, methods));
		}
	}
}
