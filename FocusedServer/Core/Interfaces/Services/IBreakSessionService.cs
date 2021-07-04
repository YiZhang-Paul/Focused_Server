using Core.Models.TimeSession;
using System;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IBreakSessionService
    {
        Task<double> GetBreakDurationByDateRange(string userId, DateTime start, DateTime end);
        Task<bool> StartBreakSession(string userId, BreakSessionStartupOption option);
        Task<bool> StopBreakSession(string userId, string id);
    }
}
