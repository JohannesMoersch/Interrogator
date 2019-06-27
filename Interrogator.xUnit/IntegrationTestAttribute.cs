using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Interrogator.xUnit
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class IntegrationTestAttribute : FactAttribute
	{
	}
}
