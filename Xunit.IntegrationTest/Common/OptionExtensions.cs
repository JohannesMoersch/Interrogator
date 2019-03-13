using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.IntegrationTest.Common
{
	internal static class OptionExtensions
	{
		public static Result<TValue, TFailure> ToResult<TValue, TFailure>(this Option<TValue> value, Func<TFailure> failureFactory)
			=> value
				.Match
				(
					Result.Success<TValue, TFailure>,
					() => Result.Failure<TValue, TFailure>(failureFactory.Invoke())
				);
	}
}
