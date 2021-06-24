using Core.Configurations;
using Core.Models.TimeSession;
using Microsoft.Extensions.Options;

namespace Service.Repositories
{
    public class TimeSeriesRepository : UserOwnedRecordRepository<TimeSeries>
    {
        public TimeSeriesRepository(IOptions<DatabaseConfiguration> configuration) : base(configuration, typeof(TimeSeries).Name) { }
    }
}
