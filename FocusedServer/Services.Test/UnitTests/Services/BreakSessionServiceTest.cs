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
        private Mock<IBreakSessionRepository> BreakSessionRepository { get; set; }
        private BreakSessionService SubjectUnderTest { get; set; }

        [SetUp]
        public void Setup()
        {
            BreakSessionRepository = new Mock<IBreakSessionRepository>();
            SubjectUnderTest = new BreakSessionService(BreakSessionRepository.Object);
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
            BreakSessionRepository.Setup(_ => _.GetBreakSessionByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(sessions);

            var result = await SubjectUnderTest.GetBreakDurationByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(2, result);
        }
    }
}
