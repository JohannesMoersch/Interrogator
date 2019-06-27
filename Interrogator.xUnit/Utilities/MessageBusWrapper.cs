using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Interrogator.xUnit.Utilities
{
	internal class MessageBusWrapper : IMessageBus
	{
		private readonly IMessageBus _messageBus;
		private readonly Action<IMessageSinkMessage> _interceptor;

		public MessageBusWrapper(IMessageBus messageBus, Action<IMessageSinkMessage> interceptor)
		{
			_messageBus = messageBus;
			_interceptor = interceptor;
		}

		public void Dispose() 
			=> _messageBus.Dispose();

		public bool QueueMessage(IMessageSinkMessage message)
		{
			_interceptor.Invoke(message);

			return _messageBus.QueueMessage(message);
		}
	}
}
