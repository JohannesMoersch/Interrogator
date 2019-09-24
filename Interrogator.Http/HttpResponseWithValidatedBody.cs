using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	public class HttpResponseWithValidatedBody<T>
	{
		public T Content { get; }

		internal HttpResponseWithValidatedBody(T content) 
			=> Content = content;
	}
}
