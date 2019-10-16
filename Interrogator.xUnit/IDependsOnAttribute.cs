using Interrogator.xUnit.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Interrogator.xUnit
{
	internal interface IDependsOnAttribute
	{
		bool ContinueOnDependencyFailure { get; }

		Result<Option<MethodInfo>, string> TryGetMethod(Type containingType, MemberInfo member, MethodInfo[] testMethods);
	}
}
