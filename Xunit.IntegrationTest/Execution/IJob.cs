using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.IntegrationTest.Execution
{
	internal interface IJob
	{
		ExecutionStatus Status { get; }

		IReadOnlyList<MethodInfo> ParameterMethods { get; }

		void SetParameter(MethodInfo methodInfo, object value);

		Task<object> Execute(IMessageBus messageBus, ExceptionAggregator exceptionAggregator, CancellationTokenSource cancellationTokenSource);

		void Abort(IMessageBus messageBus, ExceptionAggregator exceptionAggregator, CancellationTokenSource cancellationTokenSource);
	}
}
