using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.Http
{
	[Flags]
	public enum HasHeaderFlags
	{
		None = 0,
		OrderMustMatch = 1,
		MustBeExactMatch = 2,
		ValuesMustBeUnique = 4
	}
}
