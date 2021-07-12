using Core.Models;
using MongoDB.Bson;
using NUnit.Framework;
using Service.Repositories.RepositoryBase;
using System.Threading.Tasks;

namespace Services.Test.IntegrationTests.Repositories
{
    [TestFixture]
    public class GenericRecordRepositoryTest
    {
        private GenericRecordRepository<DatabaseEntry> SubjectUnderTest { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            SubjectUnderTest = new GenericRecordRepository<DatabaseEntry>(ConfigurationUtility.GetDatabaseConfiguration(), typeof(DatabaseEntry).Name);
        }

        [Test]
        public async Task ReplaceShouldReturnNullWhenNoDocumentIsReplaced()
        {
            var document = new DatabaseEntry { Id = ObjectId.GenerateNewId().ToString() };

            Assert.IsNull(await SubjectUnderTest.Replace(document).ConfigureAwait(false));
        }

        [Test]
        public async Task ReplaceShouldReturnReplacedDocumentOnSuccess()
        {
            var document = new DatabaseEntry();
            await SubjectUnderTest.Add(document).ConfigureAwait(false);

            var result = await SubjectUnderTest.Replace(document).ConfigureAwait(false);

            Assert.IsNotNull(result.Id);
            Assert.AreEqual(document.Id, result.Id);
        }

        [TearDown]
        public async Task TearDown()
        {
            await SubjectUnderTest.DropCollection().ConfigureAwait(false);
        }
    }
}
