using Core.Dtos;
using Core.Enums;
using Core.Models.Generic;
using Core.Models.WorkItem;
using NUnit.Framework;
using Service.Utilities;
using System;
using System.Collections.Generic;

namespace Services.Test.UnitTests.Utilities
{
    [TestFixture]
    public class PerformanceRatingUtilityTest
    {
        [Test]
        public void GetEstimationRatingShouldReturnZeroWhenNoCompletedItemsFound()
        {
            var progressions = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto { Progress = new ProgressionCounter<double> { IsCompleted = false } },
                new WorkItemProgressionDto { Progress = new ProgressionCounter<double> { IsCompleted = false } },
                new WorkItemProgressionDto { Progress = new ProgressionCounter<double> { IsCompleted = false } }
            };

            var result = PerformanceRatingUtility.GetEstimationRating(progressions);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetEstimationRatingShouldReturnCorrectEstimationRating()
        {
            var progressions = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto { Progress = new ProgressionCounter<double> { Current = 0.05, Target = 0.4, IsCompleted = true } },
                new WorkItemProgressionDto { Progress = new ProgressionCounter<double> { Current = 3, Target = 3.5, IsCompleted = true } },
                new WorkItemProgressionDto { Progress = new ProgressionCounter<double> { Current = 0.05, Target = 0.4, IsCompleted = false } },
                new WorkItemProgressionDto { Progress = new ProgressionCounter<double> { Current = 3, Target = 2, IsCompleted = true } },
                new WorkItemProgressionDto { Progress = new ProgressionCounter<double> { Current = 5, Target = 6, IsCompleted = true } },
                new WorkItemProgressionDto { Progress = new ProgressionCounter<double> { Current = 2, Target = 6, IsCompleted = true } }
            };

            var result = PerformanceRatingUtility.GetEstimationRating(progressions);

            Assert.AreEqual(0.6, result);
        }

        [Test]
        public void GetPlanningRatingShouldReturnZeroWhenNoNonInterruptionItemsFound()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Type = WorkItemType.Interruption },
                new WorkItem { Type = WorkItemType.Interruption },
                new WorkItem { Type = WorkItemType.Interruption }
            };

            var result = PerformanceRatingUtility.GetPlanningRating(items);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetPlanningRatingShouldReturnCorrectPlanningRating()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Type = WorkItemType.Regular, DueDate = null },
                new WorkItem { Type = WorkItemType.Recurring },
                new WorkItem { Type = WorkItemType.Interruption },
                new WorkItem { Type = WorkItemType.Recurring },
                new WorkItem
                {
                    Type = WorkItemType.Regular,
                    Status = WorkItemStatus.Highlighted,
                    DueDate = new DateTime(2021, 1, 1)
                },
                new WorkItem
                {
                    Type = WorkItemType.Regular,
                    Status = WorkItemStatus.Completed,
                    DueDate = new DateTime(2021, 1, 4),
                    CompletionRecords = new List<CompletionRecord>
                    {
                        new CompletionRecord { Time = new DateTime(2021, 1, 5), IsPastDue = true }
                    }
                }
            };

            var result = PerformanceRatingUtility.GetPlanningRating(items);

            Assert.AreEqual(0.6, result);
        }

        [Test]
        public void GetAdaptabilityRatingShouldReturnZeroWhenNoInterruptionItemsFound()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Type = WorkItemType.Regular },
                new WorkItem { Type = WorkItemType.Recurring },
                new WorkItem { Type = WorkItemType.Regular }
            };

            var result = PerformanceRatingUtility.GetAdaptabilityRating(items);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetAdaptabilityRatingShouldReturnCorrectAdaptabilityRating()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Type = WorkItemType.Regular },
                new WorkItem
                {
                    Type = WorkItemType.Interruption,
                    Status = WorkItemStatus.Highlighted,
                    TimeInfo = new TimeInfo { Created = DateTime.Now.Date.AddHours(-1) }
                },
                new WorkItem
                {
                    Type = WorkItemType.Interruption,
                    Status = WorkItemStatus.Highlighted,
                    TimeInfo = new TimeInfo { Created = DateTime.Now.Date }
                },
                new WorkItem
                {
                    Type = WorkItemType.Interruption,
                    Status = WorkItemStatus.Highlighted,
                    TimeInfo = new TimeInfo { Created = DateTime.Now.Date }
                },
                new WorkItem
                {
                    Type = WorkItemType.Interruption,
                    Status = WorkItemStatus.Completed,
                    CompletionRecords = new List<CompletionRecord>
                    {
                        new CompletionRecord { Time = new DateTime(2021, 1, 5), IsPastDue = false }
                    }
                },
                new WorkItem
                {
                    Type = WorkItemType.Interruption,
                    Status = WorkItemStatus.Completed,
                    CompletionRecords = new List<CompletionRecord>
                    {
                        new CompletionRecord { Time = new DateTime(2021, 1, 5), IsPastDue = true }
                    }
                }
            };

            var result = PerformanceRatingUtility.GetAdaptabilityRating(items);

            Assert.AreEqual(0.6, result);
        }

        [Test]
        public void GetSustainabilityRatingShouldReturnZeroWhenNoUserActivityFound()
        {
            var result = PerformanceRatingUtility.GetSustainabilityRating(new List<double>());

            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetSustainabilityRatingShouldReturnCorrectSustainabilityRating()
        {
            var durations = new List<double> { 5, 1, 3, 4, 7 };

            var result = PerformanceRatingUtility.GetSustainabilityRating(durations);

            Assert.IsTrue(Math.Abs(result - 0.667) < 0.001);
        }
    }
}
