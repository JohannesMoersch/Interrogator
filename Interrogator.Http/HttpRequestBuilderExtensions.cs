﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Interrogator.Http
{
	public static class HttpRequestBuilderExtensions
	{
		public static HttpRequestBuilder WithHeader(this HttpRequestBuilder requestBuilder, string name, string value)
			=> new HttpRequestBuilder(requestBuilder.Client, requestBuilder.Method, requestBuilder.Address, requestBuilder.Headers.Append(new HttpHeader(name, value)));

		public static HttpRequestBuilder WithAuthorization(this HttpRequestBuilder requestBuilder, string value)
			=> requestBuilder.WithHeader("Authorization", value);

		public static HttpRequestBuilder WithAcceptEncoding(this HttpRequestBuilder requestBuilder, string value)
			=> requestBuilder.WithHeader("Accept-Encoding", value);

		public static HttpRequestBuilder WithAcceptLanguage(this HttpRequestBuilder requestBuilder, string value)
			=> requestBuilder.WithHeader("Accept-Language", value);

		public static HttpRequestBuilderWithBody WithBody(this HttpRequestBuilder requestBuilder, HttpContent content)
			=> new HttpRequestBuilderWithBody(requestBuilder.Client, requestBuilder.Method, requestBuilder.Address, requestBuilder.Headers, content);

		public static HttpRequestBuilderWithBody WithStringBody(this HttpRequestBuilder requestBuilder, string content, string contentType)
			=> new HttpRequestBuilderWithBody
			(
				requestBuilder.Client, 
				requestBuilder.Method, 
				requestBuilder.Address, 
				requestBuilder.Headers.Append(new HttpHeader("content-type", contentType)), 
				new StringContent(content)
			);

		public static HttpRequestBuilderWithBody WithJsonBody(this HttpRequestBuilder requestBuilder, string json)
			=> requestBuilder.WithStringBody(json, "application/json");

		public static HttpRequestBuilderWithoutBody WithoutBody(this HttpRequestBuilder requestBuilder)
			=> new HttpRequestBuilderWithoutBody(requestBuilder.Client, requestBuilder.Method, requestBuilder.Address, requestBuilder.Headers);
	}
}
