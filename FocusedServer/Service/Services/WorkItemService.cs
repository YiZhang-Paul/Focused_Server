using Core.Dtos;
using Core.Enums;
using Core.Models.TimeSession;
using Core.Models.WorkItem;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class WorkItemService
    {
        private WorkItemRepository WorkItemRepository { get; set; }
        private TimeSeriesRepository TimeSeriesRepository { get; set; }
        private FocusSessionRepository FocusSessionRepository { get; set; }

        public WorkItemService
        (
            WorkItemRepository workItemRepository,
            TimeSeriesRepository timeSeriesRepository,
            FocusSessionRepository focusSessionRepository
        )
        {
            WorkItemRepository = workItemRepository;
            TimeSeriesRepository = timeSeriesRepository;
            FocusSessionRepository = focusSessionRepository;
        }

        public async Task<List<string>> GetTrackedWorkItemIdsByDateRange(string userId, DateTime start, DateTime end)
        {
            var series = await TimeSeriesRepository.GetTimeSeriesByDateRange(userId, start, end, TimeSeriesType.WorkItem);

            return series.Select(_ => _.DataSourceId).Distinct().ToList();
        }

        public async Task<string> CreateWorkItem(WorkItemDto item)
        {
            try
            {
                var workItem = new WorkItem
                {
                    Name = item.Name.Trim(),
                    Type = item.Type,
                    Priority = item.Priority,
                    EstimatedHours = item.ItemProgress.Target
                };

                return await WorkItemRepository.Add(workItem).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        public async Task<WorkItem> GetWorkItem(string userId, string id)
        {
            return await WorkItemRepository.Get(userId, id).ConfigureAwait(false);
        }

        public async Task<WorkItem> UpdateWorkItem(WorkItem item)
        {
            item.TimeInfo.LastModified = DateTime.UtcNow;

            return await WorkItemRepository.Replace(item).ConfigureAwait(false);
        }

        public async Task<bool> DeleteWorkItem(string userId, string id)
        {
            return await WorkItemRepository.Delete(userId, id).ConfigureAwait(false);
        }

        public async Task<WorkItemDto> UpdateWorkItemMeta(WorkItemDto item)
        {
            var workItem = await WorkItemRepository.Get(item.UserId, item.Id).ConfigureAwait(false);

            if (workItem == null)
            {
                return null;
            }

            workItem.Name = item.Name;
            workItem.Type = item.Type;
            workItem.Priority = item.Priority;
            workItem.Status = item.Status;
            workItem.EstimatedHours = item.ItemProgress.Target;
            workItem.TimeInfo.LastModified = DateTime.UtcNow;

            if (item.Status == WorkItemStatus.Ongoing && !await SyncOngoingStatus(item.UserId, item.Id).ConfigureAwait(false))
            {
                return null;
            }

            if (await WorkItemRepository.Replace(workItem).ConfigureAwait(false) == null)
            {
                return null;
            }

            return await WorkItemRepository.GetWorkItemMeta(item.UserId, item.Id).ConfigureAwait(false);
        }

        public async Task<WorkItemDto> GetWorkItemMeta(string userId, string id)
        {
            return await WorkItemRepository.GetWorkItemMeta(userId, id).ConfigureAwait(false);
        }

        public async Task<List<WorkItemDto>> GetWorkItemMetas(string userId, WorkItemQuery query)
        {
            return await WorkItemRepository.GetWorkItemMetas(userId, query).ConfigureAwait(false);
        }

        private async Task<bool> SyncOngoingStatus(string userId, string workItemId)
        {
            var source = WorkItemStatus.Ongoing;
            var target = WorkItemStatus.Highlighted;
            var session = await FocusSessionRepository.GetActiveFocusSession(userId).ConfigureAwait(false);

            if (session == null || !await WorkItemRepository.UpdateWorkItemsStatus(userId, source, target).ConfigureAwait(false))
            {
                return false;
            }

            if (session.WorkItemIds.Contains(workItemId))
            {
                return true;
            }

            session.WorkItemIds.Add(workItemId);

            return await FocusSessionRepository.Replace(session).ConfigureAwait(false) != null;
        }
    }
}
