using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Interrogator.Http
{
	public static class HttpResponseWithExpectedStatusCodeExtensions
	{
		public static Task<HttpResponseWithExpectedStatusCode> HasHeader(this Task<HttpResponseWithExpectedStatusCode> response, string name)
			=> response.HasHeader(name, _ => { });

		public static async Task<HttpResponseWithExpectedStatusCode> HasHeader(this Task<HttpResponseWithExpectedStatusCode> response, string name, Action<IReadOnlyList<string>> assertOnHeaderValues)
		{
			var value = await response;

			var headerOrNull = value
				.Headers
				.Where(h => h.Name == name)
				.Select(h => (HttpHeader?)h)
				.FirstOrDefault();

			if (headerOrNull is HttpHeader header)
				assertOnHeaderValues.Invoke(header.Value);
			else
				throw new HttpAssertionException($"Response does not contain a header with the name \"{name}\".");

			return value;
		}

		public static Task<HttpResponseWithExpectedStatusCode> HasHeaderValues(this Task<HttpResponseWithExpectedStatusCode> response, string name, params string[] values)
			=> response
				.HasHeader
				(
					name,
					headerValues =>
					{
						var missingValues = values
							.Where(value => !headerValues.Any(v => String.Equals(v, value, StringComparison.InvariantCultureIgnoreCase)))
							.ToArray();

						if (missingValues.Any())
							throw new HttpAssertionException($"Response header with the name \"{name}\" was expected to contain {values.ToStringValueList()}.");
					}
				);

		public static async Task AssertBody(this Task<HttpResponseWithExpectedStatusCode> response, Func<HttpContent, Task> assertOnBody)
		{
			var value = await response;

			await assertOnBody.Invoke(value.Content);
		}

		public static Task AssertStringBody(this Task<HttpResponseWithExpectedStatusCode> response, Action<string> assertOnBody)
			=> response.AssertBody(async httpContent => assertOnBody.Invoke(await httpContent.ReadAsStringAsync()));

		public static Task AssertJsonBody(this Task<HttpResponseWithExpectedStatusCode> response, Action<string> assertOnBody)
			=> response
				.HasHeaderValues("Content-Type", "application/json")
				.AssertBody(async httpContent => assertOnBody.Invoke(await httpContent.ReadAsStringAsync()));
	}
}
