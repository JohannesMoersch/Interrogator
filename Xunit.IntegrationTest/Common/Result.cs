using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.IntegrationTest.Common
{
	internal struct Result<TSuccess, TFailure> : IEquatable<Result<TSuccess, TFailure>>
	{
		private readonly bool? _isSuccess;

		private readonly TSuccess _success;

		private readonly TFailure _failure;

		internal Result(bool isSuccess, TSuccess success, TFailure failure)
		{
			_isSuccess = isSuccess;
			_success = success;
			_failure = failure;
		}

		private bool IsSuccess()
			=> _isSuccess ?? throw new ResultNotInitializedException();

		public TResult Match<TResult>(Func<TSuccess, TResult> success, Func<TFailure, TResult> failure)
		{
			if (success == null)
				throw new ArgumentNullException(nameof(success));

			if (failure == null)
				throw new ArgumentNullException(nameof(failure));

			return IsSuccess() ? success.Invoke(_success) : failure.Invoke(_failure);
		}

		public bool Equals(Result<TSuccess, TFailure> other)
			=> IsSuccess() == other.IsSuccess()
				&& (IsSuccess() ? Equals(_success, other._success) : Equals(_failure, other._failure));

		public override int GetHashCode()
			=> IsSuccess() ? _success.GetHashCode() * 31 : _failure.GetHashCode() * 31;

		public override bool Equals(object obj)
			=> obj is Result<TSuccess, TFailure> result && Equals(result);

		public override string ToString()
			=> IsSuccess() ? $"Success:{_success}" : $"Failure:{_failure}";

		public static bool operator ==(Result<TSuccess, TFailure> left, Result<TSuccess, TFailure> right)
			=> left.Equals(right);

		public static bool operator !=(Result<TSuccess, TFailure> left, Result<TSuccess, TFailure> right)
			=> !left.Equals(right);
	}

	internal static class Result
	{
		public static Result<TSuccess, TFailure> Success<TSuccess, TFailure>(TSuccess success)
		{
			if (success == null)
				throw new ArgumentNullException(nameof(success));

			return new Result<TSuccess, TFailure>(true, success, default);
		}

		public static Result<TSuccess, TFailure> Failure<TSuccess, TFailure>(TFailure failure)
		{
			if (failure == null)
				throw new ArgumentNullException(nameof(failure));

			return new Result<TSuccess, TFailure>(false, default, failure);
		}

		public static Result<TSuccess, TFailure> Create<TSuccess, TFailure>(bool isSuccess, Func<TSuccess> successFactory, Func<TFailure> failureFactory)
		{
			if (successFactory == null)
				throw new ArgumentNullException(nameof(successFactory));

			if (failureFactory == null)
				throw new ArgumentNullException(nameof(failureFactory));

			return isSuccess
				? Success<TSuccess, TFailure>(successFactory.Invoke())
				: Failure<TSuccess, TFailure>(failureFactory.Invoke());
		}
	}
}
