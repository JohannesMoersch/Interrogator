using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.IntegrationTest.Common;
using Xunit.IntegrationTest.Infrastructure;
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
			try
			{
				var obj = Activator.CreateInstance(method.DeclaringType);

				await Task.CompletedTask;

				var result = method.Invoke(obj, arguments);

				return Result.Success<Option<object>, Exception>(Option.FromNullable(result));
			}
			catch (Exception ex)
			{
				return Result.Failure<Option<object>, Exception>(ex);
			}
		}
	}
}
