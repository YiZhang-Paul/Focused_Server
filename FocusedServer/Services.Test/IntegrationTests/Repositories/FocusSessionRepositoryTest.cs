using Core.Models.TimeSession;
using MongoDB.Bson;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Test.IntegrationTests.Repositories
{
    [TestFixture]
    public class FocusSessionRepositoryTest
    {
        private FocusSessionRepository SubjectUnderTest { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            SubjectUnderTest = new FocusSessionRepository(ConfigurationUtility.GetDatabaseConfiguration());
        }

        [Test]
        public async Task GetActiveFocusSessionShouldReturnNullWhenNoActiveFocusSessionExist()
        {
            var sessions = new List<FocusSession>
            {
                new FocusSession { UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), EndTime = DateTime.Now.AddHours(-1) }
            };

            await SubjectUnderTest.Add(sessions).ConfigureAwait(false);

            Assert.IsNull(await SubjectUnderTest.GetActiveFocusSession("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetActiveFocusSessionShouldReturnNullWhenActiveFocusSessionAlreadyEnded()
        {
            var sessions = new List<FocusSession>
            {
                new FocusSession { UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), TargetDuration = 1.5 }
            };

            await SubjectUnderTest.Add(sessions).ConfigureAwait(false);

            Assert.IsNull(await SubjectUnderTest.GetActiveFocusSession("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetActiveFocusSessionShouldReturnActiveFocusSessionFound()
        {
            var sessions = new List<FocusSession>
            {
                new FocusSession { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), TargetDuration = 2.5 }
            };

            await SubjectUnderTest.Add(sessions).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetActiveFocusSession("user_id").ConfigureAwait(false);

            Assert.AreEqual(sessions[0].Id, result.Id);
        }

        [TearDown]
        public async Task TearDown()
        {
            await SubjectUnderTest.DropCollection().ConfigureAwait(false);
        }
    }
}
