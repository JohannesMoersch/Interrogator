﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Interrogator.xUnit.Utilities
{
	internal class FilterableMessageSink : IMessageSink
	{
		public FilterableMessageSink(IMessageSink messageSink, Func<IMessageSinkMessage, IMessageSinkMessage> filter)
		{
			_messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
		}

		private readonly IMessageSink _messageSink;
		private readonly Func<IMessageSinkMessage, IMessageSinkMessage> _filter;

		public bool OnMessage(IMessageSinkMessage message)
		{
			message = _filter.Invoke(message);

			if (message != null)
				return _messageSink.OnMessage(message);

			return true;
		}
	}
}
