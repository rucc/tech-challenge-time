using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pentoTrack.Models;
using pentoTrack.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pentoTrack.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TrackerController : ControllerBase
	{
		readonly ITrackerRepository m_trackerRepo;
		readonly IUserRepository m_userRepo;

		public TrackerController(ITrackerRepository trackerRepo, IUserRepository userRepo)
		{
			m_trackerRepo = trackerRepo;
			m_userRepo = userRepo;
		}

		User CurrentUser { get { return m_userRepo.GetCurrentUser(Request); } }

		/// <summary>
		/// Gets the currently active tracking session
		/// </summary>
		/// <returns>The active tracker or null if there is none</returns>
		[HttpGet("active")]
		public ActionResult Get()
		{
			var activeTracker = m_trackerRepo.GetActiveTrackerOfUser(CurrentUser.Id);
			return Ok(activeTracker);
		}

		/// <summary>
		/// Start time tracking
		/// </summary>
		[HttpPost]
		public ActionResult Post([FromBody] StartTrackingRequest csr)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var activeTracker = m_trackerRepo.GetActiveTrackerOfUser(CurrentUser.Id);
			if (activeTracker!= null)
			{
				return BadRequest("Time tracking already active");
			}

			var startedTracker = m_trackerRepo.StartTracker(CurrentUser.Id, csr.TrackerName);
			return Ok(startedTracker);
		}

		/// <summary>
		/// Stops the tracker
		/// </summary>
		/// <param name="trackerId">The id of the tracker to stop</param>
		/// <returns>The patched tracker</returns>
		[HttpPatch("{trackerId}")]
		public ActionResult Patch(string trackerId)
		{
			if (trackerId == null)
			{
				return BadRequest("missing tracker id");
			}
			var tracker = m_trackerRepo.GetTracker(trackerId);
			if (tracker == null || tracker.UserId != CurrentUser.Id)
			{
				return BadRequest($"invalid tracker id {trackerId}");
			}
			if (!tracker.IsRunning)
			{
				return BadRequest($"Tracking is not active for {trackerId}");
			}
			tracker = m_trackerRepo.StopTracker(trackerId);
			return Ok(tracker);
		}

	}
}
