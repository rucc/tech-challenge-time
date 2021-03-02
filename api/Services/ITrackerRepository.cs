using Newtonsoft.Json;
using pentoTrack.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace pentoTrack.Services
{
	public interface ITrackerRepository
	{
		Tracker StartTracker(string userId, string trackerName);
		Tracker StopTracker(string trackerId);

		IEnumerable<Tracker> GetTrackerHistoryDuring(string userId, DateTime from, DateTime to);

		Tracker GetActiveTrackerOfUser(string userId);

		Tracker GetTracker(string id);

		/// <summary>
		/// Stores sheets in a json file
		/// </summary>
		public class JsonFileTrackerRepo : ITrackerRepository
		{
			readonly Config m_cfg;
			readonly IClock m_clock;
			private object m_mutex = new object();
			Dictionary<string, Tracker> m_trackers = new Dictionary<string, Tracker>();

			public JsonFileTrackerRepo(Config cfg, IClock clock)
			{
				m_cfg = cfg;
				m_clock = clock;
				Load();
			}

			private void Load()
			{
				m_trackers = File.Exists(m_cfg.TrackersJsonFileLocation) ?
					JsonConvert.DeserializeObject<Dictionary<string, Tracker>>(File.ReadAllText(m_cfg.TrackersJsonFileLocation))
					:
					new Dictionary<string, Tracker>();
			}

			private void SaveTrackers()
			{
				File.WriteAllText(m_cfg.TrackersJsonFileLocation, JsonConvert.SerializeObject(m_trackers, Formatting.Indented));
			}

			public Tracker StartTracker(string userId, string trackerName)
			{
				var tracker = new Tracker
				{
					Id = Guid.NewGuid().ToString(),
					UserId = userId,
					Name = trackerName,
					StartedAt = m_clock.Now,
					StoppedAt = null
				};
				lock (m_mutex)
				{
					m_trackers[tracker.Id] = tracker;
					SaveTrackers();
				}
				return tracker;
			}

			public Tracker StopTracker(string trackerId)
			{
				lock (m_mutex)
				{
					var tracker = m_trackers[trackerId];
					if (tracker.IsRunning)
					{
						tracker.StoppedAt = m_clock.Now;
						if (tracker.StoppedAt < tracker.StartedAt)
						{
							tracker.StoppedAt = tracker.StartedAt;
						}
					}
					SaveTrackers();
					return tracker;
				}
			}

			public IEnumerable<Tracker> GetTrackerHistoryDuring(string userId, DateTime from, DateTime to)
			{
				lock (m_mutex)
				{
					return m_trackers.Values.Where(t => t.UserId == userId && t.StoppedAt > from && t.StartedAt < to).ToList();
				}
			}

			public Tracker GetActiveTrackerOfUser(string userId)
			{
				lock (m_mutex)
				{
					return m_trackers.Values.FirstOrDefault(t => t.UserId == userId && t.IsRunning);
				}
			}

			public Tracker GetTracker(string id)
			{
				lock (m_mutex)
				{
					Tracker ret;
					m_trackers.TryGetValue(id, out ret);
					return ret;
				}
			}
		}

	}
}
