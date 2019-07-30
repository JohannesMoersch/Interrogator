using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.Http
{
	public class HttpAssertionException : Exception
	{
		internal HttpAssertionException(string message)
			: base(message)
		{
		}
	}
}
