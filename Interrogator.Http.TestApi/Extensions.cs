using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Interrogator.Http.TestApi
{
	public static class Extensions
	{
		public static OkResult ThenOk<T>(this AndConstraint<T> _)
			=> new OkResult();
	}
}
