using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.xUnit.Execution
{
	internal enum ExecutionStatus
	{
		NotReady,
		Ready,
		Executing,
		NotComplete,
		Complete
	}
}
