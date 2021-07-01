using Core.Enums;
using Core.Interfaces.Repositories.RepositoryBase;
using Core.Models.TimeSession;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface ITimeSeriesRepository : ITimeRangeRecordRepository<TimeSeries>
    {
        Task<List<string>> GetDataSourceIdsByDateRange(string userId, DateTime start, DateTime end, TimeSeriesType type = TimeSeriesType.All);
        Task<List<TimeSeries>> GetTimeSeriesByDataSource(string userId, string dataSourceId);
        Task<List<TimeSeries>> GetTimeSeriesByDateRange(string userId, DateTime start, DateTime end, TimeSeriesType type = TimeSeriesType.All);
    }
}
