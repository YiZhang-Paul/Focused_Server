using Core.Models;
using Core.Models.TimeSession;
using Core.Models.WorkItem;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Service.Repositories.RepositoryBase;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.AppStart
{
    public class MongoDbIndexesSetup : IHostedService
    {
        private DatabaseConnector<DatabaseEntry> Connector { get; set; }

        public MongoDbIndexesSetup(DatabaseConnector<DatabaseEntry> connector)
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
            var index = new CreateIndexModel<BreakSession>(builder.Ascending(_ => _.UserId));
            await collection.Indexes.CreateOneAsync(index, null, cancellationToken).ConfigureAwait(false);
        }

        private async Task AddFocusSessionIndexes(CancellationToken cancellationToken)
        {
            var builder = Builders<FocusSession>.IndexKeys;
            var collection = Connector.Connect<FocusSession>(typeof(FocusSession).Name);
            var index = new CreateIndexModel<FocusSession>(builder.Ascending(_ => _.UserId));
            await collection.Indexes.CreateOneAsync(index, null, cancellationToken).ConfigureAwait(false);
        }

        private async Task AddTimeSeriesIndexes(CancellationToken cancellationToken)
        {
            var builder = Builders<TimeSeries>.IndexKeys;
            var collection = Connector.Connect<TimeSeries>(typeof(TimeSeries).Name);
            var index = new CreateIndexModel<TimeSeries>(builder.Combine(builder.Ascending(_ => _.UserId), builder.Ascending(_ => _.DataSourceId)));
            await collection.Indexes.CreateOneAsync(index, null, cancellationToken).ConfigureAwait(false);
        }

        private async Task AddWorkItemIndexes(CancellationToken cancellationToken)
        {
            var builder = Builders<WorkItem>.IndexKeys;
            var collection = Connector.Connect<WorkItem>(typeof(WorkItem).Name);
            var index = new CreateIndexModel<WorkItem>(builder.Combine(builder.Ascending(_ => _.UserId), builder.Ascending(_ => _.Status)));
            await collection.Indexes.CreateOneAsync(index, null, cancellationToken).ConfigureAwait(false);
        }
    }
}
