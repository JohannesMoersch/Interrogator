using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.IntegrationTest
{
	internal class IntegrationTestCase : TestMethodTestCase
	{
		public IntegrationTestCase() { }

		public IntegrationTestCase(TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod) 
			: base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, null)
		{
		}
	}
}
