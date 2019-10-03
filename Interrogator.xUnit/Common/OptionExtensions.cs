using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.xUnit.Common
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

		public static void Apply<TValue>(this Option<TValue> option, Action<TValue> onSome, Action onNone)
			=> option.Match(value => { onSome.Invoke(value); return 0; }, () => { onNone.Invoke(); return 0; });

	}
}
