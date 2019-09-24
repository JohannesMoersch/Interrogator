using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace Interrogator.Http.Tests
{
	public static class FluentAssertionsExtensions
	{
		public static async Task<StringAssertions> Should(this Task<string> value)
			=> (await value)
				.Should();

		public static async Task<AndConstraint<StringAssertions>> Be(this Task<StringAssertions> assertions, string expected, string because = "", params object[] becauseArgs)
			=> (await assertions)
				.Be(expected, because, becauseArgs);
	}
}
