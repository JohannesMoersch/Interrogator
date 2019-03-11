using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Xunit.IntegrationTest
{
	public class IntegrationTest : LongLivedMarshalByRefObject, ITest
	{
		public IntegrationTest(ITestCase testCase)
			=> TestCase = testCase;

		public ITestCase TestCase { get; }

		public string DisplayName => TestCase.DisplayName;
	}
}
