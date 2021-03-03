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

		public TrackingSummary(IEnumerable<Tracker> trackers, string name)
		{
			Name = name;
			Trackers = trackers.Where(t => t.StoppedAt.HasValue).OrderByDescending(t => t.StartedAt).ToList();
			TotalSeconds = Convert.ToInt32(Trackers
				.Select(t => (t.StoppedAt.Value - t.StartedAt).TotalSeconds)
				.Sum());
		}
	}
}
