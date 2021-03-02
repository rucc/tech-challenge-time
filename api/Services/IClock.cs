using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pentoTrack.Services
{
	public interface IClock
	{
		DateTime Now { get; }
	}

	public class SysClock : IClock
	{
		private static DateTime m_launchedAt;
		static SysClock()
		{
			m_launchedAt = DateTime.Now;
		}

		public DateTime Now
		{
			get
			{
				return DateTime.Now;
			}
		}

		public DateTime LaunchedAt => m_launchedAt;

		public TimeSpan UpTime => Now - LaunchedAt;
	}
}
