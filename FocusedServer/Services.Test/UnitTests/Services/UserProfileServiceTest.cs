using Core.Interfaces.Repositories;
using Core.Models.User;
using Moq;
using NUnit.Framework;
using Service.Services;
using System.Threading.Tasks;

namespace Services.Test.UnitTests.Services
{
    [TestFixture]
    public class UserProfileServiceTest
    {
        private Mock<IUserProfileRepository> UserProfileRepository { get; set; }
        private UserProfileService SubjectUnderTest { get; set; }

        [SetUp]
        public void Setup()
        {
            UserProfileRepository = new Mock<IUserProfileRepository>();
            SubjectUnderTest = new UserProfileService(UserProfileRepository.Object);
        }

        [Test]
        public async Task UpdateUserRatingsShouldReturnNullWhenUserDoesNotExist()
        {
            UserProfileRepository.Setup(_ => _.Get(It.IsAny<string>())).ReturnsAsync((UserProfile)null);

            Assert.IsNull(await SubjectUnderTest.UpdateUserRatings("user_id", new PerformanceRating()).ConfigureAwait(false));
            UserProfileRepository.Verify(_ => _.Replace(It.IsAny<UserProfile>()), Times.Never);
        }

        [Test]
        public async Task UpdateUserRatingsShouldReturnNullWhenUpdateFailed()
        {
            UserProfileRepository.Setup(_ => _.Get(It.IsAny<string>())).ReturnsAsync(new UserProfile());
            UserProfileRepository.Setup(_ => _.Replace(It.IsAny<UserProfile>())).ReturnsAsync((UserProfile)null);

            Assert.IsNull(await SubjectUnderTest.UpdateUserRatings("user_id", new PerformanceRating()).ConfigureAwait(false));
            UserProfileRepository.Verify(_ => _.Replace(It.IsAny<UserProfile>()), Times.Once);
        }

        [Test]
        public async Task UpdateUserRatingsShouldReturnUpdatedRatings()
        {
            var ratings = new PerformanceRating
            {
                Determination = 0.6,
                Estimation = 0.7,
                Planning = 0.5,
                Adaptability = 0.7,
                Sustainability = 0.4
            };

            var profile = new UserProfile();
            UserProfileRepository.Setup(_ => _.Get(It.IsAny<string>())).ReturnsAsync(profile);
            UserProfileRepository.Setup(_ => _.Replace(It.IsAny<UserProfile>())).ReturnsAsync(profile);

            var result = await SubjectUnderTest.UpdateUserRatings("user_id", ratings).ConfigureAwait(false);

            Assert.AreEqual(ratings.Determination, result.Determination);
            Assert.AreEqual(ratings.Estimation, result.Estimation);
            Assert.AreEqual(ratings.Planning, result.Planning);
            Assert.AreEqual(ratings.Adaptability, result.Adaptability);
            Assert.AreEqual(ratings.Sustainability, result.Sustainability);
        }
    }
}
