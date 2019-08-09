using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Interrogator.Http
{
	public static class HttpResponseExtensions
	{
		public static async Task<HttpResponseWithExpectedStatusCode> IsSuccess(this Task<HttpResponse> response)
		{
			var value = await response;

			if ((int)value.StatusCode < 200 || (int)value.StatusCode > 299)
				throw new HttpAssertionException($"Expected response status code to be a success, but response status code was {(int)value.StatusCode} ({value.StatusCode}).");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> Is3xx(this Task<HttpResponse> response)
		{
			var value = await response;

			if ((int)value.StatusCode < 300 || (int)value.StatusCode > 399)
				throw new HttpAssertionException($"Expected response status code to be 3xx, but response status code was {(int)value.StatusCode} ({value.StatusCode}).");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> Is4xx(this Task<HttpResponse> response)
		{
			var value = await response;

			if ((int)value.StatusCode < 400 || (int)value.StatusCode > 499)
				throw new HttpAssertionException($"Expected response status code to be 4xx, but response status code was {(int)value.StatusCode} ({value.StatusCode}).");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> Is5xx(this Task<HttpResponse> response)
		{
			var value = await response;

			if ((int)value.StatusCode < 500 || (int)value.StatusCode > 599)
				throw new HttpAssertionException($"Expected response status code to be 5xx, but response status code was {(int)value.StatusCode} ({value.StatusCode}).");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static Task<HttpResponseWithExpectedStatusCode> IsOk(this Task<HttpResponse> response)
			=> response.IsStatusCode(HttpStatusCode.OK);

		public static Task<HttpResponseWithExpectedStatusCode> IsCreated(this Task<HttpResponse> response)
			=> response.IsStatusCode(HttpStatusCode.Created);

		public static Task<HttpResponseWithExpectedStatusCode> IsAccepted(this Task<HttpResponse> response)
			=> response.IsStatusCode(HttpStatusCode.Accepted);

		public static Task<HttpResponseWithExpectedStatusCode> IsBadRequest(this Task<HttpResponse> response)
			=> response.IsStatusCode(HttpStatusCode.BadRequest);

		public static Task<HttpResponseWithExpectedStatusCode> IsUnauthorized(this Task<HttpResponse> response)
			=> response.IsStatusCode(HttpStatusCode.Unauthorized);

		public static Task<HttpResponseWithExpectedStatusCode> IsForbidden(this Task<HttpResponse> response)
			=> response.IsStatusCode(HttpStatusCode.Forbidden);

		public static Task<HttpResponseWithExpectedStatusCode> IsNotFound(this Task<HttpResponse> response)
			=> response.IsStatusCode(HttpStatusCode.NotFound);

		public static Task<HttpResponseWithExpectedStatusCode> IsConflict(this Task<HttpResponse> response)
			=> response.IsStatusCode(HttpStatusCode.Conflict);

		public static async Task<HttpResponseWithExpectedStatusCode> IsStatusCode(this Task<HttpResponse> response, HttpStatusCode statusCode)
		{
			var value = await response;

			if (value.StatusCode != statusCode)
				throw new HttpAssertionException($"Expected response status code to be {(int)statusCode} ({statusCode}), but response status code was {(int)value.StatusCode} ({value.StatusCode}).\n\nResponse body: {await value.Content.ReadAsStringAsync()}");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}
	}
}
