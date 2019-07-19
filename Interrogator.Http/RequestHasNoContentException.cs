using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.Http
{
	public class RequestHasNoContentException : Exception
	{
		public RequestHasNoContentException(string message)
			: base(message)
		{
		}
	}
}
