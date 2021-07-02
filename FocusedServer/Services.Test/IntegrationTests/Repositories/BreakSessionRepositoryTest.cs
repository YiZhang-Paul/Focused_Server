using Core.Models.TimeSession;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.AppStart;

namespace Services.Test.IntegrationTests.Repositories
{
    [TestFixture]
    public class BreakSessionRepositoryTest
    {
        private BreakSessionRepository _repository;

        [OneTimeSetUp]
        public void Setup()
        {
            CustomBsonSerializers.Register();
            _repository = new BreakSessionRepository(ConfigurationUtility.GetDatabaseConfiguration());
        }

        [Test]
        public async Task GetActiveBreakSessionShouldReturnNullWhenNoActiveBreakSessionExist()
        {
            var sessions = new List<BreakSession>
            {
                new BreakSession { UserId = "user_id", StartTime = DateTime.UtcNow.AddHours(-2), EndTime = DateTime.UtcNow.AddHours(-1) }
            };

            await _repository.Add(sessions).ConfigureAwait(false);

            Assert.IsNull(await _repository.GetActiveBreakSession("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetActiveBreakSessionShouldReturnNullWhenActiveBreakSessionAlreadyEnded()
        {
            var sessions = new List<BreakSession>
            {
                new BreakSession { UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), TargetDuration = 1.5 }
            };

            await _repository.Add(sessions).ConfigureAwait(false);

            Assert.IsNull(await _repository.GetActiveBreakSession("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetActiveBreakSessionShouldReturnActiveBreakSessionFound()
        {
            var sessions = new List<BreakSession>
            {
                new BreakSession { UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), TargetDuration = 2.5 }
            };

            await _repository.Add(sessions).ConfigureAwait(false);

            var result = await _repository.GetActiveBreakSession("user_id").ConfigureAwait(false);

            Assert.AreEqual(2.5, result.TargetDuration);
            Assert.IsTrue(Math.Abs((sessions[0].StartTime - result.StartTime).TotalMinutes) < 1);
            Assert.IsNull(result.EndTime);
        }

        [Test]
        public async Task GetBreakSessionByDateRangeShouldReturnEmptyCollectionWhenNoBreakSessionFound()
        {
            var sessions = new List<BreakSession>
            {
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 10, 0), EndTime = new DateTime(2021, 1, 1, 15, 30, 0) },
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 35, 0), EndTime = new DateTime(2021, 1, 1, 15, 50, 0) },
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 55, 0), EndTime = new DateTime(2021, 1, 1, 16, 15, 0) }
            };

            await _repository.Add(sessions).ConfigureAwait(false);

            var result = await _repository.GetBreakSessionByDateRange("user_id", new DateTime(2021, 1, 2), new DateTime(2021, 1, 5)).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetBreakSessionByDateRangeShouldReturnBreakSessionsWithinTimeRange()
        {
            var sessions = new List<BreakSession>
            {
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 10, 0), EndTime = new DateTime(2021, 1, 1, 15, 30, 0) },
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 35, 0), EndTime = new DateTime(2021, 1, 1, 15, 50, 0) },
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 55, 0), EndTime = new DateTime(2021, 1, 1, 16, 15, 0) }
            };

            await _repository.Add(sessions).ConfigureAwait(false);

            var result = await _repository.GetBreakSessionByDateRange("user_id", new DateTime(2021, 1, 1, 15, 30, 1), new DateTime(2021, 1, 1, 15, 54, 59)).ConfigureAwait(false);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sessions[1].StartTime, result[0].StartTime);
            Assert.AreEqual(sessions[1].EndTime, result[0].EndTime);
        }

        [Test]
        public async Task GetBreakSessionByDateRangeShouldReturnOverlappingBreakSessions()
        {
            var sessions = new List<BreakSession>
            {
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 14, 45, 0), EndTime = new DateTime(2021, 1, 1, 15, 05, 0) },
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 10, 0), EndTime = new DateTime(2021, 1, 1, 15, 30, 0) },
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 35, 0), EndTime = new DateTime(2021, 1, 1, 15, 50, 0) },
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 55, 0), EndTime = new DateTime(2021, 1, 1, 16, 15, 0) },
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 16, 20, 0), EndTime = new DateTime(2021, 1, 1, 16, 40, 0) }
            };

            await _repository.Add(sessions).ConfigureAwait(false);

            var result = await _repository.GetBreakSessionByDateRange("user_id", new DateTime(2021, 1, 1, 15, 20, 0), new DateTime(2021, 1, 1, 16, 05, 0)).ConfigureAwait(false);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(sessions[1].StartTime, result[0].StartTime);
            Assert.AreEqual(sessions[1].EndTime, result[0].EndTime);
            Assert.AreEqual(sessions[2].StartTime, result[1].StartTime);
            Assert.AreEqual(sessions[2].EndTime, result[1].EndTime);
            Assert.AreEqual(sessions[3].StartTime, result[2].StartTime);
            Assert.AreEqual(sessions[3].EndTime, result[2].EndTime);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _repository.DropCollection().ConfigureAwait(false);
        }
    }
}
