using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Interrogator.Http
{
	internal static class HttpRequestMessageExtensions
	{
		public static void AddHeaders(this HttpRequestMessage request, IEnumerable<HttpHeader> headers)
		{
			foreach (var header in headers.GroupBy(h => h.Name))
				request.AddHeader(header.Key, header.SelectMany(h => h.Values));
		}

		public static void AddHeader(this HttpRequestMessage message, string name, IEnumerable<string> values)
		{
			switch (name.ToLower())
			{
				// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.headers.httpcontentheaders
				case "allow":
				case "content-disposition":
				case "content-encoding":
				case "content-language":
				case "content-length":
				case "content-location":
				case "content-md5":
				case "content-range":
				case "content-type":
				case "expires":
				case "last-modified":
					if (message.Content == null)
						throw new RequestHasNoContentException($"Failed to add header \"{name}\" to request because request has no content.");

					message.Content.Headers.Remove(name);
					message.Content.Headers.TryAddWithoutValidation(name, values);

					break;
				default:
					message.Headers.Remove(name);
					message.Headers.TryAddWithoutValidation(name, values);

					break;
			}
		}

		public static async Task<HttpResponse> Send(this HttpClient client, HttpRequestMessage request)
		{
			var timer = new Stopwatch();
			timer.Start();

			var response = await client.SendAsync(request);

			timer.Stop();

			return new HttpResponse(response.StatusCode, GetHttpHeaders(response), response.Content, timer.Elapsed);
		}

		private static IEnumerable<HttpHeader> GetHttpHeaders(HttpResponseMessage response)
			=> response
				.Headers
				.Concat(response.Content.Headers)
				.Select(header => new HttpHeader(header.Key, header.Value.ToArray()));
	}
}
