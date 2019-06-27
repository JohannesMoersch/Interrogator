using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Interrogator.xUnit.Execution
{
	internal class ErrorIntegrationTest : LongLivedMarshalByRefObject, ITest
	{
		public ErrorIntegrationTest(ITestCase testCase)
			=> TestCase = testCase;

		public ITestCase TestCase { get; }

		public string DisplayName => TestCase.DisplayName;
	}
}
