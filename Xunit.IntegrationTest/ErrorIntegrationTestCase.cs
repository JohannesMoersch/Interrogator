using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.IntegrationTest
{
	internal class ErrorIntegrationTestCase : IntegrationTestCase
	{
		public ErrorIntegrationTestCase() { }

		public ErrorIntegrationTestCase(TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, string errorMessage)
			: base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod) 
			=> ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));

		public string ErrorMessage { get; private set; }

		public override void Serialize(IXunitSerializationInfo data)
		{
			data.AddValue(nameof(ErrorMessage), ErrorMessage);

			base.Serialize(data);
		}

		public override void Deserialize(IXunitSerializationInfo data)
		{
			ErrorMessage = data.GetValue<string>(nameof(ErrorMessage));

			base.Deserialize(data);
		}
	}
}
