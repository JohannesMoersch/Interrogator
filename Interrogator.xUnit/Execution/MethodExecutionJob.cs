using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Infrastructure;
using Interrogator.xUnit.Utilities;
using Xunit.Sdk;

namespace Interrogator.xUnit.Execution
{
	internal static class MethodExecutionJob
	{
		public static ExecutionJob Create(MethodInfo method, MethodInfo[] testMethods)
			=> ExecutionData
				.Create(method, testMethods)
				.Match
				(
					executionData => ExecutionJob.Create(method, executionData, (arguments, cts) => ExecuteTest(method, arguments.methodArguments, arguments.constructorArguments, cts), _ => { }),
					errorMessage => ExecutionJob.Create(method, ExecutionData.Empty, (arguments, cts) => Task.FromResult(Result.Failure<Option<object>, TestFailure>(new TestFailure(errorMessage))), _ => { })
				);

		private static async Task<Result<Option<object>, TestFailure>> ExecuteTest(MethodInfo method, object[] methodArguments, object[] constructorArguments, CancellationTokenSource cancellationTokenSource)
		{
			if (method.TryGetSkipReason(out var skipReason))
				return Result.Failure<Option<object>, TestFailure>(new TestFailure(skipReason));

			var methodInfo = new ReflectionMethodInfo(method);
			var typeInfo = new ReflectionTypeInfo(method.DeclaringType);
			var assemblyInfo = new ReflectionAssemblyInfo(method.DeclaringType.Assembly);

			var testMethod = new TestMethod(new TestClass(new TestCollection(new TestAssembly(assemblyInfo), null, String.Empty), typeInfo), methodInfo);

			try
			{
				var testCase = new IntegrationTestCase(new DummyMessageBus(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.All, new TestMethodWrapper(testMethod));

				return await testCase.Execute(constructorArguments, methodArguments, new DummyMessageBus(), new ExceptionAggregator(), cancellationTokenSource);
			}
			catch (Exception ex)
			{
				return Result.Failure<Option<object>, TestFailure>(new TestFailure(ex));
			}
		}
	}
}
