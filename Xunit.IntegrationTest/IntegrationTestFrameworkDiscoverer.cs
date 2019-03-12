using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.IntegrationTest
{
	internal class IntegrationTestFrameworkDiscoverer : TestFrameworkDiscoverer
	{
		public IntegrationTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink)
			: base(assemblyInfo, sourceProvider, diagnosticMessageSink)
		{
			TestFrameworkDisplayName = "Integration Test Framework";

			_testAssembly = new TestAssembly(assemblyInfo);
		}

		private readonly ITestAssembly _testAssembly;

		protected override ITestClass CreateTestClass(ITypeInfo @class)
			=> new ClassWrapper(@class);

		protected override bool FindTestsForType(ITestClass classInfo, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			foreach (var method in classInfo.Class.GetMethods(true))
			{
				if (method.IsAbstract || method.IsGenericMethodDefinition)
					continue;

				if (!method.IsIntegrationTest())
					continue;

				var testCollection = new TestCollection(_testAssembly, null, String.Empty);
				var testClass = new TestClass(testCollection, classInfo.Class);
				var testMethod = new TestMethod(testClass, method);

				var runtimeMethod = method.ToRuntimeMethod();

				var parameterWithoutFromAttribute = runtimeMethod
					.GetParameters()
					.FirstOrDefault(p => p.TryGetFromAttribute().Match(_ => false, () => true));

				if (parameterWithoutFromAttribute == null)
				{
					var testCase = new IntegrationTestCase(discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);

					messageBus.QueueMessage(new TestCaseDiscoveryMessage(testCase));
				}
				else
				{
					var message = $"Parameter '{parameterWithoutFromAttribute.Name}' on test method '{runtimeMethod.DeclaringType.Name}.{method.Name}' requires a [From] attribute.";
					var testCase = new ErrorIntegrationTestCase(TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod, message);

					messageBus.QueueMessage(new TestCaseDiscoveryMessage(testCase));
				}
			}

			return true;
		}

		private class ClassWrapper : ITestClass
		{
			public ClassWrapper() { }

			public ClassWrapper(ITypeInfo typeInfo) 
				=> Class = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));

			public ITypeInfo Class { get; }

			ITestCollection ITestClass.TestCollection => throw new NotImplementedException();
			void IXunitSerializable.Deserialize(IXunitSerializationInfo info) => throw new NotImplementedException();
			void IXunitSerializable.Serialize(IXunitSerializationInfo info) => throw new NotImplementedException();
		}
	}
}
