using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Models.Generic;
using Core.Models.TimeSession;
using Core.Models.WorkItem;
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
    public class WorkItemServiceTest
    {
        private Mock<IWorkItemRepository> WorkItemRepository { get; set; }
        private Mock<ITimeSeriesRepository> TimeSeriesRepository { get; set; }
        private Mock<IFocusSessionRepository> FocusSessionRepository { get; set; }
        private WorkItemService SubjectUnderTest { get; set; }

        [SetUp]
        public void Setup()
        {
            WorkItemRepository = new Mock<IWorkItemRepository>();
            TimeSeriesRepository = new Mock<ITimeSeriesRepository>();
            FocusSessionRepository = new Mock<IFocusSessionRepository>();

            SubjectUnderTest = new WorkItemService
            (
                WorkItemRepository.Object,
                TimeSeriesRepository.Object,
                FocusSessionRepository.Object
            );
        }

        [Test]
        public async Task CreateWorkItemShouldReturnNullOnCreationFailure()
        {
            WorkItemRepository.Setup(_ => _.Add(It.IsAny<WorkItem>())).ThrowsAsync(new Exception());

            Assert.IsNull(await SubjectUnderTest.CreateWorkItem(new WorkItemDto()).ConfigureAwait(false));
        }

        [Test]
        public async Task CreateWorkItemShouldReturnWorkItemId()
        {
            var item = new WorkItemDto
            {
                Name = " item_name ",
                ItemProgress = new ProgressionCounter<double> { Target = 5 }
            };

            WorkItemRepository.Setup(_ => _.Add(It.IsAny<WorkItem>())).ReturnsAsync("item_id");

            var result = await SubjectUnderTest.CreateWorkItem(item).ConfigureAwait(false);

            Assert.AreEqual("item_id", result);
            WorkItemRepository.Verify(_ => _.Add(It.Is<WorkItem>(item => item.Name == "item_name" && item.EstimatedHours == 5)), Times.Once);
        }

        [Test]
        public async Task UpdateWorkItemShouldSetLastModifiedTime()
        {
            var item = new WorkItem
            {
                TimeInfo = new TimeInfo { LastModified = new DateTime(2021, 1, 1) }
            };

            await SubjectUnderTest.UpdateWorkItem(item).ConfigureAwait(false);

            Assert.AreEqual(DateTime.UtcNow.ToLongDateString(), item.TimeInfo.LastModified.ToLongDateString());
        }

        [Test]
        public async Task StartWorkItemShouldReturnFalseWhenWorkItemDoesNotExist()
        {
            WorkItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((WorkItem)null);
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());

            Assert.IsFalse(await SubjectUnderTest.StartWorkItem("user_id", "item_id").ConfigureAwait(false));
        }

        [Test]
        public async Task StartWorkItemShouldReturnFalseWhenNoActiveFocusSessionExist()
        {
            WorkItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync((FocusSession)null);

            Assert.IsFalse(await SubjectUnderTest.StartWorkItem("user_id", "item_id").ConfigureAwait(false));
        }

        [Test]
        public async Task StartWorkItemShouldReturnFalseWhenFailedToStopWorkItem()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            WorkItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            WorkItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());
            TimeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<TimeSeries>());

            Assert.IsFalse(await SubjectUnderTest.StartWorkItem("user_id", "item_id").ConfigureAwait(false));
        }

        [Test]
        public async Task StartWorkItemShouldReturnFalseWhenFailedToAddTimeSeries()
        {
            WorkItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            WorkItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(new List<WorkItem>());
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());
            TimeSeriesRepository.Setup(_ => _.Add(It.IsAny<TimeSeries>())).ReturnsAsync(string.Empty);

            var result = await SubjectUnderTest.StartWorkItem("user_id", "item_id").ConfigureAwait(false);

            Assert.IsFalse(result);
            TimeSeriesRepository.Verify(_ => _.Add(It.IsAny<TimeSeries>()), Times.Once);
            WorkItemRepository.Verify(_ => _.Replace(It.IsAny<WorkItem>()), Times.Never);
        }

        [Test]
        public async Task StartWorkItemShouldReturnFalseWhenFailedToUpdateWorkItem()
        {
            WorkItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            WorkItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(new List<WorkItem>());
            WorkItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync((WorkItem)null);
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());
            TimeSeriesRepository.Setup(_ => _.Add(It.IsAny<TimeSeries>())).ReturnsAsync("time_series_id");

            var result = await SubjectUnderTest.StartWorkItem("user_id", "item_id").ConfigureAwait(false);

            Assert.IsFalse(result);
            WorkItemRepository.Verify(_ => _.Replace(It.IsAny<WorkItem>()), Times.Once);
        }

        [Test]
        public async Task StartWorkItemShouldReturnTrueOnSuccess()
        {
            WorkItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            WorkItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(new List<WorkItem>());
            WorkItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync(new WorkItem());
            FocusSessionRepository.Setup(_ => _.GetUnfinishedFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());
            TimeSeriesRepository.Setup(_ => _.Add(It.IsAny<TimeSeries>())).ReturnsAsync("time_series_id");

            var result = await SubjectUnderTest.StartWorkItem("user_id", "item_id").ConfigureAwait(false);

            Assert.IsTrue(result);
            TimeSeriesRepository.Verify(_ => _.Add(It.Is<TimeSeries>(series => series.Type == TimeSeriesType.WorkItem)), Times.Once);
            WorkItemRepository.Verify(_ => _.Replace(It.Is<WorkItem>(item => item.Status == WorkItemStatus.Ongoing)), Times.Once);
        }

        [Test]
        public async Task StopWorkItemShouldReturnTrueWhenNoOngoingWorkItemExist()
        {
            WorkItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(new List<WorkItem>());

            Assert.IsTrue(await SubjectUnderTest.StopWorkItem("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task StopWorkItemShouldReturnFalseWhenNoAssociatedTimeSeriesExist()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            WorkItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            TimeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<TimeSeries>());

            Assert.IsFalse(await SubjectUnderTest.StopWorkItem("user_id").ConfigureAwait(false));
            TimeSeriesRepository.Verify(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task StopWorkItemShouldReturnFalseWhenNoOpenTimeSeriesFound()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            var series = new List<TimeSeries> { new TimeSeries { EndTime = new DateTime(2021, 1, 1) } };
            WorkItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            TimeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(series);

            Assert.IsFalse(await SubjectUnderTest.StopWorkItem("user_id").ConfigureAwait(false));
            TimeSeriesRepository.Verify(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task StopWorkItemShouldReturnFalseWhenFailedToUpdateTimeSeries()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            var series = new List<TimeSeries> { new TimeSeries { EndTime = null } };
            WorkItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            TimeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(series);
            TimeSeriesRepository.Setup(_ => _.Replace(It.IsAny<TimeSeries>())).ReturnsAsync((TimeSeries)null);

            Assert.IsFalse(await SubjectUnderTest.StopWorkItem("user_id").ConfigureAwait(false));
            TimeSeriesRepository.Verify(_ => _.Replace(It.IsAny<TimeSeries>()), Times.Once);
        }

        [Test]
        public async Task StopWorkItemShouldReturnFalseWhenFailedToUpdateWorkItem()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            var series = new List<TimeSeries> { new TimeSeries { EndTime = null } };
            WorkItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            WorkItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync((WorkItem)null);
            TimeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(series);
            TimeSeriesRepository.Setup(_ => _.Replace(It.IsAny<TimeSeries>())).ReturnsAsync(series.Last());

            Assert.IsFalse(await SubjectUnderTest.StopWorkItem("user_id").ConfigureAwait(false));
            WorkItemRepository.Verify(_ => _.Replace(It.IsAny<WorkItem>()), Times.Once);
        }

        [Test]
        public async Task StopWorkItemShouldReturnTrueOnSuccess()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            var series = new List<TimeSeries> { new TimeSeries { EndTime = null } };
            WorkItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            WorkItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync(items.Last());
            TimeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(series);
            TimeSeriesRepository.Setup(_ => _.Replace(It.IsAny<TimeSeries>())).ReturnsAsync(series.Last());

            Assert.IsTrue(await SubjectUnderTest.StopWorkItem("user_id").ConfigureAwait(false));

            TimeSeriesRepository.Verify(_ => _.Replace(It.Is<TimeSeries>(series => series.EndTime != null)), Times.Once);
            WorkItemRepository.Verify(_ => _.Replace(It.Is<WorkItem>(item => item.Status == WorkItemStatus.Highlighted)), Times.Once);
        }

        [Test]
        public async Task UpdateWorkItemMetaShouldReturnNullWhenWorkItemDoesNotExist()
        {
            var meta = new WorkItemDto { Id = "item_id", UserId = "user_id" };
            WorkItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((WorkItem)null);

            Assert.IsNull(await SubjectUnderTest.UpdateWorkItemMeta(meta).ConfigureAwait(false));
            WorkItemRepository.Verify(_ => _.Replace(It.IsAny<WorkItem>()), Times.Never);
        }

        [Test]
        public async Task UpdateWorkItemMetaShouldReturnNullWhenFailedToUpdateWorkItem()
        {
            var meta = new WorkItemDto { Id = "item_id", UserId = "user_id" };
            WorkItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            WorkItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync((WorkItem)null);

            Assert.IsNull(await SubjectUnderTest.UpdateWorkItemMeta(meta).ConfigureAwait(false));
            WorkItemRepository.Verify(_ => _.Replace(It.IsAny<WorkItem>()), Times.Once);
        }

        [Test]
        public async Task UpdateWorkItemMetaShouldReturnUpdatedWorkItemMeta()
        {
            var meta = new WorkItemDto
            {
                Id = "item_id",
                UserId = "user_id",
                Name = "current_name",
                Type = WorkItemType.Regular,
                Priority = WorkItemPriority.UrgentImportant,
                Status = WorkItemStatus.Highlighted,
                ItemProgress = new ProgressionCounter<double> { Target = 5 }
            };

            var item = new WorkItem
            {
                Name = "previous_name",
                Type = WorkItemType.Interruption,
                Priority = WorkItemPriority.ImportantNotUrgent,
                Status = WorkItemStatus.Idle,
                EstimatedHours = 3
            };

            WorkItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(item);
            WorkItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync(item);
            WorkItemRepository.Setup(_ => _.GetWorkItemMeta(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(meta);

            Assert.IsNotNull(await SubjectUnderTest.UpdateWorkItemMeta(meta).ConfigureAwait(false));

            WorkItemRepository.Verify(_ => _.Replace(It.Is<WorkItem>
            (
                item => item.Name == "current_name" &&
                        item.Type == WorkItemType.Regular &&
                        item.Priority == WorkItemPriority.UrgentImportant &&
                        item.Status == WorkItemStatus.Highlighted &&
                        item.EstimatedHours == 5
            )), Times.Once);
        }

        [Test]
        public async Task GetWorkItemActivityBreakdownByDateRangeShouldReturnActivityBreakdown()
        {
            var progression = new List<WorkItemProgressionDto>
            {
                new WorkItemProgressionDto { Type = WorkItemType.Regular, Progress = new ProgressionCounter<double> { Current = 5 } },
                new WorkItemProgressionDto { Type = WorkItemType.Recurring, Progress = new ProgressionCounter<double> { Current = 3 } },
                new WorkItemProgressionDto { Type = WorkItemType.Interruption, Progress = new ProgressionCounter<double> { Current = 2 } },
                new WorkItemProgressionDto { Type = WorkItemType.Regular, Progress = new ProgressionCounter<double> { Current = 4 } }
            };

            var ids = new List<string> { "item_id" };
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 2);
            TimeSeriesRepository.Setup(_ => _.GetDataSourceIdsByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TimeSeriesType>())).ReturnsAsync(ids);
            WorkItemRepository.Setup(_ => _.GetWorkItemProgressionByDateRange(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(progression);

            var result = await SubjectUnderTest.GetWorkItemActivityBreakdownByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(9, result.Regular);
            Assert.AreEqual(3, result.Recurring);
            Assert.AreEqual(2, result.Interruption);
            Assert.AreEqual(0, result.Overlearning);
        }
    }
}
