using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using Xunit.IntegrationTest.Common;
using Xunit.IntegrationTest.Infrastructure;

namespace Xunit.IntegrationTest.Execution
{
	internal class IntegrationTest : LongLivedMarshalByRefObject, ITest
	{
		private IntegrationTest(ITestCase testCase, MemberInfo[] dependencies) 
			=> TestCase = testCase;

		public ITestCase TestCase { get; }

		public string DisplayName => TestCase.DisplayName;

		private readonly object[] _parameters;

		public static Result<IntegrationTest, string> Create(ITestCase testCase)
		{
			if (testCase is ErrorIntegrationTestCase errorTestCase)
				return Result.Failure<IntegrationTest, string>(errorTestCase.ErrorMessage);

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
				.Select(methods => new IntegrationTest(testCase, methods));
		}
	}
}
