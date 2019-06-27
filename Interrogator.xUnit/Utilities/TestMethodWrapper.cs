using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Interrogator.xUnit.Utilities
{
	internal class TestMethodWrapper : ITestMethod
	{
		private readonly ITestMethod _testMethod;

		[Obsolete]
		public TestMethodWrapper() { }

		public TestMethodWrapper(ITestMethod testMethod)
			=> _testMethod = testMethod ?? throw new ArgumentNullException(nameof(testMethod));

		public IMethodInfo Method => new MethodInfoWrapper(_testMethod.Method);

		public ITestClass TestClass => _testMethod.TestClass;

		public void Deserialize(IXunitSerializationInfo info)
			=> _testMethod.Deserialize(info);

		public void Serialize(IXunitSerializationInfo info)
			=> _testMethod.Serialize(info);
	}
}
