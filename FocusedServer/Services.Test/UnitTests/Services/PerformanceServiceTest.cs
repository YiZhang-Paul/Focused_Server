using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Generic;
using Moq;
using NUnit.Framework;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Test.UnitTests.Services
{
    [TestFixture]
    public class PerformanceServiceTest
    {
        private Mock<IWorkItemRepository> WorkItemRepository { get; set; }
        private Mock<IWorkItemService> WorkItemService { get; set; }
        private Mock<IFocusSessionService> FocusSessionService { get; set; }
        private Mock<IBreakSessionService> BreakSessionService { get; set; }
        private PerformanceService SubjectUnderTest { get; set; }

        [SetUp]
        public void Setup()
        {
            WorkItemRepository = new Mock<IWorkItemRepository>();
            WorkItemService = new Mock<IWorkItemService>();
            FocusSessionService = new Mock<IFocusSessionService>();
            BreakSessionService = new Mock<IBreakSessionService>();

            SubjectUnderTest = new PerformanceService
            (
                WorkItemRepository.Object,
                WorkItemService.Object,
                FocusSessionService.Object,
                BreakSessionService.Object
            );
        }

        [Test]
        public async Task GetFocusProgressionByDateShouldReturnDailyFocusProgression()
        {
            var breakdown = new ActivityBreakdownDto { Regular = 3, Recurring = 2.5, Interruption = 1 };
            WorkItemService.Setup(_ => _.GetWorkItemActivityBreakdownByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(breakdown);
            FocusSessionService.Setup(_ => _.GetOverlearningHoursByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(2.5);

            var result = await SubjectUnderTest.GetFocusProgressionByDate("user_id", 2021, 1, 1).ConfigureAwait(false);

            Assert.AreEqual(8, result.Current);
            Assert.AreEqual(8, result.Target);
            Assert.IsTrue(result.IsCompleted);
        }

        [Test]
        public async Task GetTimeTrackingBreakdownByDateShouldReturnTimeTrackingBreakdown()
        {
            var breakdown = new ActivityBreakdownDto { Regular = 3, Recurring = 2.5, Interruption = 1 };
            WorkItemService.Setup(_ => _.GetWorkItemActivityBreakdownByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(breakdown);
            FocusSessionService.Setup(_ => _.GetOverlearningHoursByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(2.5);
            BreakSessionService.Setup(_ => _.GetBreakDurationByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(4);

            var result = await SubjectUnderTest.GetTimeTrackingBreakdownByDate("user_id", 2021, 1, 1).ConfigureAwait(false);

            Assert.AreEqual(9, result.ActivityTime);
            Assert.AreEqual(4, result.BreakTime);
            Assert.AreEqual(11, result.UntrackedTime);
        }

        [Test]
        public async Task GetActivityBreakdownByDaysShouldReturnEmptyCollectionWhenStartDateIsLaterThanEndDate()
        {
            var start = new DateTime(2021, 1, 5);
            var end = new DateTime(2021, 1, 4);

            var result = await SubjectUnderTest.GetActivityBreakdownByDays("user_id", start, end).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetActivityBreakdownByDaysShouldReturnActivityBreakdownsByDays()
        {
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 10);
            var breakdown = new ActivityBreakdownDto { Regular = 1, Recurring = 1, Interruption = 1 };
            WorkItemService.Setup(_ => _.GetWorkItemActivityBreakdownByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(breakdown);
            FocusSessionService.Setup(_ => _.GetOverlearningHoursByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(1);

            var result = await SubjectUnderTest.GetActivityBreakdownByDays("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(9, result.Count);
            WorkItemService.Verify(_ => _.GetWorkItemActivityBreakdownByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Exactly(9));
            FocusSessionService.Verify(_ => _.GetOverlearningHoursByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Exactly(9));
        }

        [Test]
        public async Task GetActivityBreakdownByDateRangeShouldReturnActivityBreakdown()
        {
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 10);
            var breakdown = new ActivityBreakdownDto { Regular = 4, Recurring = 3, Interruption = 2 };
            WorkItemService.Setup(_ => _.GetWorkItemActivityBreakdownByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(breakdown);
            FocusSessionService.Setup(_ => _.GetOverlearningHoursByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(1);

            var result = await SubjectUnderTest.GetActivityBreakdownByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(4, result.Regular);
            Assert.AreEqual(3, result.Recurring);
            Assert.AreEqual(2, result.Interruption);
            Assert.AreEqual(1, result.Overlearning);
        }

        [Test]
        public async Task GetActivityBreakdownByDateRangeShouldDefaultToPastTwoWeeks()
        {
            var breakdown = new ActivityBreakdownDto { Regular = 4, Recurring = 3, Interruption = 2 };
            WorkItemService.Setup(_ => _.GetWorkItemActivityBreakdownByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(breakdown);
            FocusSessionService.Setup(_ => _.GetOverlearningHoursByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(1);

            await SubjectUnderTest.GetActivityBreakdownByDateRange("user_id", null, null).ConfigureAwait(false);

            WorkItemService.Verify(_ => _.GetWorkItemActivityBreakdownByDateRange
            (
                It.IsAny<string>(),
                It.Is<DateTime>(date => (DateTime.Now.AddDays(-14) - date).TotalSeconds < 3),
                It.Is<DateTime>(date => (DateTime.Now - date).TotalSeconds < 3)
            ), Times.Once);

            FocusSessionService.Verify(_ => _.GetOverlearningHoursByDateRange
            (
                It.IsAny<string>(),
                It.Is<DateTime>(date => (DateTime.Now.AddDays(-14) - date).TotalSeconds < 3),
                It.Is<DateTime>(date => (DateTime.Now - date).TotalSeconds < 3)
            ), Times.Once);
        }

        [Test]
        public async Task GetEstimationBreakdownByDateRangeShouldProperlyCountUnderestimatedTime()
        {
            var current = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    Progress = new ProgressionCounter<double> { Current = 3, Target = 4 }
                }
            };

            var overall = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    Progress = new ProgressionCounter<double> { Current = 5, Target = 4 }
                }
            };

            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 10);

            WorkItemService.SetupSequence(_ => _.GetWorkItemProgressionByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(current))
                .Returns(Task.FromResult(overall));

            var result = await SubjectUnderTest.GetEstimationBreakdownByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(2, result.Normal);
            Assert.AreEqual(1, result.Underestimate);
            Assert.AreEqual(0, result.Overestimate);
        }

        [Test]
        public async Task GetEstimationBreakdownByDateRangeShouldProperlyCountOverestimatedTimeWhenOverestimationExceedsThreeHours()
        {
            var current = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    Progress = new ProgressionCounter<double> { Current = 1, Target = 5 }
                }
            };

            var overall = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    // total overestimation exceeds 3 hours
                    Progress = new ProgressionCounter<double> { Current = 1.5, Target = 5, IsCompleted = true }
                }
            };

            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 10);

            WorkItemService.SetupSequence(_ => _.GetWorkItemProgressionByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(current))
                .Returns(Task.FromResult(overall));

            var result = await SubjectUnderTest.GetEstimationBreakdownByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(1, result.Normal);
            Assert.AreEqual(0, result.Underestimate);
            Assert.AreEqual(3.5, result.Overestimate);
        }

        [Test]
        public async Task GetEstimationBreakdownByDateRangeShouldProperlyCountOverestimatedTimeWhenOverestimationReachesSixtyPercent()
        {
            var current = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    Progress = new ProgressionCounter<double> { Current = 1, Target = 5 }
                }
            };

            var overall = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    // total overestimation reaches (5 - 2) / 5 * 100 = 60%
                    Progress = new ProgressionCounter<double> { Current = 2, Target = 5, IsCompleted = true }
                }
            };

            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 10);

            WorkItemService.SetupSequence(_ => _.GetWorkItemProgressionByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(current))
                .Returns(Task.FromResult(overall));

            var result = await SubjectUnderTest.GetEstimationBreakdownByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(1, result.Normal);
            Assert.AreEqual(0, result.Underestimate);
            Assert.AreEqual(3, result.Overestimate);
        }

        [Test]
        public async Task GetEstimationBreakdownByDateRangeShouldNotCountOverestimatedTimeWhenEstimationIsLessThanThirtyMinutes()
        {
            var current = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    Progress = new ProgressionCounter<double> { Current = 0.3, Target = 0.4 }
                }
            };

            var overall = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    // total overestimation reaches 60%, but original estimation is less than 30 minutes
                    Progress = new ProgressionCounter<double> { Current = 0.3, Target = 0.4, IsCompleted = true }
                }
            };

            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 10);

            WorkItemService.SetupSequence(_ => _.GetWorkItemProgressionByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(current))
                .Returns(Task.FromResult(overall));

            var result = await SubjectUnderTest.GetEstimationBreakdownByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(0.3, result.Normal);
            Assert.AreEqual(0, result.Underestimate);
            Assert.AreEqual(0, result.Overestimate);
        }

        [Test]
        public async Task GetEstimationBreakdownByDateRangeShouldNotCountOverestimatedTimeWhenItemIsUnfinished()
        {
            var current = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    Progress = new ProgressionCounter<double> { Current = 1, Target = 5 }
                }
            };

            var overall = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    // work item not completed yet
                    Progress = new ProgressionCounter<double> { Current = 2, Target = 5, IsCompleted = false }
                }
            };

            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 10);

            WorkItemService.SetupSequence(_ => _.GetWorkItemProgressionByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(current))
                .Returns(Task.FromResult(overall));

            var result = await SubjectUnderTest.GetEstimationBreakdownByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(1, result.Normal);
            Assert.AreEqual(0, result.Underestimate);
            Assert.AreEqual(0, result.Overestimate);
        }

        [Test]
        public async Task GetEstimationBreakdownByDateRangeShouldReturnAggregatedEstimationBreakdown()
        {
            var current = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    Progress = new ProgressionCounter<double> { Current = 3, Target = 4 }
                },
                new WorkItemProgressionDto
                {
                    Id = "id_2",
                    Progress = new ProgressionCounter<double> { Current = 1, Target = 5 }
                },
                new WorkItemProgressionDto
                {
                    Id = "id_3",
                    Progress = new ProgressionCounter<double> { Current = 1, Target = 5 }
                }
            };

            var overall = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto
                {
                    Id = "id_1",
                    Progress = new ProgressionCounter<double> { Current = 5, Target = 4 }
                },
                new WorkItemProgressionDto
                {
                    Id = "id_2",
                    Progress = new ProgressionCounter<double> { Current = 1.5, Target = 5, IsCompleted = true }
                },
                new WorkItemProgressionDto
                {
                    Id = "id_3",
                    Progress = new ProgressionCounter<double> { Current = 2, Target = 5, IsCompleted = true }
                }
            };

            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 10);

            WorkItemService.SetupSequence(_ => _.GetWorkItemProgressionByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(current))
                .Returns(Task.FromResult(overall));

            var result = await SubjectUnderTest.GetEstimationBreakdownByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(4, result.Normal);
            Assert.AreEqual(1, result.Underestimate);
            Assert.AreEqual(6.5, result.Overestimate);
        }

        [Test]
        public async Task GetEstimationBreakdownByDateRangeShouldDefaultToPastTwoWeeks()
        {
            WorkItemService.Setup(_ => _.GetWorkItemProgressionByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<WorkItemProgressionDto>());

            await SubjectUnderTest.GetEstimationBreakdownByDateRange("user_id", null, null).ConfigureAwait(false);

            WorkItemService.Verify(_ => _.GetWorkItemProgressionByDateRange
            (
                It.IsAny<string>(),
                It.Is<DateTime>(date => (DateTime.Now.AddDays(-14) - date).TotalSeconds < 3),
                It.Is<DateTime>(date => (DateTime.Now - date).TotalSeconds < 3)
            ), Times.Once);
        }

        [Test]
        public async Task GetDueDateBreakdownByDateRangeShouldReturnDueDateBreakdown()
        {
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 10);
            WorkItemRepository.Setup(_ => _.GetPastDueWorkItemsCount(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(3);
            WorkItemRepository.Setup(_ => _.GetLoomingWorkItemsCount(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(2);

            var result = await SubjectUnderTest.GetDueDateBreakdownByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(3, result.PastDue);
            Assert.AreEqual(2, result.Looming);
        }

        [Test]
        public async Task GetDueDateBreakdownByDateRangeShouldDefaultToPastTwoWeeks()
        {
            WorkItemRepository.Setup(_ => _.GetPastDueWorkItemsCount(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(3);
            WorkItemRepository.Setup(_ => _.GetLoomingWorkItemsCount(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(2);

            await SubjectUnderTest.GetDueDateBreakdownByDateRange("user_id", null, null).ConfigureAwait(false);

            WorkItemRepository.Verify(_ => _.GetPastDueWorkItemsCount
            (
                It.IsAny<string>(),
                It.Is<DateTime>(date => (DateTime.Now.AddDays(-14) - date).TotalSeconds < 3),
                It.Is<DateTime>(date => (DateTime.Now - date).TotalSeconds < 3)
            ), Times.Once);

            WorkItemRepository.Verify(_ => _.GetLoomingWorkItemsCount
            (
                It.IsAny<string>(),
                It.Is<DateTime>(date => (DateTime.Now.AddDays(-14) - date).TotalSeconds < 3),
                It.Is<DateTime>(date => (DateTime.Now.AddDays(1) - date).TotalSeconds < 3)
            ), Times.Once);
        }
    }
}
