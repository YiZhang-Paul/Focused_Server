using NUnit.Framework;
using Service.Repositories;
using System.Threading.Tasks;

namespace Services.Test.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserProfileRepositoryTest
    {
        private UserProfileRepository SubjectUnderTest { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            SubjectUnderTest = new UserProfileRepository(ConfigurationUtility.GetDatabaseConfiguration());
        }

        [Test]
        public async Task GetShouldReturnUserProfile()
        {
            var result = await SubjectUnderTest.Get("60cd1862629e063c384f3ea1").ConfigureAwait(false);

            Assert.AreEqual("Yi Zhang", result.Name);
        }
    }
}
