using Core.Dtos;
using System;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IFocusSessionService
    {
        Task<FocusSessionDto> GetActiveFocusSessionMeta(string userId);
        Task<double> GetOverlearningHoursByDateRange(string userId, DateTime start, DateTime end);
    }
}
