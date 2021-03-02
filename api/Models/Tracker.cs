using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pentoTrack.Models
{
	public class Tracker
	{
		public string Id { get; set; }
		public string UserId { get; set; }
		public string Name { get; set; }
		public DateTime StartedAt { get; set; }

		public DateTime? StoppedAt { get; set; }
		public bool IsRunning => StoppedAt == null; 

	}

	public class StartTrackingRequest
	{
		[Required]
		public string TrackerName { get; set; }
	}
}
