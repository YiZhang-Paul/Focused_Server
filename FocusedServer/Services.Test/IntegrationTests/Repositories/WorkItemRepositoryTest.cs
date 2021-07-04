using Core.Enums;
using Core.Models.Generic;
using Core.Models.TimeSession;
using Core.Models.WorkItem;
using MongoDB.Bson;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Test.IntegrationTests.Repositories
{
    [TestFixture]
    public class WorkItemRepositoryTest
    {
        private TimeSeriesRepository TimeSeriesRepository { get; set; }
        private WorkItemRepository SubjectUnderTest { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            var configuration = ConfigurationUtility.GetDatabaseConfiguration();
            TimeSeriesRepository = new TimeSeriesRepository(configuration);
            SubjectUnderTest = new WorkItemRepository(configuration);
        }

        [Test]
        public async Task GetWorkItemsShouldReturnEmptyCollectionWhenNoWorkItemsFound()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { UserId = "user_id", Status = WorkItemStatus.Highlighted },
                new WorkItem { UserId = "user_id", Status = WorkItemStatus.Completed },
                new WorkItem { UserId = "user_id", Status = WorkItemStatus.Ongoing }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItems("user_id", WorkItemStatus.Idle).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetWorkItemsShouldReturnWorkItemsFound()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Highlighted },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Completed },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Highlighted },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Idle }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItems("user_id", WorkItemStatus.Highlighted).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(items[0].Id, result[0].Id);
            Assert.AreEqual(items[2].Id, result[1].Id);
        }

        [Test]
        public async Task GetWorkItemMetaShouldReturnNullWhenNoWorkItemMetaFound()
        {
            var items = new List<WorkItem>
            {
                new WorkItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    TimeInfo = new TimeInfo { Created = new DateTime(2021, 1, 1) }
                },
                new WorkItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    TimeInfo = new TimeInfo { Created = new DateTime(2021, 1, 1) }
                },
                new WorkItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    TimeInfo = new TimeInfo { Created = new DateTime(2021, 1, 1) }
                }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            Assert.IsNull(await SubjectUnderTest.GetWorkItemMeta("user_id", ObjectId.GenerateNewId().ToString()).ConfigureAwait(false));
        }

        [Test]
        public async Task GetWorkItemMetaShouldReturnWorkItemMetaFound()
        {
            var items = new List<WorkItem>
            {
                new WorkItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    TimeInfo = new TimeInfo { Created = new DateTime(2021, 1, 1) }
                },
                new WorkItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    TimeInfo = new TimeInfo { Created = new DateTime(2021, 1, 1) }
                },
                new WorkItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    TimeInfo = new TimeInfo { Created = new DateTime(2021, 1, 1) }
                }
            };

            var series = new List<TimeSeries>
            {
                new TimeSeries { UserId = "user_id", DataSourceId = items[1].Id, StartTime = new DateTime(2021, 1, 2, 15, 0, 0), EndTime = new DateTime(2021, 1, 2, 16, 0, 0) },
                new TimeSeries { UserId = "user_id", DataSourceId = items[0].Id, StartTime = new DateTime(2021, 1, 2, 17, 0, 0), EndTime = new DateTime(2021, 1, 2, 18, 0, 0) },
                new TimeSeries { UserId = "user_id", DataSourceId = items[1].Id, StartTime = new DateTime(2021, 1, 2, 19, 0, 0), EndTime = new DateTime(2021, 1, 2, 20, 0, 0) }
            };

            await TimeSeriesRepository.Add(series).ConfigureAwait(false);
            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMeta("user_id", items[1].Id).ConfigureAwait(false);

            Assert.AreEqual(items[1].Id, result.Id);
            Assert.AreEqual(2, result.ItemProgress.Current);
        }

        [Test]
        public async Task GetWorkItemMetasWithIdsShouldReturnEmptyCollectionWhenNoWorkItemMetasFound()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" }
            };

            var ids = new List<string>
            {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString()
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMetas("user_id", ids).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetWorkItemMetasWithIdsShouldReturnWorkItemMetasFound()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" }
            };

            var ids = new List<string> { items[0].Id, items[2].Id };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMetas("user_id", ids).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(items[0].Id, result[0].Id);
            Assert.AreEqual(items[2].Id, result[1].Id);
        }

        [Test]
        public async Task GetWorkItemMetasWithQueryShouldReturnEmptyCollectionWhenNoWorkItemsFound()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMetas("user_id", new WorkItemQuery { Skip = items.Count }).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetWorkItemMetasWithQueryShouldMatchPagination()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" }
            };

            var query = new WorkItemQuery
            {
                Skip = 2,
                Limit = 2
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMetas("user_id", query).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(items[2].Id, result[0].Id);
            Assert.AreEqual(items[3].Id, result[1].Id);
        }

        [Test]
        public async Task GetWorkItemMetasWithQueryShouldMatchSearchText()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Name = "name_1" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Name = "name_2" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Name = "name_3" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Name = "name_4" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Name = "name_11" }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMetas("user_id", new WorkItemQuery { SearchText = "name_1" }).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(items[0].Id, result[0].Id);
            Assert.AreEqual(items[4].Id, result[1].Id);
        }

        [Test]
        public async Task GetWorkItemMetasWithQueryShouldMatchType()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Type = WorkItemType.Regular },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Type = WorkItemType.Recurring },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Type = WorkItemType.Interruption },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Type = WorkItemType.Regular },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Type = WorkItemType.Recurring }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMetas("user_id", new WorkItemQuery { Type = WorkItemType.Regular }).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(items[0].Id, result[0].Id);
            Assert.AreEqual(items[3].Id, result[1].Id);
        }

        [Test]
        public async Task GetWorkItemMetasWithQueryShouldMatchHighlighted()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Highlighted },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Idle },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Completed },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Highlighted },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Ongoing }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMetas("user_id", new WorkItemQuery { IsHighlighted = true }).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(items[0].Id, result[0].Id);
            Assert.AreEqual(items[3].Id, result[1].Id);
        }

        [Test]
        public async Task GetWorkItemMetasWithQueryShouldMatchUnhighlighted()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Highlighted },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Idle },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Completed },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Highlighted },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Ongoing }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMetas("user_id", new WorkItemQuery { IsHighlighted = false }).ConfigureAwait(false);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(items[1].Id, result[0].Id);
            Assert.AreEqual(items[2].Id, result[1].Id);
            Assert.AreEqual(items[4].Id, result[2].Id);
        }

        [Test]
        public async Task GetWorkItemMetasWithQueryShouldMatchCompleted()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Highlighted },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Idle },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Completed },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Completed },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Ongoing }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMetas("user_id", new WorkItemQuery { IsCompleted = true }).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(items[2].Id, result[0].Id);
            Assert.AreEqual(items[3].Id, result[1].Id);
        }

        [Test]
        public async Task GetWorkItemMetasWithQueryShouldMatchIncompleted()
        {
            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Highlighted },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Idle },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Completed },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Completed },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", Status = WorkItemStatus.Ongoing }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemMetas("user_id", new WorkItemQuery { IsCompleted = false }).ConfigureAwait(false);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(items[0].Id, result[0].Id);
            Assert.AreEqual(items[1].Id, result[1].Id);
            Assert.AreEqual(items[4].Id, result[2].Id);
        }

        [Test]
        public async Task GetPastDueWorkItemsCountShouldReturnZeroWhenNoPastDueWorkItemsFound()
        {
            var start = DateTime.Now.AddHours(-5);
            var end = DateTime.Now.AddHours(-3.5);

            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-2) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-1) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(3) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-2.5) }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            Assert.AreEqual(0, await SubjectUnderTest.GetPastDueWorkItemsCount("user_id", start, end).ConfigureAwait(false));
        }

        [Test]
        public async Task GetPastDueWorkItemsCountShouldReturnTotalPastDueWorkItemsCount()
        {
            var start = DateTime.Now.AddHours(-5);
            var end = DateTime.Now.AddHours(-1.5);

            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-2) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-1) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(3) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-2.5) }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            Assert.AreEqual(2, await SubjectUnderTest.GetPastDueWorkItemsCount("user_id", start, end).ConfigureAwait(false));
        }

        [Test]
        public async Task GetLoomingWorkItemsCountShouldReturnZeroWhenNoLoomingWorkItemsFound()
        {
            var start = DateTime.Now.AddHours(-5);
            var end = DateTime.Now.AddHours(2);

            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-2) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-1) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(3) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-2.5) }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            Assert.AreEqual(0, await SubjectUnderTest.GetLoomingWorkItemsCount("user_id", start, end).ConfigureAwait(false));
        }

        [Test]
        public async Task GetLoomingWorkItemsCountShouldReturnTotalLoomingWorkItemsCount()
        {
            var start = DateTime.Now.AddHours(-5);
            var end = DateTime.Now.AddHours(3.5);

            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-2) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(1) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(3) },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DueDate = DateTime.Now.AddHours(-2.5) }
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            Assert.AreEqual(2, await SubjectUnderTest.GetLoomingWorkItemsCount("user_id", start, end).ConfigureAwait(false));
        }

        [Test]
        public async Task GetWorkItemProgressionByDateRangeShouldReturnEmptyCollectionWhenNoWorkItemsFound()
        {
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 2);

            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" }
            };

            var ids = new List<string>
            {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString()
            };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemProgressionByDateRange("user_id", ids, start, end).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetWorkItemProgressionByDateRangeShouldReturnWorkItemProgressions()
        {
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 2);

            var items = new List<WorkItem>
            {
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" },
                new WorkItem { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id" }
            };

            var ids = new List<string> { items[0].Id, items[2].Id };

            await SubjectUnderTest.Add(items).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetWorkItemProgressionByDateRange("user_id", ids, start, end).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(items[0].Id, result[0].Id);
            Assert.AreEqual(items[2].Id, result[1].Id);
        }

        [TearDown]
        public async Task TearDown()
        {
            await TimeSeriesRepository.DropCollection().ConfigureAwait(false);
            await SubjectUnderTest.DropCollection().ConfigureAwait(false);
        }
    }
}
