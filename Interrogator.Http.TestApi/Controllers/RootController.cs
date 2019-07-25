using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Interrogator.Http.TestApi.Controllers
{
	[Route("")]
	[ApiController]
	public class RootController : ControllerBase
	{
		[HttpGet("GetOkWithNoBody")]
		public ActionResult<string> GetOkWithNoBody()
			=> Ok();

		[HttpGet("GetUnauthorizedWithNoBody")]
		public ActionResult GetUnauthorizedWithNoBody()
			=> Unauthorized();

		[HttpGet("GetForbiddenWithNoBody")]
		public ActionResult GetForbiddenWithNoBody()
			=> Forbid();

		[HttpPost("PostStringArrayGetOkWithNoBody")]
		public ActionResult PostStringArrayGetOkWithNoBody([FromBody]string[] value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return Ok();
		}

		[HttpPost("PostStringArrayGetOkWithStringArray")]
		public ActionResult<string[]> PostStringArrayGetOkWithStringArray([FromBody]string[] value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return value;
		}
	}
}
