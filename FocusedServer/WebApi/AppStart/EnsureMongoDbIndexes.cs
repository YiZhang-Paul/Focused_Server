using Core.Models;
using Core.Models.TimeSession;
using Core.Models.WorkItem;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Service.Repositories.RepositoryBase;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.AppStart
{
    public class EnsureMongoDbIndexes : IHostedService
    {
        private DatabaseConnector<DatabaseEntry> Connector { get; set; }

        public EnsureMongoDbIndexes(DatabaseConnector<DatabaseEntry> connector)
        {
            Connector = connector;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await AddBreakSessionIndexes(cancellationToken).ConfigureAwait(false);
            await AddFocusSessionIndexes(cancellationToken).ConfigureAwait(false);
            await AddTimeSeriesIndexes(cancellationToken).ConfigureAwait(false);
            await AddWorkItemIndexes(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task AddBreakSessionIndexes(CancellationToken cancellationToken)
        {
            var builder = Builders<BreakSession>.IndexKeys;
            var collection = Connector.Connect<BreakSession>(typeof(BreakSession).Name);

            var indexes = new List<CreateIndexModel<BreakSession>>
            {
                new CreateIndexModel<BreakSession>(builder.Ascending(_ => _.UserId)),
                new CreateIndexModel<BreakSession>(builder.Combine(builder.Ascending(_ => _.UserId), builder.Ascending(_ => _.Id)))
            };

            await collection.Indexes.CreateManyAsync(indexes, cancellationToken).ConfigureAwait(false);
        }

        private async Task AddFocusSessionIndexes(CancellationToken cancellationToken)
        {
            var builder = Builders<FocusSession>.IndexKeys;
            var collection = Connector.Connect<FocusSession>(typeof(FocusSession).Name);

            var indexes = new List<CreateIndexModel<FocusSession>>
            {
                new CreateIndexModel<FocusSession>(builder.Ascending(_ => _.UserId)),
                new CreateIndexModel<FocusSession>(builder.Combine(builder.Ascending(_ => _.UserId), builder.Ascending(_ => _.Id)))
            };

            await collection.Indexes.CreateManyAsync(indexes, cancellationToken).ConfigureAwait(false);
        }

        private async Task AddTimeSeriesIndexes(CancellationToken cancellationToken)
        {
            var builder = Builders<TimeSeries>.IndexKeys;
            var collection = Connector.Connect<TimeSeries>(typeof(TimeSeries).Name);

            var indexes = new List<CreateIndexModel<TimeSeries>>
            {
                new CreateIndexModel<TimeSeries>(builder.Ascending(_ => _.UserId)),
                new CreateIndexModel<TimeSeries>(builder.Combine(builder.Ascending(_ => _.UserId), builder.Ascending(_ => _.DataSourceId)))
            };

            await collection.Indexes.CreateManyAsync(indexes, cancellationToken).ConfigureAwait(false);
        }

        private async Task AddWorkItemIndexes(CancellationToken cancellationToken)
        {
            var builder = Builders<WorkItem>.IndexKeys;
            var collection = Connector.Connect<WorkItem>(typeof(WorkItem).Name);

            var indexes = new List<CreateIndexModel<WorkItem>>
            {
                new CreateIndexModel<WorkItem>(builder.Ascending(_ => _.UserId)),
                new CreateIndexModel<WorkItem>(builder.Combine(builder.Ascending(_ => _.UserId), builder.Ascending(_ => _.Id))),
                new CreateIndexModel<WorkItem>(builder.Combine(builder.Ascending(_ => _.UserId), builder.Ascending(_ => _.Status)))
            };

            await collection.Indexes.CreateManyAsync(indexes, cancellationToken).ConfigureAwait(false);
        }
    }
}
