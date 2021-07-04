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
        private Mock<IWorkItemRepository> _workItemRepository;
        private Mock<ITimeSeriesRepository> _timeSeriesRepository;
        private Mock<IFocusSessionRepository> _focusSessionRepository;
        private Mock<IWorkItemService> _workItemService;
        private FocusSessionService _service;

        [SetUp]
        public void Setup()
        {
            _workItemRepository = new Mock<IWorkItemRepository>();
            _timeSeriesRepository = new Mock<ITimeSeriesRepository>();
            _focusSessionRepository = new Mock<IFocusSessionRepository>();
            _workItemService = new Mock<IWorkItemService>();

            _service = new FocusSessionService
            (
                _workItemRepository.Object,
                _timeSeriesRepository.Object,
                _focusSessionRepository.Object,
                _workItemService.Object
            );
        }

        [Test]
        public async Task GetActiveFocusSessionMetaShouldReturnNullWhenNoActiveFocusSessionAvailable()
        {
            _focusSessionRepository.Setup(_ => _.GetActiveFocusSession(It.IsAny<string>())).ReturnsAsync((FocusSession)null);

            Assert.IsNull(await _service.GetActiveFocusSessionMeta("user_id").ConfigureAwait(false));
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
            _focusSessionRepository.Setup(_ => _.GetActiveFocusSession(It.IsAny<string>())).ReturnsAsync(session);
            _timeSeriesRepository.Setup(_ => _.GetDataSourceIdsByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TimeSeriesType>())).ReturnsAsync(new List<string>());
            _timeSeriesRepository.Setup(_ => _.GetTimeSeriesByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TimeSeriesType>())).ReturnsAsync(series);
            _workItemService.Setup(_ => _.GetWorkItemActivityBreakdownByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(breakdown);

            var result = await _service.GetActiveFocusSessionMeta("user_id").ConfigureAwait(false);

            Assert.AreEqual(5, result.Activities.Regular);
            Assert.AreEqual(2, result.Activities.Recurring);
            Assert.AreEqual(3, result.Activities.Interruption);
            Assert.AreEqual(2, result.Activities.Overlearning);
            _workItemRepository.Verify(_ => _.GetWorkItemMetas(It.IsAny<string>(), It.IsAny<List<string>>()), Times.Once);
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
            _timeSeriesRepository.Setup(_ => _.GetTimeSeriesByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TimeSeriesType>())).ReturnsAsync(sessions);

            var result = await _service.GetOverlearningHoursByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(2, result);
        }
    }
}
