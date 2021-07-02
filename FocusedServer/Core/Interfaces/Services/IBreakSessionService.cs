using System;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IBreakSessionService
    {
        Task<double> GetBreakDurationByDateRange(string userId, DateTime start, DateTime end);
    }
}