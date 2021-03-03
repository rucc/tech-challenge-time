using NUnit.Framework;
using pentoTrack.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models
{
	public class TrackingSummaryTest
	{
		[Test]
		public void TestSum()
		{
			var now = new DateTime(2021, 3, 3, 13, 56, 30);
			var ts = new TrackingSummary(new[]
			{
				new Tracker{StartedAt = now, StoppedAt =now.AddMilliseconds(600)},
				new Tracker{StartedAt = now.AddMilliseconds(1000), StoppedAt =now.AddMilliseconds(1600)},
				new Tracker{StartedAt = now.AddMilliseconds(2000), StoppedAt =now.AddMilliseconds(2600)},
				new Tracker{StartedAt = now.AddMilliseconds(3000), StoppedAt =now.AddMilliseconds(3600)}
			}, "test summary");
			// 4 * 600ms = 2,4 secs should be rounded to 2 secs
			Assert.AreEqual(2, ts.TotalSeconds);

		}
	}
}
