using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;

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
