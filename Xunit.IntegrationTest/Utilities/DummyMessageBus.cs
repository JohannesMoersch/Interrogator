using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Utilities
{
	internal class DummyMessageBus : IMessageSink, IMessageBus
	{
		public void Dispose() { }
		public bool OnMessage(IMessageSinkMessage message) => true;
		public bool QueueMessage(IMessageSinkMessage message) => true;
	}
}
