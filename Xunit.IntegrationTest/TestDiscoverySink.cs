using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.IntegrationTest
{
	internal class TestDiscoverySink : IMessageSink
	{
		private readonly List<ITestCase> _testCases = new List<ITestCase>();

		private readonly TaskCompletionSource<IEnumerable<ITestCase>> _taskCompletionSource = new TaskCompletionSource<IEnumerable<ITestCase>>();

		bool IMessageSink.OnMessage(IMessageSinkMessage message)
		{
			if (message is IDiscoveryCompleteMessage)
				_taskCompletionSource.SetResult(_testCases);
			else if (message is ITestCaseDiscoveryMessage testCaseMessage)
				_testCases.Add(testCaseMessage.TestCase);

			return true;
		}

		public Task<IEnumerable<ITestCase>> GetTestCases()
			=> _taskCompletionSource.Task;
	}
}
