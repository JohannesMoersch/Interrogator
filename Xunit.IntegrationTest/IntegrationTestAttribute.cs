using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.IntegrationTest
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class IntegrationTestAttribute : Attribute
	{
	}
}
