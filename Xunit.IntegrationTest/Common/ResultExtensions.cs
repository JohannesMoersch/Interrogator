using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.IntegrationTest.Common
{
	internal static class ResultExtensions
	{
		public static Result<TResult, TFailure> Select<TSuccess, TFailure, TResult>(this Result<TSuccess, TFailure> result, Func<TSuccess, TResult> select)
			=> result.Match(success => Result.Success<TResult, TFailure>(select.Invoke(success)), Result.Failure<TResult, TFailure>);

		public static Result<TResult, TFailure> Bind<TSuccess, TFailure, TResult>(this Result<TSuccess, TFailure> result, Func<TSuccess, Result<TResult, TFailure>> bind)
			=> result.Match(success => bind.Invoke(success), Result.Failure<TResult, TFailure>);

		public static Result<TSuccess, TResult> MapFailure<TSuccess, TFailure, TResult>(this Result<TSuccess, TFailure> result, Func<TFailure, TResult> mapFailure)
			=> result.Match(Result.Success<TSuccess, TResult>, failure => Result.Failure<TSuccess, TResult>(mapFailure.Invoke(failure)));

		public static void Apply<TSuccess, TFailure>(this Result<TSuccess, TFailure> result, Action<TSuccess> onSuccess, Action<TFailure> onFailure)
			=> result.Match(success => { onSuccess.Invoke(success); return 0; }, failure => { onFailure.Invoke(failure); return 0; });
	}
}
