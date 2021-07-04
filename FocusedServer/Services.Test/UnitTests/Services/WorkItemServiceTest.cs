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
        private Mock<IWorkItemRepository> _workItemRepository;
        private Mock<ITimeSeriesRepository> _timeSeriesRepository;
        private Mock<IFocusSessionRepository> _focusSessionRepository;
        private WorkItemService _service;

        [SetUp]
        public void Setup()
        {
            _workItemRepository = new Mock<IWorkItemRepository>();
            _timeSeriesRepository = new Mock<ITimeSeriesRepository>();
            _focusSessionRepository = new Mock<IFocusSessionRepository>();

            _service = new WorkItemService
            (
                _workItemRepository.Object,
                _timeSeriesRepository.Object,
                _focusSessionRepository.Object
            );
        }

        [Test]
        public async Task CreateWorkItemShouldReturnNullOnCreationFailure()
        {
            _workItemRepository.Setup(_ => _.Add(It.IsAny<WorkItem>())).ThrowsAsync(new Exception());

            Assert.IsNull(await _service.CreateWorkItem(new WorkItemDto()).ConfigureAwait(false));
        }

        [Test]
        public async Task CreateWorkItemShouldReturnWorkItemId()
        {
            var item = new WorkItemDto
            {
                Name = " item_name ",
                ItemProgress = new ProgressionCounter<double> { Target = 5 }
            };

            _workItemRepository.Setup(_ => _.Add(It.IsAny<WorkItem>())).ReturnsAsync("item_id");

            var result = await _service.CreateWorkItem(item).ConfigureAwait(false);

            Assert.AreEqual("item_id", result);
            _workItemRepository.Verify(_ => _.Add(It.Is<WorkItem>(item => item.Name == "item_name" && item.EstimatedHours == 5)), Times.Once);
        }

        [Test]
        public async Task UpdateWorkItemShouldSetLastModifiedTime()
        {
            var item = new WorkItem
            {
                TimeInfo = new TimeInfo { LastModified = new DateTime(2021, 1, 1) }
            };

            await _service.UpdateWorkItem(item).ConfigureAwait(false);

            Assert.AreEqual(DateTime.UtcNow.ToLongDateString(), item.TimeInfo.LastModified.ToLongDateString());
        }

        [Test]
        public async Task StartWorkItemShouldReturnFalseWhenWorkItemDoesNotExist()
        {
            _workItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((WorkItem)null);
            _focusSessionRepository.Setup(_ => _.GetActiveFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());

            Assert.IsFalse(await _service.StartWorkItem("user_id", "item_id").ConfigureAwait(false));
        }

        [Test]
        public async Task StartWorkItemShouldReturnFalseWhenNoActiveFocusSessionExist()
        {
            _workItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            _focusSessionRepository.Setup(_ => _.GetActiveFocusSession(It.IsAny<string>())).ReturnsAsync((FocusSession)null);

            Assert.IsFalse(await _service.StartWorkItem("user_id", "item_id").ConfigureAwait(false));
        }

        [Test]
        public async Task StartWorkItemShouldReturnFalseWhenFailedToStopWorkItem()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            _workItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            _workItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            _focusSessionRepository.Setup(_ => _.GetActiveFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());
            _timeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<TimeSeries>());

            Assert.IsFalse(await _service.StartWorkItem("user_id", "item_id").ConfigureAwait(false));
        }

        [Test]
        public async Task StartWorkItemShouldReturnFalseWhenFailedToAddTimeSeries()
        {
            _workItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            _workItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(new List<WorkItem>());
            _focusSessionRepository.Setup(_ => _.GetActiveFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());
            _timeSeriesRepository.Setup(_ => _.Add(It.IsAny<TimeSeries>())).ReturnsAsync(string.Empty);

            var result = await _service.StartWorkItem("user_id", "item_id").ConfigureAwait(false);

            Assert.IsFalse(result);
            _timeSeriesRepository.Verify(_ => _.Add(It.IsAny<TimeSeries>()), Times.Once);
            _workItemRepository.Verify(_ => _.Replace(It.IsAny<WorkItem>()), Times.Never);
        }

        [Test]
        public async Task StartWorkItemShouldReturnFalseWhenFailedToUpdateWorkItem()
        {
            _workItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            _workItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(new List<WorkItem>());
            _workItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync((WorkItem)null);
            _focusSessionRepository.Setup(_ => _.GetActiveFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());
            _timeSeriesRepository.Setup(_ => _.Add(It.IsAny<TimeSeries>())).ReturnsAsync("time_series_id");

            var result = await _service.StartWorkItem("user_id", "item_id").ConfigureAwait(false);

            Assert.IsFalse(result);
            _workItemRepository.Verify(_ => _.Replace(It.IsAny<WorkItem>()), Times.Once);
        }

        [Test]
        public async Task StartWorkItemShouldReturnTrueOnSuccess()
        {
            _workItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            _workItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(new List<WorkItem>());
            _workItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync(new WorkItem());
            _focusSessionRepository.Setup(_ => _.GetActiveFocusSession(It.IsAny<string>())).ReturnsAsync(new FocusSession());
            _timeSeriesRepository.Setup(_ => _.Add(It.IsAny<TimeSeries>())).ReturnsAsync("time_series_id");

            var result = await _service.StartWorkItem("user_id", "item_id").ConfigureAwait(false);

            Assert.IsTrue(result);
            _timeSeriesRepository.Verify(_ => _.Add(It.Is<TimeSeries>(series => series.Type == TimeSeriesType.WorkItem)), Times.Once);
            _workItemRepository.Verify(_ => _.Replace(It.Is<WorkItem>(item => item.Status == WorkItemStatus.Ongoing)), Times.Once);
        }

        [Test]
        public async Task StopWorkItemShouldReturnTrueWhenNoOngoingWorkItemExist()
        {
            _workItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(new List<WorkItem>());

            Assert.IsTrue(await _service.StopWorkItem("user_id").ConfigureAwait(false));
        }

        [Test]
        public async Task StopWorkItemShouldReturnFalseWhenNoAssociatedTimeSeriesExist()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            _workItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            _timeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<TimeSeries>());

            Assert.IsFalse(await _service.StopWorkItem("user_id").ConfigureAwait(false));
            _timeSeriesRepository.Verify(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task StopWorkItemShouldReturnFalseWhenNoOpenTimeSeriesFound()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            var series = new List<TimeSeries> { new TimeSeries { EndTime = new DateTime(2021, 1, 1) } };
            _workItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            _timeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(series);

            Assert.IsFalse(await _service.StopWorkItem("user_id").ConfigureAwait(false));
            _timeSeriesRepository.Verify(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task StopWorkItemShouldReturnFalseWhenFailedToUpdateTimeSeries()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            var series = new List<TimeSeries> { new TimeSeries { EndTime = null } };
            _workItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            _timeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(series);
            _timeSeriesRepository.Setup(_ => _.Replace(It.IsAny<TimeSeries>())).ReturnsAsync((TimeSeries)null);

            Assert.IsFalse(await _service.StopWorkItem("user_id").ConfigureAwait(false));
            _timeSeriesRepository.Verify(_ => _.Replace(It.IsAny<TimeSeries>()), Times.Once);
        }

        [Test]
        public async Task StopWorkItemShouldReturnFalseWhenFailedToUpdateWorkItem()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            var series = new List<TimeSeries> { new TimeSeries { EndTime = null } };
            _workItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            _workItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync((WorkItem)null);
            _timeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(series);
            _timeSeriesRepository.Setup(_ => _.Replace(It.IsAny<TimeSeries>())).ReturnsAsync(series.Last());

            Assert.IsFalse(await _service.StopWorkItem("user_id").ConfigureAwait(false));
            _workItemRepository.Verify(_ => _.Replace(It.IsAny<WorkItem>()), Times.Once);
        }

        [Test]
        public async Task StopWorkItemShouldReturnTrueOnSuccess()
        {
            var items = new List<WorkItem> { new WorkItem { Id = "id_1" } };
            var series = new List<TimeSeries> { new TimeSeries { EndTime = null } };
            _workItemRepository.Setup(_ => _.GetWorkItems(It.IsAny<string>(), It.IsAny<WorkItemStatus>())).ReturnsAsync(items);
            _workItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync(items.Last());
            _timeSeriesRepository.Setup(_ => _.GetTimeSeriesByDataSource(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(series);
            _timeSeriesRepository.Setup(_ => _.Replace(It.IsAny<TimeSeries>())).ReturnsAsync(series.Last());

            Assert.IsTrue(await _service.StopWorkItem("user_id").ConfigureAwait(false));

            _timeSeriesRepository.Verify(_ => _.Replace(It.Is<TimeSeries>(series => series.EndTime != null)), Times.Once);
            _workItemRepository.Verify(_ => _.Replace(It.Is<WorkItem>(item => item.Status == WorkItemStatus.Highlighted)), Times.Once);
        }

        [Test]
        public async Task UpdateWorkItemMetaShouldReturnNullWhenWorkItemDoesNotExist()
        {
            var meta = new WorkItemDto { Id = "item_id", UserId = "user_id" };
            _workItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((WorkItem)null);

            Assert.IsNull(await _service.UpdateWorkItemMeta(meta).ConfigureAwait(false));
            _workItemRepository.Verify(_ => _.Replace(It.IsAny<WorkItem>()), Times.Never);
        }

        [Test]
        public async Task UpdateWorkItemMetaShouldReturnNullWhenFailedToUpdateWorkItem()
        {
            var meta = new WorkItemDto { Id = "item_id", UserId = "user_id" };
            _workItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WorkItem());
            _workItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync((WorkItem)null);

            Assert.IsNull(await _service.UpdateWorkItemMeta(meta).ConfigureAwait(false));
            _workItemRepository.Verify(_ => _.Replace(It.IsAny<WorkItem>()), Times.Once);
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

            _workItemRepository.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(item);
            _workItemRepository.Setup(_ => _.Replace(It.IsAny<WorkItem>())).ReturnsAsync(item);
            _workItemRepository.Setup(_ => _.GetWorkItemMeta(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(meta);

            Assert.IsNotNull(await _service.UpdateWorkItemMeta(meta).ConfigureAwait(false));

            _workItemRepository.Verify(_ => _.Replace(It.Is<WorkItem>
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
            _timeSeriesRepository.Setup(_ => _.GetDataSourceIdsByDateRange(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TimeSeriesType>())).ReturnsAsync(ids);
            _workItemRepository.Setup(_ => _.GetWorkItemProgressionByDateRange(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(progression);

            var result = await _service.GetWorkItemActivityBreakdownByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(9, result.Regular);
            Assert.AreEqual(3, result.Recurring);
            Assert.AreEqual(2, result.Interruption);
            Assert.AreEqual(0, result.Overlearning);
        }
    }
}
