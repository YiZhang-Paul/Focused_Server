using Core.Models;
using MongoDB.Bson;
using NUnit.Framework;
using Service.Repositories.RepositoryBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Test.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserOwnedRecordRepositoryTest
    {
        private UserOwnedRecordRepository<UserOwnedRecord> SubjectUnderTest { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            SubjectUnderTest = new UserOwnedRecordRepository<UserOwnedRecord>(ConfigurationUtility.GetDatabaseConfiguration(), typeof(UserOwnedRecord).Name);
        }

        [Test]
        public async Task GetShouldReturnNullWhenNoUserOwnedRecordFound()
        {
            var records = new List<UserOwnedRecord>
            {
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_1" },
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_2" },
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_3" }
            };

            await SubjectUnderTest.Add(records).ConfigureAwait(false);

            Assert.IsNull(await SubjectUnderTest.Get("user_id_4", records[2].Id).ConfigureAwait(false));
        }

        [Test]
        public async Task GetShouldReturnUserOwnedRecordFound()
        {
            var records = new List<UserOwnedRecord>
            {
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_1" },
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_2" },
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_3" }
            };

            await SubjectUnderTest.Add(records).ConfigureAwait(false);

            var result = await SubjectUnderTest.Get("user_id_2", records[1].Id).ConfigureAwait(false);

            Assert.AreEqual(records[1].Id, result.Id);
        }

        [Test]
        public async Task GetShouldReturnEmptyCollectionWhenNoUserOwnedRecordFound()
        {
            var records = new List<UserOwnedRecord>
            {
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_1" },
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_2" },
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_3" }
            };

            var ids = new List<string> { ObjectId.GenerateNewId().ToString() };
            await SubjectUnderTest.Add(records).ConfigureAwait(false);

            var result = await SubjectUnderTest.Get("user_id_4", ids).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetShouldReturnUserOwnedRecordsFound()
        {
            var records = new List<UserOwnedRecord>
            {
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_1" },
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_1" },
                new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id_1" }
            };

            var ids = new List<string> { records[1].Id, records[2].Id };
            await SubjectUnderTest.Add(records).ConfigureAwait(false);

            var result = await SubjectUnderTest.Get("user_id_1", ids).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(records[1].Id, result[0].Id);
            Assert.AreEqual(records[2].Id, result[1].Id);
        }

        [Test]
        public void AddShouldThrowWhenUserIdIsMissing()
        {
            var record = new UserOwnedRecord { UserId = " " };

            Assert.ThrowsAsync<ArgumentNullException>(async() => await SubjectUnderTest.Add(record).ConfigureAwait(false));
        }

        [Test]
        public async Task AddShouldReturnIdOnSuccess()
        {
            var record = new UserOwnedRecord { UserId = "user_id" };

            var result = await SubjectUnderTest.Add(record).ConfigureAwait(false);

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }

        [Test]
        public void AddShouldThrowWhenAtLeastOneUserIdIsMissing()
        {
            var records = new List<UserOwnedRecord>
            {
                new UserOwnedRecord { UserId = "user_id_1" },
                new UserOwnedRecord { UserId = " " },
                new UserOwnedRecord { UserId = "user_id_2" }
            };

            Assert.ThrowsAsync<ArgumentNullException>(async () => await SubjectUnderTest.Add(records).ConfigureAwait(false));
        }

        [Test]
        public async Task AddShouldReturnIdsOnSuccess()
        {
            var records = new List<UserOwnedRecord>
            {
                new UserOwnedRecord { UserId = "user_id_1" },
                new UserOwnedRecord { UserId = "user_id_2" },
                new UserOwnedRecord { UserId = "user_id_3" }
            };

            var result = await SubjectUnderTest.Add(records).ConfigureAwait(false);

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.All(_ => !string.IsNullOrWhiteSpace(_)));
        }

        [Test]
        public void ReplaceShouldThrowWhenUserIdIsMissing()
        {
            var record = new UserOwnedRecord { UserId = " " };

            Assert.ThrowsAsync<ArgumentNullException>(async () => await SubjectUnderTest.Replace(record).ConfigureAwait(false));
        }

        [Test]
        public async Task ReplaceShouldReturnNullWhenNoRecordIsReplaced()
        {
            var record = new UserOwnedRecord { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" };

            Assert.IsNull(await SubjectUnderTest.Replace(record).ConfigureAwait(false));
        }

        [Test]
        public async Task ReplaceShouldReturnReplacedRecordOnSuccess()
        {
            var record = new UserOwnedRecord { UserId = "user_id" };
            var id = await SubjectUnderTest.Add(record).ConfigureAwait(false);

            await SubjectUnderTest.Replace(record).ConfigureAwait(false);
            var result = await SubjectUnderTest.Get("user_id", id).ConfigureAwait(false);

            Assert.AreEqual(id, result.Id);
            Assert.AreEqual("user_id", result.UserId);
        }

        [Test]
        public async Task DeleteShouldReturnTrueOnSuccess()
        {
            var record = new UserOwnedRecord { UserId = "user_id" };
            var id = await SubjectUnderTest.Add(record).ConfigureAwait(false);
            Assert.IsNotNull(await SubjectUnderTest.Get("user_id", id).ConfigureAwait(false));

            await SubjectUnderTest.Delete("user_id", id).ConfigureAwait(false);

            Assert.IsNull(await SubjectUnderTest.Get("user_id", id).ConfigureAwait(false));
        }

        [TearDown]
        public async Task TearDown()
        {
            await SubjectUnderTest.DropCollection().ConfigureAwait(false);
        }
    }
}
