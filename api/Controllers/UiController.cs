using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pentoTrack.Controllers
{
	[Route("ui")]
	[ApiController]
	public class UiController : ControllerBase
	{
		readonly Config m_cfg;

		public UiController(Config cfg)
		{
			m_cfg = cfg;
		}

		/// <summary>
		/// Serves the user interface of the tracker app
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Get()
		{
			return Content(System.IO.File.ReadAllText(Path.Combine(m_cfg.ContentRoot, "index.html")), "text/html", Encoding.UTF8);
		}
	}
}
