using Core.Enums;
using Core.Models.TimeSession;
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
    public class TimeSeriesRepositoryTest
    {
        private TimeSeriesRepository SubjectUnderTest { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            SubjectUnderTest = new TimeSeriesRepository(ConfigurationUtility.GetDatabaseConfiguration());
        }

        [Test]
        public async Task GetDataSourceIdsByDateRangeShouldReturnEmptyCollectionWhenNoDataSourceIdFound()
        {
            var start = new DateTime(2021, 1, 3);
            var end = new DateTime(2021, 1, 5);

            var series = new List<TimeSeries>
            {
                new TimeSeries { UserId = "user_id", DataSourceId = "id_1", StartTime = new DateTime(2021, 1, 2, 10, 0, 0), EndTime = new DateTime(2021, 1, 2, 11, 0, 0) },
                new TimeSeries { UserId = "user_id", DataSourceId = "id_2", StartTime = new DateTime(2021, 1, 2, 12, 0, 0), EndTime = new DateTime(2021, 1, 2, 13, 0, 0) },
                new TimeSeries { UserId = "user_id", DataSourceId = "id_3", StartTime = new DateTime(2021, 1, 2, 14, 0, 0), EndTime = new DateTime(2021, 1, 2, 15, 0, 0) }
            };

            await SubjectUnderTest.Add(series).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetDataSourceIdsByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetDataSourceIdsByDateRangeShouldReturnDataSourceIds()
        {
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 3);

            var series = new List<TimeSeries>
            {
                new TimeSeries { UserId = "user_id", DataSourceId = "id_1", StartTime = new DateTime(2021, 1, 2, 10, 0, 0), EndTime = new DateTime(2021, 1, 2, 11, 0, 0) },
                new TimeSeries { UserId = "user_id", DataSourceId = "id_2", StartTime = new DateTime(2021, 1, 2, 12, 0, 0), EndTime = new DateTime(2021, 1, 2, 13, 0, 0) },
                new TimeSeries { UserId = "user_id", DataSourceId = "id_2", StartTime = new DateTime(2021, 1, 2, 14, 0, 0), EndTime = new DateTime(2021, 1, 2, 15, 0, 0) },
                new TimeSeries { UserId = "user_id", DataSourceId = "id_3", StartTime = new DateTime(2021, 1, 2, 16, 0, 0), EndTime = new DateTime(2021, 1, 2, 17, 0, 0) }
            };

            await SubjectUnderTest.Add(series).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetDataSourceIdsByDateRange("user_id", start, end).ConfigureAwait(false);

            CollectionAssert.AreEqual(new List<string> { "id_1", "id_2", "id_3" }, result);
        }

        [Test]
        public async Task GetTimeSeriesByDataSourceShouldReturnEmptyCollectionWhenNoTimeSeriesFound()
        {
            var series = new List<TimeSeries>
            {
                new TimeSeries { UserId = "user_id", DataSourceId = "id_1" },
                new TimeSeries { UserId = "user_id", DataSourceId = "id_2" },
                new TimeSeries { UserId = "user_id", DataSourceId = "id_3" }
            };

            await SubjectUnderTest.Add(series).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetTimeSeriesByDataSource("user_id", "id_4").ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetTimeSeriesByDataSourceShouldReturnTimeSeriesFound()
        {
            var series = new List<TimeSeries>
            {
                new TimeSeries { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DataSourceId = "id_2" },
                new TimeSeries { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DataSourceId = "id_3" },
                new TimeSeries { Id = ObjectId.GenerateNewId().ToString(), UserId = "user_id", DataSourceId = "id_2" }
            };

            await SubjectUnderTest.Add(series).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetTimeSeriesByDataSource("user_id", "id_2").ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(series[0].Id, result[0].Id);
            Assert.AreEqual(series[2].Id, result[1].Id);
        }

        [Test]
        public async Task GetTimeSeriesByDateRangeShouldReturnEmptyCollectionWhenNoTimeSeriesFound()
        {
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 2);

            var series = new List<TimeSeries>
            {
                new TimeSeries { UserId = "user_id", StartTime = new DateTime(2021, 1, 2, 10, 0, 0), EndTime = new DateTime(2021, 1, 2, 11, 0, 0), Type = TimeSeriesType.WorkItem },
                new TimeSeries { UserId = "user_id", StartTime = new DateTime(2021, 1, 2, 12, 0, 0), EndTime = new DateTime(2021, 1, 2, 13, 0, 0), Type = TimeSeriesType.Session },
                new TimeSeries { UserId = "user_id", StartTime = new DateTime(2021, 1, 2, 14, 0, 0), EndTime = new DateTime(2021, 1, 2, 15, 0, 0), Type = TimeSeriesType.WorkItem }
            };

            await SubjectUnderTest.Add(series).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetTimeSeriesByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetTimeSeriesByDateRangeShouldReturnTimeSeriesFound()
        {
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 2, 13, 0, 0);

            var series = new List<TimeSeries>
            {
                new TimeSeries
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    StartTime = new DateTime(2021, 1, 2, 10, 0, 0),
                    EndTime = new DateTime(2021, 1, 2, 11, 0, 0),
                    Type = TimeSeriesType.WorkItem
                },
                new TimeSeries
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    StartTime = new DateTime(2021, 1, 2, 12, 0, 0),
                    EndTime = new DateTime(2021, 1, 2, 13, 0, 0),
                    Type = TimeSeriesType.Session
                },
                new TimeSeries
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    StartTime = new DateTime(2021, 1, 2, 14, 0, 0),
                    EndTime = new DateTime(2021, 1, 2, 15, 0, 0),
                    Type = TimeSeriesType.WorkItem
                }
            };

            await SubjectUnderTest.Add(series).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetTimeSeriesByDateRange("user_id", start, end).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(series[0].Id, result[0].Id);
            Assert.AreEqual(series[1].Id, result[1].Id);
        }

        [Test]
        public async Task GetTimeSeriesByDateRangeShouldReturnTimeSeriesByType()
        {
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 2, 13, 0, 0);

            var series = new List<TimeSeries>
            {
                new TimeSeries
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    StartTime = new DateTime(2021, 1, 2, 10, 0, 0),
                    EndTime = new DateTime(2021, 1, 2, 11, 0, 0),
                    Type = TimeSeriesType.WorkItem
                },
                new TimeSeries
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    StartTime = new DateTime(2021, 1, 2, 12, 0, 0),
                    EndTime = new DateTime(2021, 1, 2, 13, 0, 0),
                    Type = TimeSeriesType.Session
                },
                new TimeSeries
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = "user_id",
                    StartTime = new DateTime(2021, 1, 2, 14, 0, 0),
                    EndTime = new DateTime(2021, 1, 2, 15, 0, 0),
                    Type = TimeSeriesType.WorkItem
                }
            };

            await SubjectUnderTest.Add(series).ConfigureAwait(false);

            var result = await SubjectUnderTest.GetTimeSeriesByDateRange("user_id", start, end, TimeSeriesType.Session).ConfigureAwait(false);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(series[1].Id, result[0].Id);
        }

        [TearDown]
        public async Task TearDown()
        {
            await SubjectUnderTest.DropCollection().ConfigureAwait(false);
        }
    }
}
