using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.IntegrationTest.Common;
using Xunit.IntegrationTest.Infrastructure;
using Xunit.IntegrationTest.Utilities;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal static class MethodExecutionJob
	{
		public static ExecutionJob Create(MethodInfo method)
			=> ExecutionJob
				.Create
				(
					 method,
					 (arguments, cts) => ExecuteTest(method, arguments, cts),
					 _ => { }
				);

		private static async Task<Result<Option<object>, Exception>> ExecuteTest(MethodInfo method, object[] arguments, CancellationTokenSource cancellationTokenSource)
		{
			var methodInfo = new ReflectionMethodInfo(method);
			var typeInfo = new ReflectionTypeInfo(method.DeclaringType);
			var assemblyInfo = new ReflectionAssemblyInfo(method.DeclaringType.Assembly);

			var testMethod = new TestMethod(new TestClass(new TestCollection(new TestAssembly(assemblyInfo), null, String.Empty), typeInfo), methodInfo);

			try
			{
				var obj = Activator.CreateInstance(method.DeclaringType);

				var testCase = new IntegrationTestCase(new DummyMessageBus(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.All, new TestMethodWrapper(testMethod));

				return await testCase.Execute(Array.Empty<object>(), arguments, new DummyMessageBus(), new ExceptionAggregator(), cancellationTokenSource);
			}
			catch (Exception ex)
			{
				return Result.Failure<Option<object>, Exception>(ex);
			}
		}
	}
}
