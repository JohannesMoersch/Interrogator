using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Interrogator.Http.TestApi.Controllers
{
	[Route("")]
	[ApiController]
	public class ResponseController : ControllerBase
	{
		[HttpGet("Response/OK")]
		public ActionResult<string> ReturnOk()
			=> Ok();

		[HttpGet("Response/Created")]
		public ActionResult<string> ReturnCreated()
			=> StatusCode((int)HttpStatusCode.Created);

		[HttpGet("Response/Accepted")]
		public ActionResult<string> ReturnAccepted()
			=> Accepted();

		[HttpGet("Response/BadRequest")]
		public ActionResult<string> ReturnBadRequest()
			=> BadRequest();

		[HttpGet("Response/Unauthorized")]
		public ActionResult<string> ReturnUnauthorized()
			=> Unauthorized();

		[HttpGet("Response/Forbidden")]
		public ActionResult<string> ReturnForbidden()
			=> StatusCode((int)HttpStatusCode.Forbidden);

		[HttpGet("Response/NotFound")]
		public ActionResult<string> ReturnNotFound()
			=> NotFound();

		[HttpGet("Response/Conflict")]
		public ActionResult<string> ReturnConflict()
			=> Conflict();

		[HttpGet("Response/Success")]
		public ActionResult<string> ReturnSuccess()
			=> StatusCode(299);

		[HttpGet("Response/3xx")]
		public ActionResult<string> Return3xx()
			=> StatusCode(399);

		[HttpGet("Response/4xx")]
		public ActionResult<string> Return4xx()
			=> StatusCode(499);

		[HttpGet("Response/5xx")]
		public ActionResult<string> Return5xx()
			=> StatusCode(599);

		[HttpGet("Response/Body")]
		public ActionResult<string> ReturnBody()
			=> new JsonResult(Constants.TestBody);

		[HttpGet("Response/NoBody")]
		public ActionResult<string> ReturnNoBody()
			=> Ok();

		[HttpGet("Response/Header")]
		public ActionResult<string> ReturnHeader()
		{
			HttpContext.Response.Headers.Add(Constants.TestHeaderName, Constants.TestHeaderValue);

			return Ok();
		}

		[HttpGet("Response/MultiHeader")]
		public ActionResult<string> ReturnMultiHeader()
		{
			HttpContext.Response.Headers.Add(Constants.TestHeaderName, Constants.TestMultiHeaderValues);

			return Ok();
		}

		[HttpGet("Response/NoHeader")]
		public ActionResult<string> ReturnNoHeader()
			=> Ok();
	}
}
