using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Interrogator.xUnit.Common;
using Interrogator.xUnit.Infrastructure;
using Xunit.Sdk;

namespace Interrogator.xUnit.Execution
{
	internal class IntegrationTest : XunitTest
	{
		public IntegrationTest(IntegrationTestCase testCase)
			: base(testCase, testCase.DisplayName)
		{
		}

		public new IntegrationTestCase TestCase => base.TestCase as IntegrationTestCase;
	}
}
