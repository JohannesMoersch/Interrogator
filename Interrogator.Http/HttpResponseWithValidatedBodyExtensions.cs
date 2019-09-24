using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Interrogator.Http
{
	public static class HttpResponseWithValidatedBodyExtensions
	{
		public static async Task<T> ReturnFromBody<T>(this Task<HttpResponseWithValidatedBody<HttpContent>> response, Func<HttpContent, Task<T>> contentReader)
			=> await contentReader.Invoke((await response).Content);

		public static async Task<T> ReturnFromBody<T>(this Task<HttpResponseWithValidatedBody<string>> response, Func<string, T> contentReader)
			=> contentReader.Invoke((await response).Content);
	}
}
