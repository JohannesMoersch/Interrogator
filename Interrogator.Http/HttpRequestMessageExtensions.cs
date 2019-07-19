using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Interrogator.Http
{
	internal static class HttpRequestMessageExtensions
	{
		public static void AddHeaders(this HttpRequestMessage request, IEnumerable<HttpHeader> headers)
		{
			foreach (var header in headers.GroupBy(h => h.Name))
				request.AddHeader(header.Key, header.Select(h => h.Value));
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
	}
}
