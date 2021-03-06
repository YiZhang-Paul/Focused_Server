using Core.Dtos;
using Core.Enums;
using Core.Models.TimeSession;
using System;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IFocusSessionService
    {
        Task<FocusSessionDto> GetActiveFocusSessionMeta(string userId);
        Task<FocusSessionDto> GetStaleFocusSessionMeta(string userId);
        Task<bool> StartFocusSession(string userId, FocusSessionStartupOption option);
        Task<bool> StopFocusSession(string userId, string id);
        Task<bool> StartOverlearning(string userId, WorkItemStatus targetStatus);
        Task<bool> SwitchWorkItem(string userId, string id);
        Task<double> GetOverlearningHoursByDateRange(string userId, DateTime start, DateTime end);
    }
}
