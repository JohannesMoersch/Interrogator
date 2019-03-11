using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Xunit.IntegrationTest
{
	internal class FilterableMessageSink : IMessageSink
	{
		public FilterableMessageSink(IMessageSink messageSink, Predicate<IMessageSinkMessage> filter)
		{
			_messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
		}

		private readonly IMessageSink _messageSink;
		private readonly Predicate<IMessageSinkMessage> _filter;

		public bool OnMessage(IMessageSinkMessage message)
		{
			if (_filter.Invoke(message))
				return _messageSink.OnMessage(message);

			return true;
		}
	}
}
