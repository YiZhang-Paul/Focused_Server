using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.TimeSession;
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

            Assert.IsNull(await SubjectUnderTest.GetActiveFocusSessionMeta("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetActiveFocusSessionMetaShouldReturnActiveFocusSessionMeta()
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
