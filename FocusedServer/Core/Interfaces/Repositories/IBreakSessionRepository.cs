using Core.Interfaces.Repositories.RepositoryBase;
using Core.Models.TimeSession;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IBreakSessionRepository : ITimeRangeRecordRepository<BreakSession>
    {
        Task<BreakSession> GetUnfinishedBreakSession(string userId);
        Task<BreakSession> GetStaleBreakSession(string userId);
        Task<List<BreakSession>> GetBreakSessionByDateRange(string userId, DateTime start, DateTime end);
    }
}
