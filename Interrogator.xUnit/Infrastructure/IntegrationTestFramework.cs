﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using Interrogator.xUnit.Discovery;
using Interrogator.xUnit.Execution;
using Xunit;

namespace Interrogator.xUnit.Infrastructure
{
	internal class IntegrationTestFramework : LongLivedMarshalByRefObject, ITestFramework
	{
		private readonly IMessageSink _diagnosticMessageSink;

		public IntegrationTestFramework(IMessageSink diagnosticMessageSink) 
			=> _diagnosticMessageSink = diagnosticMessageSink;

		public ISourceInformationProvider SourceInformationProvider { get; set; }

		public void Dispose() { }

		public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly)
			=> new CombinationTestFrameworkDiscoverer(assembly, SourceInformationProvider, _diagnosticMessageSink);

		public ITestFrameworkExecutor GetExecutor(AssemblyName assemblyName)
			=> new CombinationTestFrameworkExecutor(assemblyName, SourceInformationProvider, _diagnosticMessageSink);
	}
}
