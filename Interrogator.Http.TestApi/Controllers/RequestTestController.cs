using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Interrogator.Http.TestApi.Controllers
{
	[Route("")]
	[ApiController]
	public class RequestController : ControllerBase
	{
		[HttpGet("Request/GetWithNoBody")]
		public ActionResult<string> GetWithNoBody()
			=> HttpContext
				.Request
				.ContentLength
				.Should()
				.BeNull()
				.ThenOk();

		[HttpGet("Request/GetWithHeader")]
		public ActionResult<string> GetWithHeader([FromHeader(Name = Constants.TestHeaderName)]string headerValue)
			=> headerValue
				.Should()
				.Be(Constants.TestHeaderValue)
				.ThenOk();

		[HttpGet("Request/GetWithMultiHeader")]
		public ActionResult<string> GetWithMultiHeader([FromHeader(Name = Constants.TestHeaderName)]string[] headerValue)
			=> headerValue
				.Should()
				.BeEquivalentTo(Constants.TestMultiHeaderValues)
				.ThenOk();

		[HttpGet("Request/GetWithNoHeader")]
		public ActionResult<string> GetWithNoHeader([FromHeader(Name = Constants.TestHeaderName)]string headerValue)
			=> headerValue
				.Should()
				.BeNull()
				.ThenOk();

		[HttpPost("Request/PostWithBody")]
		public ActionResult<string> PostWithBody([FromBody]string[] body)
			=> body
				.Should()
				.BeEquivalentTo(Constants.TestBody)
				.ThenOk();

		[HttpPost("Request/PostWithNoBody")]
		public ActionResult<string> PostWithNoBody()
			=> HttpContext
				.Request
				.ContentLength
				.Should()
				.BeNull()
				.ThenOk();

		[HttpPost("Request/PostWithHeader")]
		public ActionResult<string> PostWithHeader([FromHeader(Name = Constants.TestHeaderName)]string headerValue)
			=> headerValue
				.Should()
				.Be(Constants.TestHeaderValue)
				.ThenOk();

		[HttpPost("Request/PostWithMultiHeader")]
		public ActionResult<string> PostWithMultiHeader([FromHeader(Name = Constants.TestHeaderName)]string[] headerValue)
			=> headerValue
				.Should()
				.BeEquivalentTo(Constants.TestMultiHeaderValues)
				.ThenOk();

		[HttpPost("Request/PostWithNoHeader")]
		public ActionResult<string> PostWithNoHeader([FromHeader(Name = Constants.TestHeaderName)]string headerValue)
			=> headerValue
				.Should()
				.BeNull()
				.ThenOk();
	}
}
