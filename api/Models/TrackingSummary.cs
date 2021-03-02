using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pentoTrack.Models
{
	public class TrackingSummary
	{
		public string Name { get; set; }
		public IEnumerable<Tracker> Trackers { get; set; }
		public int TotalSeconds { get; set; }
	}
}
