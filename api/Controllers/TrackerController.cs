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
		readonly IClock m_clock;

		public TrackerController(ITrackerRepository trackerRepo, IUserRepository userRepo, IClock clock)
		{
			m_trackerRepo = trackerRepo;
			m_userRepo = userRepo;
			m_clock = clock;
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
		/// Gets the tracking summary for the given interval
		/// </summary>
		/// <param name="interval">Possible values: day | week | month</param>
		/// <returns></returns>
		[HttpGet("summary/{interval}")]
		public ActionResult GetSummary(string interval)
		{
			if (string.IsNullOrEmpty(interval))
			{
				ModelState.AddModelError("interval", "this field is required");
			}
			else if (!(new[] { "day", "week", "month" }.Contains(interval))) {
				ModelState.AddModelError("interval", $"invalid interval: {interval} valid values are: day, week and month");
			}
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var now = m_clock.Now;
			var begin = interval switch
			{
				"day" => now.Date,
				"week" => now.Date.AddDays(-1 * ((((int) now.DayOfWeek) + 7) % 8)),
				"month" => new DateTime(now.Year, now.Month, 1),
				_ => throw new ArgumentOutOfRangeException($"unexpected interval: {interval}")
			};
			
			var trackers = m_trackerRepo.GetTrackerHistoryDuring(CurrentUser.Id, begin, now);
			return Ok(new TrackingSummary(trackers, interval));
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
