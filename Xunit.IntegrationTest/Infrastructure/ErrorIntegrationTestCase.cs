using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Infrastructure
{
	internal class ErrorIntegrationTestCase : IntegrationTestCase
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public ErrorIntegrationTestCase() { }

		public ErrorIntegrationTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, string errorMessage)
			: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod) 
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
