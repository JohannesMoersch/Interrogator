using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.xUnit.Common
{
	internal struct Option<TValue> : IEquatable<Option<TValue>>
	{
		private readonly bool _hasValue;

		private readonly TValue _value;

		internal Option(bool hasValue, TValue value)
		{
			_hasValue = hasValue;

			_value = value;
		}

		public TResult Match<TResult>(Func<TValue, TResult> some, Func<TResult> none)
		{
			if (some == null)
				throw new ArgumentNullException(nameof(some));

			if (none == null)
				throw new ArgumentNullException(nameof(none));

			return _hasValue ? some.Invoke(_value) : none.Invoke();
		}

		public bool HasValue => _hasValue;

		public bool Equals(Option<TValue> other)
			=> _hasValue == other._hasValue && (!_hasValue || Equals(_value, other._value));

		public override int GetHashCode()
			=> _hasValue ? _value.GetHashCode() * 31 : 0;

		public override bool Equals(object obj)
			=> obj is Option<TValue> option && Equals(option);

		public override string ToString()
			=> _hasValue ? $"Some:{_value}" : "None";

		public static bool operator ==(Option<TValue> left, Option<TValue> right)
			=> left.Equals(right);

		public static bool operator !=(Option<TValue> left, Option<TValue> right)
			=> !left.Equals(right);
	}

	internal static class Option
	{
		public static Option<T> Some<T>(T value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return new Option<T>(true, value);
		}

		public static Option<T> None<T>()
			=> new Option<T>(false, default);

		public static Option<T> FromNullable<T>(T value)
			where T : class
			=> value != null
				? Some(value)
				: None<T>();

		public static Option<T> Create<T>(bool isSome, Func<T> valueFactory)
		{
			if (valueFactory == null)
				throw new ArgumentNullException(nameof(valueFactory));

			return isSome
				? Some(valueFactory.Invoke())
				: None<T>();
		}
	}
}
