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
        private FocusSessionRepository _repository;

        [OneTimeSetUp]
        public void Setup()
        {
            _repository = new FocusSessionRepository(ConfigurationUtility.GetDatabaseConfiguration());
        }

        [Test]
        public async Task GetActiveFocusSessionShouldReturnNullWhenNoActiveFocusSessionExist()
        {
            var sessions = new List<FocusSession>
            {
                new FocusSession { UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), EndTime = DateTime.Now.AddHours(-1) }
            };

            await _repository.Add(sessions).ConfigureAwait(false);

            Assert.IsNull(await _repository.GetActiveFocusSession("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetActiveFocusSessionShouldReturnNullWhenActiveFocusSessionAlreadyEnded()
        {
            var sessions = new List<FocusSession>
            {
                new FocusSession { UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), TargetDuration = 1.5 }
            };

            await _repository.Add(sessions).ConfigureAwait(false);

            Assert.IsNull(await _repository.GetActiveFocusSession("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task GetActiveFocusSessionShouldReturnActiveFocusSessionFound()
        {
            var sessions = new List<FocusSession>
            {
                new FocusSession { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", StartTime = DateTime.Now.AddHours(-2), TargetDuration = 2.5 }
            };

            await _repository.Add(sessions).ConfigureAwait(false);

            var result = await _repository.GetActiveFocusSession("user_id").ConfigureAwait(false);

            Assert.AreEqual(sessions[0].Id, result.Id);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _repository.DropCollection().ConfigureAwait(false);
        }
    }
}
