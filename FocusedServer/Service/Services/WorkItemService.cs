using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.TimeSession;
using Core.Models.WorkItem;
using Service.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class WorkItemService : IWorkItemService
    {
        private IWorkItemRepository WorkItemRepository { get; set; }
        private ITimeSeriesRepository TimeSeriesRepository { get; set; }

        public WorkItemService(IWorkItemRepository workItemRepository, ITimeSeriesRepository timeSeriesRepository)
        {
            WorkItemRepository = workItemRepository;
            TimeSeriesRepository = timeSeriesRepository;
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
            item.TimeInfo.LastModified = DateTime.Now;

            return await WorkItemRepository.Replace(item).ConfigureAwait(false);
        }

        public async Task<bool> StartWorkItem(string userId, string id)
        {
            var item = await GetWorkItem(userId, id).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            var series = new TimeSeries
            {
                UserId = userId,
                StartTime = DateTime.Now,
                Type = TimeSeriesType.WorkItem,
                DataSourceId = item.Id
            };

            var seriesId = await TimeSeriesRepository.Add(series).ConfigureAwait(false);
            item.Status = WorkItemStatus.Ongoing;

            return !string.IsNullOrWhiteSpace(seriesId) && await UpdateWorkItem(item).ConfigureAwait(false) != null;
        }

        public async Task<bool> StopWorkItem(string userId, WorkItemStatus targetStatus = WorkItemStatus.Highlighted)
        {
            var items = await WorkItemRepository.GetWorkItems(userId, WorkItemStatus.Ongoing).ConfigureAwait(false);

            if (!items.Any())
            {
                return true;
            }

            var item = items.First();
            var series = (await TimeSeriesRepository.GetTimeSeriesByDataSource(userId, item.Id).ConfigureAwait(false)).LastOrDefault();

            if (series == null || series.EndTime != null)
            {
                return false;
            }

            series.EndTime = DateTime.Now;
            item.Status = targetStatus;

            if (targetStatus == WorkItemStatus.Completed)
            {
                WorkItemUtility.AddCompletionRecord(item);
            }

            return await TimeSeriesRepository.Replace(series).ConfigureAwait(false) != null && await UpdateWorkItem(item).ConfigureAwait(false) != null;
        }

        public async Task<WorkItemDto> UpdateWorkItemMeta(WorkItemDto item)
        {
            var workItem = await GetWorkItem(item.UserId, item.Id).ConfigureAwait(false);

            if (workItem == null)
            {
                return null;
            }

            if (workItem.Status != item.Status && item.Status == WorkItemStatus.Completed)
            {
                WorkItemUtility.AddCompletionRecord(workItem);
            }

            workItem.Name = item.Name;
            workItem.Type = item.Type;
            workItem.Priority = item.Priority;
            workItem.Status = item.Status;
            workItem.EstimatedHours = item.ItemProgress.Target;

            if (await UpdateWorkItem(workItem).ConfigureAwait(false) == null)
            {
                return null;
            }

            return await WorkItemRepository.GetWorkItemMeta(item.UserId, item.Id).ConfigureAwait(false);
        }

        public async Task<ActivityBreakdownDto> GetWorkItemActivityBreakdownByDateRange(string userId, DateTime start, DateTime end)
        {
            var progress = await GetWorkItemCurrentProgressionByDateRange(userId, start, end).ConfigureAwait(false);

            return new ActivityBreakdownDto
            {
                Regular = progress.Sum(_ => _.Type == WorkItemType.Regular ? _.Progress.Current : 0),
                Recurring = progress.Sum(_ => _.Type == WorkItemType.Recurring ? _.Progress.Current : 0),
                Interruption = progress.Sum(_ => _.Type == WorkItemType.Interruption ? _.Progress.Current : 0),
            };
        }

        public async Task<List<WorkItemProgressionDto>> GetWorkItemCurrentProgressionByDateRange(string userId, DateTime start, DateTime end)
        {
            return await GetWorkItemProgressionByDateRange(userId, start, end, false).ConfigureAwait(false);
        }

        public async Task<List<WorkItemProgressionDto>> GetWorkItemOverallProgressionByDateRange(string userId, DateTime start, DateTime end)
        {
            return await GetWorkItemProgressionByDateRange(userId, start, end, true).ConfigureAwait(false);
        }

        private async Task<List<WorkItemProgressionDto>> GetWorkItemProgressionByDateRange(string userId, DateTime start, DateTime end, bool isOverall)
        {
            var startDate = isOverall ? new DateTime(1970, 1, 1) : start;
            var ids = await TimeSeriesRepository.GetDataSourceIdsByDateRange(userId, start, end, TimeSeriesType.WorkItem).ConfigureAwait(false);

            return await WorkItemRepository.GetWorkItemProgressionByDateRange(userId, ids, startDate, end).ConfigureAwait(false);
        }
    }
}
