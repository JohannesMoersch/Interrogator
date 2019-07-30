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
			=> response
				.AssertHeader
				(
					name, 
					headerOrNull =>
					{
						if (!headerOrNull.HasValue)
							throw new HttpAssertionException($"Response does not contain a header with the name \"{name}\".");
					}
				);

		public static Task<HttpResponseWithExpectedStatusCode> HasHeaderValues(this Task<HttpResponseWithExpectedStatusCode> response, string name, params string[] values)
			=> response
				.AssertHeader
				(
					name,
					headerOrNull =>
					{
						if (headerOrNull is HttpHeader header)
						{
							var missingValues = values
								.Where(value => !header.Values.Any(v => String.Equals(v, value, StringComparison.InvariantCultureIgnoreCase)))
								.ToArray();

							if (missingValues.Any())
								throw new HttpAssertionException($"Response header with the name \"{name}\" was expected to contain {values.ToStringValueList()}.");
						}
						else
							throw new HttpAssertionException($"Response does not contain a header with the name \"{name}\".");
					}
				);

		public static Task<HttpResponseWithExpectedStatusCode> DoesNotHaveHeader(this Task<HttpResponseWithExpectedStatusCode> response, string name)
			=> response
				.AssertHeader
				(
					name,
					headerOrNull =>
					{
						if (headerOrNull.HasValue)
							throw new HttpAssertionException($"Response contains a header with the name \"{name}\".");
					}
				);

		public static async Task<HttpResponseWithExpectedStatusCode> AssertHeader(this Task<HttpResponseWithExpectedStatusCode> response, string name, Action<HttpHeader?> assertOnHeader)
		{
			var value = await response;

			var headerOrNull = value
				.Headers
				.Where(h => h.Name == name)
				.Select(h => (HttpHeader?)h)
				.FirstOrDefault();

			assertOnHeader.Invoke(headerOrNull);

			return value;
		}

		public static Task HasBody(this Task<HttpResponseWithExpectedStatusCode> response)
			=> response
				.AssertBody(async content => 
				{
					if ((await content.ReadAsByteArrayAsync()).Length == 0)
						throw new HttpAssertionException($"Response does not contain a body.");
				});

		public static Task DoesNotHaveBody(this Task<HttpResponseWithExpectedStatusCode> response)
			=> response
				.AssertBody(async content =>
				{
					if ((await content.ReadAsByteArrayAsync()).Length > 0)
						throw new HttpAssertionException($"Response contains a body.");
				});

		public static async Task AssertBody(this Task<HttpResponseWithExpectedStatusCode> response, Func<HttpContent, Task> assertOnBody)
		{
			var value = await response;

			await assertOnBody.Invoke(value.Content);
		}

		public static Task AssertStringBody(this Task<HttpResponseWithExpectedStatusCode> response, Action<string> assertOnBody)
			=> response.AssertBody(async httpContent => assertOnBody.Invoke(await httpContent.ReadAsStringAsync()));

		public static Task AssertJsonBody(this Task<HttpResponseWithExpectedStatusCode> response, Action<string> assertOnBody)
			=> response
				.AssertHeader("Content-Type", headerOrNull =>
				{
					if (!(headerOrNull is HttpHeader header) || header.Values.All(value => !value.StartsWith("application/json", StringComparison.InvariantCultureIgnoreCase)))
						throw new HttpAssertionException("Body does not have a content type of \"application/json\".");
				})
				.AssertBody(async httpContent => assertOnBody.Invoke(await httpContent.ReadAsStringAsync()));
	}
}
