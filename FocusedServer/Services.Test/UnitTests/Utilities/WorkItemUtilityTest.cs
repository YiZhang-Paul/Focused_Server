using Core.Enums;
using Core.Models.Aggregates;
using Core.Models.Generic;
using Core.Models.TimeSession;
using Core.Models.WorkItem;
using NUnit.Framework;
using Service.Utilities;
using System;
using System.Collections.Generic;

namespace Services.Test.UnitTests.Utilities
{
    [TestFixture]
    public class WorkItemUtilityTest
    {
        [Test]
        public void ToWorkItemDtoShouldReturnIncompletedProgression()
        {
            var item = new WorkItemWithTimeSeries
            {
                Status = WorkItemStatus.Highlighted,
                EstimatedHours = 5,
                TimeInfo = new TimeInfo { Created = new DateTime(2021, 1, 1) },
                TimeSeries = new List<TimeSeries>
                {
                    new TimeSeries
                    {
                        StartTime = new DateTime(2021, 1, 25, 5, 15, 0),
                        EndTime = new DateTime(2021, 1, 25, 12, 15, 0)
                    }
                },
                Subtasks = new List<WorkItem>
                {
                    new WorkItem { Status = WorkItemStatus.Highlighted },
                    new WorkItem { Status = WorkItemStatus.Completed }
                },
                Checklist = new List<ChecklistEntry>
                {
                    new ChecklistEntry { Description = "description_1", IsCompleted = true },
                    new ChecklistEntry { Description = "description_2", IsCompleted = false },
                    new ChecklistEntry { Description = "description_3", IsCompleted = true }
                }
            };

            var result = WorkItemUtility.ToWorkItemDto(item);

            Assert.AreEqual(7, result.ItemProgress.Current);
            Assert.AreEqual(5, result.ItemProgress.Target);
            Assert.IsFalse(result.ItemProgress.IsCompleted);
            Assert.AreEqual(1, result.SubtaskProgress.Current);
            Assert.AreEqual(2, result.SubtaskProgress.Target);
            Assert.IsFalse(result.SubtaskProgress.IsCompleted);
            Assert.AreEqual(2, result.ChecklistProgress.Current);
            Assert.AreEqual(3, result.ChecklistProgress.Target);
            Assert.IsFalse(result.ChecklistProgress.IsCompleted);
        }

        [Test]
        public void IsOverestimatedShouldReturnFalseWhenItemIsUnfinished()
        {
            var progress = new ProgressionCounter<double> { Current = 2, Target = 5, IsCompleted = false };

            Assert.IsFalse(WorkItemUtility.IsOverestimated(progress));
        }

        [Test]
        public void IsOverestimatedShouldReturnFalseWhenEstimationIsLessThanThirtyMinutes()
        {
            var progress = new ProgressionCounter<double> { Current = 0.3, Target = 0.4, IsCompleted = true };

            Assert.IsFalse(WorkItemUtility.IsOverestimated(progress));
        }

        [Test]
        public void IsOverestimatedShouldReturnTrueWhenOverestimationExceedsThreeHours()
        {
            var progress = new ProgressionCounter<double> { Current = 1.5, Target = 5, IsCompleted = true };

            Assert.IsTrue(WorkItemUtility.IsOverestimated(progress));
        }

        [Test]
        public void IsOverestimatedShouldReturnTrueWhenOverestimationReachesSixtyPercent()
        {
            var progress = new ProgressionCounter<double> { Current = 2, Target = 5, IsCompleted = true };

            Assert.IsTrue(WorkItemUtility.IsOverestimated(progress));
        }

        [Test]
        public void ToWorkItemDtoShouldReturnCompletedProgression()
        {
            var item = new WorkItemWithTimeSeries
            {
                Status = WorkItemStatus.Completed,
                EstimatedHours = 5,
                TimeInfo = new TimeInfo { Created = new DateTime(2021, 1, 1) },
                TimeSeries = new List<TimeSeries>
                {
                    new TimeSeries
                    {
                        StartTime = new DateTime(2021, 1, 25, 5, 15, 0),
                        EndTime = new DateTime(2021, 1, 25, 12, 15, 0)
                    }
                },
                Subtasks = new List<WorkItem>
                {
                    new WorkItem { Status = WorkItemStatus.Completed },
                    new WorkItem { Status = WorkItemStatus.Completed }
                },
                Checklist = new List<ChecklistEntry>
                {
                    new ChecklistEntry { IsCompleted = true },
                    new ChecklistEntry { IsCompleted = true },
                    new ChecklistEntry { IsCompleted = true }
                }
            };

            var result = WorkItemUtility.ToWorkItemDto(item);

            Assert.AreEqual(7, result.ItemProgress.Current);
            Assert.AreEqual(5, result.ItemProgress.Target);
            Assert.IsTrue(result.ItemProgress.IsCompleted);
            Assert.AreEqual(2, result.SubtaskProgress.Current);
            Assert.AreEqual(2, result.SubtaskProgress.Target);
            Assert.IsTrue(result.SubtaskProgress.IsCompleted);
            Assert.AreEqual(3, result.ChecklistProgress.Current);
            Assert.AreEqual(3, result.ChecklistProgress.Target);
            Assert.IsTrue(result.ChecklistProgress.IsCompleted);
        }

        [Test]
        public void ToWorkItemDtoShouldReturnProgressionWithinTimeRange()
        {
            var item = new WorkItemWithTimeSeries
            {
                Status = WorkItemStatus.Completed,
                EstimatedHours = 5,
                TimeInfo = new TimeInfo { Created = new DateTime(2021, 1, 1) },
                TimeSeries = new List<TimeSeries>
                {
                    new TimeSeries
                    {
                        StartTime = new DateTime(2021, 1, 25, 5, 15, 0),
                        EndTime = new DateTime(2021, 1, 25, 12, 15, 0)
                    }
                }
            };

            var start = new DateTime(2021, 1, 25, 7, 15, 0);
            var end = new DateTime(2021, 1, 25, 10, 15, 0);

            var result = WorkItemUtility.ToWorkItemDto(item, start, end);

            Assert.AreEqual(3, result.ItemProgress.Current);
            Assert.AreEqual(5, result.ItemProgress.Target);
            Assert.IsTrue(result.ItemProgress.IsCompleted);
        }
    }
}
