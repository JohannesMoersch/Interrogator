using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.xUnit.Common
{
	internal static class EnumerableExtensions
	{
		public static Result<TSuccess[], TFailure> TakeUntilFailure<TSuccess, TFailure>(this IEnumerable<Result<TSuccess, TFailure>> source)
		{
			TSuccess[] successes;

			if (source is ICollection<Result<TSuccess, TFailure>> collection)
				successes = new TSuccess[collection.Count];
			else if (source is IReadOnlyCollection<Result<TSuccess, TFailure>> readOnlyCollecton)
				successes = new TSuccess[readOnlyCollecton.Count];
			else
				successes = new TSuccess[4];

			int index = 0;
			foreach (var value in source)
			{
				if (value.Match(_ => false, _ => true))
					return Result.Failure<TSuccess[], TFailure>(value.Match(_ => default, failure => failure));

				if (index == successes.Length)
				{
					var old = successes;
					successes = new TSuccess[old.Length * 2];
					Array.Copy(old, successes, old.Length);
				}
				successes[index++] = value.Match(success => success, _ => default);
			}

			if (index != successes.Length)
			{
				var old = successes;
				successes = new TSuccess[index];
				Array.Copy(old, successes, index);
			}

			return Result.Success<TSuccess[], TFailure>(successes);
		}
	}
}
