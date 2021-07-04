using Core.Dtos;
using Core.Models.TimeSession;
using System;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IFocusSessionService
    {
        Task<FocusSessionDto> GetActiveFocusSessionMeta(string userId);
        Task<bool> StartFocusSession(string userId, FocusSessionStartupOption option);
        Task<bool> StopFocusSession(string userId);
        Task<double> GetOverlearningHoursByDateRange(string userId, DateTime start, DateTime end);
    }
}
