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
				throw new HttpAssertionException("Response status code is not a success.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> IsOK(this Task<HttpResponse> response)
		{
			var value = await response;

			if (value.StatusCode != HttpStatusCode.OK)
				throw new HttpAssertionException("Response status code is not OK.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> IsCreated(this Task<HttpResponse> response)
		{
			var value = await response;

			if (value.StatusCode != HttpStatusCode.Created)
				throw new HttpAssertionException("Response status code is not Created.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> IsAccepted(this Task<HttpResponse> response)
		{
			var value = await response;

			if (value.StatusCode != HttpStatusCode.Accepted)
				throw new HttpAssertionException("Response status code is not Accepted.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> IsBadRequest(this Task<HttpResponse> response)
		{
			var value = await response;

			if (value.StatusCode != HttpStatusCode.BadRequest)
				throw new HttpAssertionException("Response status code is not BadRequest.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> IsUnauthorized(this Task<HttpResponse> response)
		{
			var value = await response;

			if (value.StatusCode != HttpStatusCode.Unauthorized)
				throw new HttpAssertionException("Response status code is not Unauthorized.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> IsForbidden(this Task<HttpResponse> response)
		{
			var value = await response;

			if (value.StatusCode != HttpStatusCode.Forbidden)
				throw new HttpAssertionException("Response status code is not Forbidden.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> IsNotFound(this Task<HttpResponse> response)
		{
			var value = await response;

			if (value.StatusCode != HttpStatusCode.Forbidden)
				throw new HttpAssertionException("Response status code is not NotFound.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> IsConflict(this Task<HttpResponse> response)
		{
			var value = await response;

			if (value.StatusCode != HttpStatusCode.Conflict)
				throw new HttpAssertionException("Response status code is not Conflict.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> Is3xx(this Task<HttpResponse> response)
		{
			var value = await response;

			if ((int)value.StatusCode < 300 || (int)value.StatusCode > 399)
				throw new HttpAssertionException("Response status code is not a 3xx.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> Is4xx(this Task<HttpResponse> response)
		{
			var value = await response;

			if ((int)value.StatusCode < 400 || (int)value.StatusCode > 499)
				throw new HttpAssertionException("Response status code is not a 4xx.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> Is5xx(this Task<HttpResponse> response)
		{
			var value = await response;

			if ((int)value.StatusCode < 500 || (int)value.StatusCode > 599)
				throw new HttpAssertionException("Response status code is not a 5xx.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}

		public static async Task<HttpResponseWithExpectedStatusCode> IsStatusCode(this Task<HttpResponse> response, HttpStatusCode statusCode)
		{
			var value = await response;

			if (value.StatusCode != statusCode)
				throw new HttpAssertionException($"Response status code is not {statusCode}.");

			return new HttpResponseWithExpectedStatusCode(value.Headers, value.Content, value.RequestDuration);
		}
	}
}
