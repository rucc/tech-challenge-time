using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pentoTrack
{
	public class Config
	{
		public string TrackersJsonFileLocation = "./trackers.json";
		public string UsersJsonFileLocation = "./users.json";
		public string ContentRoot { get; set; }
	}
}
