using System;
using System.Collections.Generic;
using System.Text;
using Interrogator.xUnit.Common;

namespace Interrogator.xUnit.Execution
{
	internal class TestFailure
	{
		private Exception _exception;

		private string _skipMessage;

		private bool _isFaulted;

		public TestFailure(Exception exception)
		{
			_exception = exception;
			_isFaulted = true;
		}

		public TestFailure(string skipMessage)
		{
			_skipMessage = skipMessage;
			_isFaulted = false;
		}

		public T Match<T>(Func<Exception, T> onException, Func<string, T> onSkip)
			=> _isFaulted
				? onException.Invoke(_exception)
				: onSkip.Invoke(_skipMessage);
	}
}
