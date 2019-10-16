using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Interrogator.xUnit.Tests.NotConcurrent
{
	public class ConcurrencyTestLock
	{
		private class ReleaseDisposable : IDisposable
		{
			private AutoResetEvent _resetEvent;

			public ReleaseDisposable(AutoResetEvent resetEvent) 
				=> _resetEvent = resetEvent;

			public void Dispose()
			{
				lock (_resetEvent)
				{
					_resetEvent?.Set();
					_resetEvent = null;
				}
			}
		}

		private AutoResetEvent _resetEvent = new AutoResetEvent(true);

		public IDisposable Acquire([CallerMemberName]string caller = null)
		{
			if (!_resetEvent.WaitOne(0))
				throw new Exception("Concurrency detected.");

			return new ReleaseDisposable(_resetEvent);
		}
	}
}
