using Core.Dtos;
using Core.Enums;
using Core.Models.WorkItem;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Services
{
    public class WorkItemService
    {
        private WorkItemRepository WorkItemRepository { get; set; }
        private FocusSessionRepository FocusSessionRepository { get; set; }
        private UserProfileRepository UserProfileRepository { get; set; }

        public WorkItemService
        (
            WorkItemRepository workItemRepository,
            FocusSessionRepository focusSessionRepository,
            UserProfileRepository userProfileRepository
        )
        {
            WorkItemRepository = workItemRepository;
            FocusSessionRepository = focusSessionRepository;
            UserProfileRepository = userProfileRepository;
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

        public async Task<WorkItem> GetWorkItem(string id)
        {
            return await WorkItemRepository.Get(id).ConfigureAwait(false);
        }

        public async Task<WorkItem> UpdateWorkItem(WorkItem item)
        {
            item.TimeInfo.LastModified = DateTime.UtcNow;

            return await WorkItemRepository.Replace(item).ConfigureAwait(false);
        }

        public async Task<bool> DeleteWorkItem(string id)
        {
            return await WorkItemRepository.Delete(id).ConfigureAwait(false);
        }

        public async Task<WorkItemDto> UpdateWorkItemMeta(WorkItemDto item, string userId)
        {
            var workItem = await WorkItemRepository.Get(item.Id).ConfigureAwait(false);

            if (workItem == null)
            {
                return null;
            }

            workItem.Name = item.Name;
            workItem.Type = item.Type;
            workItem.Priority = item.Priority;
            workItem.Status = item.Status;
            workItem.EstimatedHours = item.ItemProgress.Target;

            if (item.Status == WorkItemStatus.Ongoing && !await SyncOngoingStatus(userId, item.Id).ConfigureAwait(false))
            {
                return null;
            }

            if (await WorkItemRepository.Replace(workItem).ConfigureAwait(false) == null)
            {
                return null;
            }

            return await WorkItemRepository.GetWorkItemMeta(item.Id).ConfigureAwait(false);
        }

        public async Task<WorkItemDto> GetWorkItemMeta(string id)
        {
            return await WorkItemRepository.GetWorkItemMeta(id).ConfigureAwait(false);
        }

        public async Task<List<WorkItemDto>> GetWorkItemMetas(string userId, WorkItemQuery query)
        {
            return await WorkItemRepository.GetWorkItemMetas(userId, query).ConfigureAwait(false);
        }

        private async Task<bool> SyncOngoingStatus(string userId, string workItemId)
        {
            var source = WorkItemStatus.Ongoing;
            var target = WorkItemStatus.Highlighted;
            var user = await UserProfileRepository.Get(userId).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(user?.FocusSessionId) || await WorkItemRepository.UpdateWorkItemsStatus(user.Id, source, target).ConfigureAwait(false))
            {
                return false;
            }

            var session = await FocusSessionRepository.Get(user.FocusSessionId).ConfigureAwait(false);

            if (session == null || session.WorkItemIds.Contains(workItemId))
            {
                return session != null;
            }

            session.WorkItemIds.Add(workItemId);

            return await FocusSessionRepository.Replace(session).ConfigureAwait(false) != null;
        }
    }
}
