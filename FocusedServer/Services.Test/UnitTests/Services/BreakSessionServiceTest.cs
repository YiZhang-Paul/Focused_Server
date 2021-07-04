using Core.Interfaces.Repositories;
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
    public class BreakSessionServiceTest
    {
        private Mock<IBreakSessionRepository> _breakSessionRepository;
        private BreakSessionService _service;

        [SetUp]
        public void Setup()
        {
            _breakSessionRepository = new Mock<IBreakSessionRepository>();
            _service = new BreakSessionService(_breakSessionRepository.Object);
        }

        [Test]
        public async Task GetBreakDurationByDateRangeShouldReturnTotalBreakDuration()
        {
            var sessions = new List<BreakSession>
            {
                new BreakSession
                {
                    StartTime = new DateTime(2021, 1, 2, 5, 0, 0),
                    EndTime = new DateTime(2021, 1, 2, 7, 0, 0)
                }
            };

            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 3);
            _breakSessionRepository.Setup(_ => _.GetBreakSessionByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(sessions);

            var result = await _service.GetBreakDurationByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(2, result);
        }
    }
}
