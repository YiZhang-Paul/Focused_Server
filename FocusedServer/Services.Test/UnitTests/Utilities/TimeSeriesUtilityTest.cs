using Core.Models.TimeSession;
using NUnit.Framework;
using Service.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Test.UnitTests.Utilities
{
    [TestFixture]
    public class TimeSeriesUtilityTest
    {
        private List<TimeSeries> _series;

        [SetUp]
        public void Setup()
        {
            _series = new List<TimeSeries>
            {
                new TimeSeries
                {
                    StartTime = new DateTime(2021, 1, 5, 5, 15, 0),
                    EndTime = new DateTime(2021, 1, 5, 12, 15, 0)
                },
                new TimeSeries
                {
                    StartTime = new DateTime(2021, 1, 5, 12, 15, 0),
                    EndTime = new DateTime(2021, 1, 5, 12, 45, 0)
                },
                new TimeSeries
                {
                    StartTime = new DateTime(2021, 1, 6, 12, 15, 0),
                    EndTime = new DateTime(2021, 1, 6, 13, 15, 0)
                },
                new TimeSeries
                {
                    StartTime = new DateTime(2021, 1, 7, 12, 15, 0),
                    EndTime = new DateTime(2021, 1, 7, 12, 45, 0)
                }
            };
        }

        [Test]
        public void GetTotalTimeShouldReturnZeroWhenTimeSeriesIsEmpty()
        {
            Assert.AreEqual(0, TimeSeriesUtility.GetTotalTime(new List<TimeSeries>(), new DateTime(2020, 1, 1), DateTime.Now));
        }

        [Test]
        public void GetTotalTimeShouldReturnTimeWithinTimeRange()
        {
            var start = new DateTime(2021, 1, 5, 12, 15, 0);
            var end = new DateTime(2021, 1, 7, 12, 15, 0);

            Assert.AreEqual(1.5, TimeSeriesUtility.GetTotalTime(_series, start, end));
        }

        [Test]
        public void GetTotalTimeShouldIncludeOverlappingTime()
        {
            var start = new DateTime(2021, 1, 5, 10, 15, 0);
            var end = new DateTime(2021, 1, 7, 12, 30, 0);

            Assert.AreEqual(3.75, TimeSeriesUtility.GetTotalTime(_series, start, end));
        }

        [Test]
        public void GetTotalTimeShouldIncludeOpenTimeRange()
        {
            _series.Last().EndTime = null;
            var start = DateTime.Now.AddHours(-3);
            var end = DateTime.Now;

            Assert.AreEqual(3, Math.Round(TimeSeriesUtility.GetTotalTime(_series, start, end), 2));
        }
    }
}
