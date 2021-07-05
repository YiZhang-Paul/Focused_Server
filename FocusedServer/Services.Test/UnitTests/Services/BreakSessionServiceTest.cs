using Core.Interfaces.Repositories;
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
    public class BreakSessionServiceTest
    {
        private Mock<IFocusSessionRepository> FocusSessionRepository { get; set; }
        private Mock<IBreakSessionRepository> BreakSessionRepository { get; set; }
        private BreakSessionService SubjectUnderTest { get; set; }

        [SetUp]
        public void Setup()
        {
            FocusSessionRepository = new Mock<IFocusSessionRepository>();
            BreakSessionRepository = new Mock<IBreakSessionRepository>();
            SubjectUnderTest = new BreakSessionService(FocusSessionRepository.Object, BreakSessionRepository.Object);
        }

        [Test]
        public async Task GetOpenBreakSessionShouldReturnUnfinishedBreakSession()
        {
            var session = new BreakSession { Id = ObjectId.GenerateNewId().ToString() };
            BreakSessionRepository.Setup(_ => _.GetUnfinishedBreakSession(It.IsAny<string>())).ReturnsAsync(session);
            BreakSessionRepository.Setup(_ => _.GetStaleBreakSession(It.IsAny<string>())).ReturnsAsync((BreakSession)null);

            var result = await SubjectUnderTest.GetOpenBreakSession("user_id", false).ConfigureAwait(false);

            Assert.AreEqual(session.Id, result.Id);
        }

        [Test]
        public async Task GetOpenBreakSessionShouldReturnStaleBreakSession()
        {
            var session = new BreakSession { Id = ObjectId.GenerateNewId().ToString() };
            BreakSessionRepository.Setup(_ => _.GetStaleBreakSession(It.IsAny<string>())).ReturnsAsync((BreakSession)null);
            BreakSessionRepository.Setup(_ => _.GetStaleBreakSession(It.IsAny<string>())).ReturnsAsync(session);

            var result = await SubjectUnderTest.GetOpenBreakSession("user_id", true).ConfigureAwait(false);

            Assert.AreEqual(session.Id, result.Id);
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

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenAssociatedFocusSessionIdDoesNotExist()
        {
            var option = new BreakSessionStartupOption { FocusSessionId = null };

            Assert.IsFalse(await SubjectUnderTest.StartBreakSession("user_id", option).ConfigureAwait(false));
        }

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenAssociatedFocusSessionDoesNotExist()
        {
            var option = new BreakSessionStartupOption { FocusSessionId = ObjectId.GenerateNewId().ToString() };
            FocusSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((FocusSession)null);

            var result = await SubjectUnderTest.StartBreakSession("user_id", option).ConfigureAwait(false);

            Assert.IsFalse(result);
            FocusSessionRepository.Verify(_ => _.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenAssociatedFocusSessionIsNotEndedYet()
        {
            var option = new BreakSessionStartupOption { FocusSessionId = ObjectId.GenerateNewId().ToString() };
            var session = new FocusSession { EndTime = null };
            FocusSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);

            var result = await SubjectUnderTest.StartBreakSession("user_id", option).ConfigureAwait(false);

            Assert.IsFalse(result);
            FocusSessionRepository.Verify(_ => _.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenUnfinishedBreakSessionExist()
        {
            var option = new BreakSessionStartupOption { FocusSessionId = ObjectId.GenerateNewId().ToString() };
            var session = new FocusSession { EndTime = new DateTime(2021, 1, 1) };
            FocusSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);
            BreakSessionRepository.Setup(_ => _.GetUnfinishedBreakSession(It.IsAny<string>())).ReturnsAsync(new BreakSession());

            var result = await SubjectUnderTest.StartBreakSession("user_id", option).ConfigureAwait(false);

            Assert.IsFalse(result);
            BreakSessionRepository.Verify(_ => _.Add(It.IsAny<BreakSession>()), Times.Never);
        }

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenFailedToCreateBreakSession()
        {
            var option = new BreakSessionStartupOption { FocusSessionId = ObjectId.GenerateNewId().ToString() };
            var session = new FocusSession { EndTime = new DateTime(2021, 1, 1) };
            FocusSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);
            BreakSessionRepository.Setup(_ => _.GetUnfinishedBreakSession(It.IsAny<string>())).ReturnsAsync((BreakSession)null);
            BreakSessionRepository.Setup(_ => _.Add(It.IsAny<BreakSession>())).ReturnsAsync(string.Empty);

            var result = await SubjectUnderTest.StartBreakSession("user_id", option).ConfigureAwait(false);

            Assert.IsFalse(result);
            BreakSessionRepository.Verify(_ => _.Add(It.IsAny<BreakSession>()), Times.Once);
        }

        [Test]
        public async Task StartBreakSessionShouldReturnTrueWhenSuccessfullyCreatedBreakSession()
        {
            var option = new BreakSessionStartupOption { FocusSessionId = ObjectId.GenerateNewId().ToString(), TotalMinutes = 15 };
            var session = new FocusSession { EndTime = new DateTime(2021, 1, 1) };
            FocusSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);
            BreakSessionRepository.Setup(_ => _.GetUnfinishedBreakSession(It.IsAny<string>())).ReturnsAsync((BreakSession)null);
            BreakSessionRepository.Setup(_ => _.Add(It.IsAny<BreakSession>())).ReturnsAsync(ObjectId.GenerateNewId().ToString());

            var result = await SubjectUnderTest.StartBreakSession("user_id", option).ConfigureAwait(false);

            Assert.IsTrue(result);

            BreakSessionRepository.Verify(_ => _.Add(It.Is<BreakSession>
            (
                session => session.UserId == "user_id" &&
                           (DateTime.Now - session.StartTime).TotalSeconds < 3 &&
                           session.FocusSessionId == option.FocusSessionId &&
                           session.TargetDuration == 0.25
            )), Times.Once);
        }

        [Test]
        public async Task StopBreakSessionShouldReturnFalseWhenBreakSessionDoesNotExist()
        {
            BreakSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((BreakSession)null);

            Assert.IsFalse(await SubjectUnderTest.StopBreakSession("user_id", "session_id").ConfigureAwait(false));
        }

        [Test]
        public async Task StopBreakSessionShouldReturnFalseWhenFailedToUpdateBreakSession()
        {
            var session = new BreakSession { Id = ObjectId.GenerateNewId().ToString() };
            BreakSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);
            BreakSessionRepository.Setup(_ => _.Replace(It.IsAny<BreakSession>())).ReturnsAsync((BreakSession)null);

            var result = await SubjectUnderTest.StopBreakSession("user_id", "session_id").ConfigureAwait(false);

            Assert.IsFalse(result);
            BreakSessionRepository.Verify(_ => _.Replace(It.IsAny<BreakSession>()), Times.Once);
        }

        [Test]
        public async Task StopBreakSessionShouldReturnTrueWhenBreakSessionUpdatedSuccessfully()
        {
            var session = new BreakSession
            {
                Id = ObjectId.GenerateNewId().ToString(),
                StartTime = DateTime.Now.AddHours(-1),
                TargetDuration = 2
            };

            BreakSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);
            BreakSessionRepository.Setup(_ => _.Replace(It.IsAny<BreakSession>())).ReturnsAsync(session);

            var result = await SubjectUnderTest.StopBreakSession("user_id", "session_id").ConfigureAwait(false);

            Assert.IsTrue(result);
            BreakSessionRepository.Verify(_ => _.Replace(It.Is<BreakSession>(session => (DateTime.Now - session.EndTime.Value).TotalSeconds < 3)), Times.Once);
        }

        [Test]
        public async Task StopBreakSessionShouldUseTargetEndTimeForStaleBreakSession()
        {
            var session = new BreakSession
            {
                Id = ObjectId.GenerateNewId().ToString(),
                StartTime = DateTime.Now.AddHours(-3),
                TargetDuration = 2
            };

            BreakSessionRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(session);
            BreakSessionRepository.Setup(_ => _.Replace(It.IsAny<BreakSession>())).ReturnsAsync(session);

            var result = await SubjectUnderTest.StopBreakSession("user_id", "session_id").ConfigureAwait(false);

            Assert.IsTrue(result);
            BreakSessionRepository.Verify(_ => _.Replace(It.Is<BreakSession>(session => (DateTime.Now.AddHours(-1) - session.EndTime.Value).TotalSeconds < 3)), Times.Once);
        }
    }
}
