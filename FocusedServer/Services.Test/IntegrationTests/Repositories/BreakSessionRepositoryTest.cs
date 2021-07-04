using Core.Models.TimeSession;
using MongoDB.Bson;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Test.IntegrationTests.Repositories
{
    [TestFixture]
    public class BreakSessionRepositoryTest
    {
        private BreakSessionRepository SubjectUnderTest { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            SubjectUnderTest = new BreakSessionRepository(ConfigurationUtility.GetDatabaseConfiguration());
        }

        [Test]
        public async Task GetActiveBreakSessionShouldReturnNullWhenNoActiveBreakSessionExist()
        {
            var sessions = new List<BreakSession>
            {
                new BreakSession { UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), EndTime = DateTime.Now.AddHours(-1) }
            };

            await SubjectUnderTest.Add(sessions).ConfigureAwait(false);

            Assert.IsNull(await SubjectUnderTest.GetActiveBreakSession("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetActiveBreakSessionShouldReturnNullWhenActiveBreakSessionAlreadyEnded()
        {
            var sessions = new List<BreakSession>
            {
                new BreakSession { UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), TargetDuration = 1.5 }
            };

            await SubjectUnderTest.Add(sessions).ConfigureAwait(false);

            Assert.IsNull(await SubjectUnderTest.GetActiveBreakSession("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetActiveBreakSessionShouldReturnActiveBreakSessionFound()
        {
            var sessions = new List<BreakSession>
            {
                new BreakSession { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), TargetDuration = 2.5 }
            };

            await SubjectUnderTest.Add(sessions).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetActiveBreakSession("user_id").ConfigureAwait(false);

            Assert.AreEqual(sessions[0].Id, result.Id);
        }

        [Test]
        public async Task GetBreakSessionByDateRangeShouldReturnEmptyCollectionWhenNoBreakSessionFound()
        {
            var start = new DateTime(2021, 1, 2);
            var end = new DateTime(2021, 1, 5);

            var sessions = new List<BreakSession>
            {
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 10, 0), EndTime = new DateTime(2021, 1, 1, 15, 30, 0) },
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 35, 0), EndTime = new DateTime(2021, 1, 1, 15, 50, 0) },
                new BreakSession { UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 55, 0), EndTime = new DateTime(2021, 1, 1, 16, 15, 0) }
            };

            await SubjectUnderTest.Add(sessions).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetBreakSessionByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetBreakSessionByDateRangeShouldReturnBreakSessionsWithinTimeRange()
        {
            var start = new DateTime(2021, 1, 1, 15, 30, 1);
            var end = new DateTime(2021, 1, 1, 15, 54, 59);

            var sessions = new List<BreakSession>
            {
                new BreakSession
                {
                    Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 10, 0), EndTime = new DateTime(2021, 1, 1, 15, 30, 0)
                },
                new BreakSession
                {
                    Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 35, 0), EndTime = new DateTime(2021, 1, 1, 15, 50, 0)
                },
                new BreakSession
                {
                    Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 55, 0), EndTime = new DateTime(2021, 1, 1, 16, 15, 0)
                }
            };

            await SubjectUnderTest.Add(sessions).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetBreakSessionByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(sessions[1].Id, result[0].Id);
        }

        [Test]
        public async Task GetBreakSessionByDateRangeShouldReturnOverlappingBreakSessions()
        {
            var start = new DateTime(2021, 1, 1, 15, 20, 0);
            var end = new DateTime(2021, 1, 1, 16, 05, 0);

            var sessions = new List<BreakSession>
            {
                new BreakSession
                {
                    Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 14, 45, 0), EndTime = new DateTime(2021, 1, 1, 15, 05, 0)
                },
                new BreakSession
                {
                    Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 10, 0), EndTime = new DateTime(2021, 1, 1, 15, 30, 0)
                },
                new BreakSession
                {
                    Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 35, 0), EndTime = new DateTime(2021, 1, 1, 15, 50, 0)
                },
                new BreakSession
                {
                    Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 15, 55, 0), EndTime = new DateTime(2021, 1, 1, 16, 15, 0)
                },
                new BreakSession
                {
                    Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = new DateTime(2021, 1, 1, 16, 20, 0), EndTime = new DateTime(2021, 1, 1, 16, 40, 0)
                }
            };

            await SubjectUnderTest.Add(sessions).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetBreakSessionByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(sessions[1].Id, result[0].Id);
            Assert.AreEqual(sessions[2].Id, result[1].Id);
            Assert.AreEqual(sessions[3].Id, result[2].Id);
        }

        [TearDown]
        public async Task TearDown()
        {
            await SubjectUnderTest.DropCollection().ConfigureAwait(false);
        }
    }
}
