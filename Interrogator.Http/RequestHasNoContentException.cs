using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.Http
{
	public class RequestHasNoContentException : Exception
	{
		internal RequestHasNoContentException(string message)
			: base(message)
		{
		}
	}
}
