using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.TimeSession;
using MongoDB.Bson;
using Moq;
using NUnit.Framework;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Test.UnitTests.Services
{
    [TestFixture]
    public class FocusSessionServiceTest
    {
        private Mock<IWorkItemRepository> WorkItemRepository { get; set; }
        private Mock<ITimeSeriesRepository> TimeSeriesRepository { get; set; }
        private Mock<IFocusSessionRepository> FocusSessionRepository { get; set; }
        private Mock<IWorkItemService> WorkItemService { get; set; }
        private FocusSessionService SubjectUnderTest { get; set; }

        [SetUp]
        public void Setup()
        {
            WorkItemRepository = new Mock<IWorkItemRepository>();
            TimeSeriesRepository = new Mock<ITimeSeriesRepository>();
            FocusSessionRepository = new Mock<IFocusSessionRepository>();
            WorkItemService = new Mock<IWorkItemService>();

            SubjectUnderTest = new FocusSessionService
            (
                WorkItemRepository.Object,
                TimeSeriesRepository.Object,
                FocusSessionRepository.Object,
                WorkItemService.Object
            );
        }

        [Test]
        public async Task GetActiveFocusSessionMetaShouldReturnNullWhenNoActiveFocusSessionAvailable()
        {
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync((FocusSession)null);
            FocusSessionRepository.Setup(_ => _.GetStaleFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());

            Assert.IsNull(await SubjectUnderTest.GetActiveFocusSessionMeta("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetActiveFocusSessionMetaShouldReturnActiveFocusSessionMetaFound()
        {
            var series = new List<TimeSeries>
            {
                new TimeSeries
                {
                    StartTime = new DateTime(2021, 1, 2, 5, 0, 0),
                    EndTime = new DateTime(2021, 1, 2, 7, 0, 0)
                }
            };

            var session = new FocusSession { StartTime = new DateTime(2021, 1, 2, 5, 0, 0) };
            var breakdown = new ActivityBreakdownDto { Regular = 5, Recurring = 2, Interruption = 3 };
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync(session);
            TimeSeriesRepository.Setup(_ => _.GetDataSourceIdsByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TimeSeriesType>())).ReturnsAsync(new List<string>());
            TimeSeriesRepository.Setup(_ => _.GetTimeSeriesByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TimeSeriesType>())).ReturnsAsync(series);
            WorkItemService.Setup(_ => _.GetWorkItemActivityBreakdownByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(breakdown);

            var result = await SubjectUnderTest.GetActiveFocusSessionMeta("user_id").ConfigureAwait(false);

            Assert.AreEqual(5, result.Activities.Regular);
            Assert.AreEqual(2, result.Activities.Recurring);
            Assert.AreEqual(3, result.Activities.Interruption);
            Assert.AreEqual(2, result.Activities.Overlearning);
            WorkItemRepository.Verify(_ => _.GetWorkItemMetas(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.Once);
        }

        [Test]
        public async Task GetStaleFocusSessionMetaShouldReturnNullWhenNoStaleFocusSessionAvailable()
        {
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());
            FocusSessionRepository.Setup(_ => _.GetStaleFocusSession(It.IsAny<string>())).ReturnsAsync((FocusSession)null);

            Assert.IsNull(await SubjectUnderTest.GetStaleFocusSessionMeta("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetStaleFocusSessionMetaShouldReturnStaleFocusSessionMetaFound()
        {
            var series = new List<TimeSeries>
            {
                new TimeSeries
                {
                    StartTime = new DateTime(2021, 1, 2, 5, 0, 0),
                    EndTime = new DateTime(2021, 1, 2, 7, 0, 0)
                }
            };

            var session = new FocusSession { StartTime = new DateTime(2021, 1, 2, 5, 0, 0), TargetDuration = 1 };
            var breakdown = new ActivityBreakdownDto { Regular = 5, Recurring = 2, Interruption = 3 };
            FocusSessionRepository.Setup(_ => _.GetStaleFocusSession(It.IsAny<string>())).ReturnsAsync(session);
            TimeSeriesRepository.Setup(_ => _.GetDataSourceIdsByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TimeSeriesType>())).ReturnsAsync(new List<string>());
            TimeSeriesRepository.Setup(_ => _.GetTimeSeriesByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TimeSeriesType>())).ReturnsAsync(series);
            WorkItemService.Setup(_ => _.GetWorkItemActivityBreakdownByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(breakdown);

            var result = await SubjectUnderTest.GetStaleFocusSessionMeta("user_id").ConfigureAwait(false);

            Assert.AreEqual(5, result.Activities.Regular);
            Assert.AreEqual(2, result.Activities.Recurring);
            Assert.AreEqual(3, result.Activities.Interruption);
            Assert.AreEqual(1, result.Activities.Overlearning);
            WorkItemRepository.Verify(_ => _.GetWorkItemMetas(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<DateTime?>(), session.TargetEndTime), Times.Once);
        }

        [Test]
        public async Task StartFocusSessionShouldReturnFalseWhenStartingWorkItemIsNotSpecified()
        {
            var option = new FocusSessionStartupOption { WorkItemId = null };

            Assert.IsFalse(await SubjectUnderTest.StartFocusSession("user_id", option).ConfigureAwait(false));
        }

        [Test]
        public async Task StartFocusSessionShouldReturnFalseWhenUnfinishedFocusSessionFound()
        {
            var option = new FocusSessionStartupOption { WorkItemId = ObjectId.GenerateNewId().ToString() };
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());

            Assert.IsFalse(await SubjectUnderTest.StartFocusSession("user_id", option).ConfigureAwait(false));
        }

        [Test]
        public async Task StartFocusSessionShouldReturnFalseWhenFailedToCreateFocusSession()
        {
            var option = new FocusSessionStartupOption { WorkItemId = ObjectId.GenerateNewId().ToString() };
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync((FocusSession)null);
            FocusSessionRepository.Setup(_ => _.Add(It.IsAny<FocusSession>())).ReturnsAsync(string.Empty);

            var result = await SubjectUnderTest.StartFocusSession("user_id", option).ConfigureAwait(false);

            Assert.IsFalse(result);
            FocusSessionRepository.Verify(_ => _.Add(It.IsAny<FocusSession>()), Times.Once);
        }

        [Test]
        public async Task StartFocusSessionShouldReturnFalseWhenFailedToStartWorkItem()
        {
            var option = new FocusSessionStartupOption { WorkItemId = ObjectId.GenerateNewId().ToString() };
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync((FocusSession)null);
            FocusSessionRepository.Setup(_ => _.Add(It.IsAny<FocusSession>())).ReturnsAsync(ObjectId.GenerateNewId().ToString());
            WorkItemService.Setup(_ => _.StartWorkItem(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var result = await SubjectUnderTest.StartFocusSession("user_id", option).ConfigureAwait(false);

            Assert.IsFalse(result);
            WorkItemService.Verify(_ => _.StartWorkItem(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task StartFocusSessionShouldReturnTrueWhenSuccessfullyCreatedFocusSession()
        {
            var option = new FocusSessionStartupOption { WorkItemId = ObjectId.GenerateNewId().ToString(), TotalMinutes = 30 };
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync((FocusSession)null);
            FocusSessionRepository.Setup(_ => _.Add(It.IsAny<FocusSession>())).ReturnsAsync(ObjectId.GenerateNewId().ToString());
            WorkItemService.Setup(_ => _.StartWorkItem(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var result = await SubjectUnderTest.StartFocusSession("user_id", option).ConfigureAwait(false);

            Assert.IsTrue(result);

            FocusSessionRepository.Verify(_ => _.Add(It.Is<FocusSession>
            (
                session => session.UserId == "user_id" &&
                           (DateTime.Now - session.StartTime).TotalSeconds < 3 &&
                           session.TargetDuration == 0.5
            )), Times.Once);
        }

        [Test]
        public async Task StopFocusSessionShouldReturnFalseWhenFocusSessionDoesNotExist()
        {
            FocusSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((FocusSession)null);

            var result = await SubjectUnderTest.StopFocusSession("user_id", "session_id").ConfigureAwait(false);

            Assert.IsFalse(result);
            FocusSessionRepository.Verify(_ => _.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task StopFocusSessionShouldReturnFalseWhenFailedToUpdateFocusSession()
        {
            var session = new FocusSession { StartTime = new DateTime(2021, 1, 1) };
            FocusSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);
            FocusSessionRepository.Setup(_ => _.Replace(It.IsAny<FocusSession>())).ReturnsAsync((FocusSession)null);

            var result = await SubjectUnderTest.StopFocusSession("user_id", "session_id").ConfigureAwait(false);

            Assert.IsFalse(result);
            FocusSessionRepository.Verify(_ => _.Replace(It.IsAny<FocusSession>()), Times.Once);
        }

        [Test]
        public async Task StopFocusSessionShouldReturnFalseWhenFailedToStopWorkItem()
        {
            var session = new FocusSession { StartTime = new DateTime(2021, 1, 1) };
            FocusSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);
            FocusSessionRepository.Setup(_ => _.Replace(It.IsAny<FocusSession>())).ReturnsAsync(session);
            WorkItemService.Setup(_ => _.StopWorkItem(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(false);

            var result = await SubjectUnderTest.StopFocusSession("user_id", "session_id").ConfigureAwait(false);

            Assert.IsFalse(result);
            WorkItemService.Verify(_ => _.StopWorkItem(It.IsAny<string>(), It.IsAny<WorkItemStatus>()), Times.Once);
        }

        [Test]
        public async Task StopFocusSessionShouldReturnTrueWhenSuccessfullyStoppedFocusSession()
        {
            var session = new FocusSession { StartTime = DateTime.Now.AddHours(-1), TargetDuration = 2 };
            FocusSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);
            FocusSessionRepository.Setup(_ => _.Replace(It.IsAny<FocusSession>())).ReturnsAsync(session);
            WorkItemService.Setup(_ => _.StopWorkItem(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(true);
            TimeSeriesRepository.Setup(_ => _.GetOpenTimeRange(It.IsAny<string>())).ReturnsAsync(new List<TimeSeries>());

            var result = await SubjectUnderTest.StopFocusSession("user_id", "session_id").ConfigureAwait(false);

            Assert.IsTrue(result);
            WorkItemService.Verify(_ => _.StopWorkItem(It.IsAny<string>(), It.IsAny<WorkItemStatus>()), Times.Once);
            FocusSessionRepository.Verify(_ => _.Replace(It.Is<FocusSession>(session => (DateTime.Now - session.EndTime.Value).TotalSeconds < 3)), Times.Once);
        }

        [Test]
        public async Task StopFocusSessionShouldUseTargetEndTimeForStaleFocusSession()
        {
            var session = new FocusSession { StartTime = DateTime.Now.AddHours(-2), TargetDuration = 1 };
            FocusSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);
            FocusSessionRepository.Setup(_ => _.Replace(It.IsAny<FocusSession>())).ReturnsAsync(session);
            WorkItemService.Setup(_ => _.StopWorkItem(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(true);
            TimeSeriesRepository.Setup(_ => _.GetOpenTimeRange(It.IsAny<string>())).ReturnsAsync(new List<TimeSeries>());

            var result = await SubjectUnderTest.StopFocusSession("user_id", "session_id").ConfigureAwait(false);

            Assert.IsTrue(result);
            FocusSessionRepository.Verify(_ => _.Replace(It.Is<FocusSession>(session => (DateTime.Now.AddHours(-1) - session.EndTime.Value).TotalSeconds < 3)), Times.Once);
        }

        [Test]
        public async Task GetOverlearningHoursByDateRangeShouldReturnTotalOverlearningHours()
        {
            var sessions = new List<TimeSeries>
            {
                new TimeSeries
                {
                    StartTime = new DateTime(2021, 1, 2, 5, 0, 0),
                    EndTime = new DateTime(2021, 1, 2, 7, 0, 0)
                }
            };

            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 3);
            TimeSeriesRepository.Setup(_ => _.GetTimeSeriesByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TimeSeriesType>())).ReturnsAsync(sessions);

            var result = await SubjectUnderTest.GetOverlearningHoursByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(2, result);
        }
    }
}
