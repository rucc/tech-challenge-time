using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using pentoTrack.Controllers;
using pentoTrack.Models;
using pentoTrack.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.CommonMocks;

namespace Test.Controllers
{
	public class TrackerControllerTestSetup
	{
		public StaticClock ClockMock { get; set; }
		public User UserMock { get; set; }
		public IUserRepository UserRepoMock { get; set; }
		public Mock<ITrackerRepository> TrackerRepoMock { get; set; }

		public TrackerController Subject { get; }

		public TrackerControllerTestSetup(List<Tracker> trackers)
		{
			ClockMock = new StaticClock { Now = new DateTime(2021, 3, 3, 10, 13, 20) };
			UserMock = new User { Id = "usr1", Name = "test" };
			UserRepoMock = Mock.Of<IUserRepository>(ur => ur.GetCurrentUser(It.IsAny<HttpRequest>()) == UserMock);
			TrackerRepoMock = new Mock<ITrackerRepository>();
			TrackerRepoMock.Setup(tr => tr.GetTrackerHistoryDuring(UserMock.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Returns(trackers);
			Subject = new TrackerController(TrackerRepoMock.Object, UserRepoMock, ClockMock);
		}
	}

	public class TrackerControllerTest
	{


		[Test]
		public void TestGetSummaryInterValCalculations()
		{
			var setup = new TrackerControllerTestSetup(new List<Tracker>());
			var summary = setup.Subject.GetSummary("week");
			setup.TrackerRepoMock.Verify(tr => 
				tr.GetTrackerHistoryDuring(setup.UserMock.Id, It.Is<DateTime>(begin => begin == new DateTime(2021, 3, 1)), setup.ClockMock.Now), 
				Times.Exactly(1)
			);

			summary = setup.Subject.GetSummary("month");
			setup.TrackerRepoMock.Verify(tr =>
				tr.GetTrackerHistoryDuring(setup.UserMock.Id, It.Is<DateTime>(begin => begin == new DateTime(2021, 3, 1)), setup.ClockMock.Now),
				Times.Exactly(2)
			);

			summary = setup.Subject.GetSummary("day");
			setup.TrackerRepoMock.Verify(tr =>
				tr.GetTrackerHistoryDuring(setup.UserMock.Id, It.Is<DateTime>(begin => begin == new DateTime(2021, 3, 3)), setup.ClockMock.Now),
				Times.Exactly(1)
			);
		}

		[Test]
		public void TestPost()
		{
			var setup = new TrackerControllerTestSetup(new List<Tracker>());
			setup.TrackerRepoMock.Setup(tr => tr.StartTracker(setup.UserMock.Id, "unit testing")).Returns(new Tracker { Id = "tid" });

			var response = setup.Subject.Post(new StartTrackingRequest { TrackerName = "unit testing" });
			Assert.IsInstanceOf<OkObjectResult>(response);
			Assert.IsInstanceOf<Tracker>(((OkObjectResult)response).Value);


			// test that we don't allow to start tracking when there is an active session
			setup = new TrackerControllerTestSetup(new List<Tracker>());
			setup.TrackerRepoMock.Setup(tr => tr.StartTracker(setup.UserMock.Id, "unit testing")).Returns(new Tracker { Id = "tid2" });
			setup.TrackerRepoMock.Setup(tr => tr.GetActiveTrackerOfUser(setup.UserMock.Id)).Returns(new Tracker { Id = "tid1" });
			response = setup.Subject.Post(new StartTrackingRequest { TrackerName = "unit testing" });
			Assert.IsInstanceOf<BadRequestObjectResult>(response);
		}

		[Test]
		public void TestPatch()
		{
			// test that we don't allow to stop an already stopped tracker
			var setup = new TrackerControllerTestSetup(new List<Tracker>());
			setup.TrackerRepoMock.Setup(tr => tr.GetTracker("tid"))
				.Returns(new Tracker { 
					Id = "tid", 
					StartedAt = setup.ClockMock.Now.AddDays(-1), 
					StoppedAt = setup.ClockMock.Now, 
					Name = "old", 
					UserId = setup.UserMock.Id 
				});

			var response = setup.Subject.Patch("tid");
			Assert.IsInstanceOf<BadRequestObjectResult>(response);
		}
	}
}
