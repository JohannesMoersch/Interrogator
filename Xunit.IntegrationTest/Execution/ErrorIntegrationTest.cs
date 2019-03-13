using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Xunit.IntegrationTest.Execution
{
	internal class ErrorIntegrationTest : LongLivedMarshalByRefObject, ITest
	{
		public ErrorIntegrationTest(ITestCase testCase)
			=> TestCase = testCase;

		public ITestCase TestCase { get; }

		public string DisplayName => TestCase.DisplayName;
	}
}
