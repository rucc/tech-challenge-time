using pentoTrack.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.CommonMocks
{
	public class StaticClock : IClock
	{
		public DateTime Now { get; set; }
	}
}
