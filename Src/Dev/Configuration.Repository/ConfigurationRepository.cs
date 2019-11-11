using Khooversoft.Toolbox.Standard;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Configuration.Repository
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private const string _tableName = "ConfigurationKeyValueTable";
        private readonly string _connectionString;
        private readonly CloudStorageAccount _account;
        private CloudTableClient _client;
        private CloudTable _table;

        public ConfigurationRepository(string connectionString)
        {
            connectionString.Verify(nameof(connectionString)).IsNotNull();

            _connectionString = connectionString;
            _account = CloudStorageAccount.Parse(_connectionString);
        }

        public async Task Open(IWorkContext context)
        {
            _client = _account.CreateCloudTableClient();
            _table = _client.GetTableReference(_tableName);
            await _table.CreateIfNotExistsAsync();
        }

        public async Task Set(IWorkContext context, string key, string value)
        {
            key.Verify(nameof(key)).IsNotEmpty();
            value.Verify(nameof(value)).IsNotEmpty();

            var entity = new ConfigurationEntity(key, value);

            TableOperation operation = TableOperation.InsertOrReplace(entity);
            await _table.ExecuteAsync(operation);
        }

        public async Task Delete(IWorkContext context, string key)
        {
            key.Verify(nameof(key)).IsNotEmpty();

            var entity = new ConfigurationEntity(key);
            entity.ETag = "*";

            TableOperation operation = TableOperation.Delete(entity);
            await _table.ExecuteAsync(operation);
        }

        public async Task<KeyValuePair<string, string>?> Get(IWorkContext context, string key)
        {
            key.Verify(nameof(key)).IsNotEmpty();

            var entity = new ConfigurationEntity(key);

            TableOperation operation = TableOperation.Retrieve<ConfigurationEntity>(entity.PartitionKey, entity.RowKey);
            TableResult tableResult = await _table.ExecuteAsync(operation);

            ConfigurationEntity result = tableResult.Result as ConfigurationEntity;
            if (result == null) return default;

            return new KeyValuePair<string, string>(result.Key, result.Value);
        }

        public async Task<IReadOnlyList<KeyValuePair<string, string>>> List(IWorkContext context, string nameSpace)
        {
            var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, nameSpace);
            var query = new TableQuery<ConfigurationEntity>().Where(condition);

            var list = new List<ConfigurationEntity>();
            TableContinuationToken token = null;

            do
            {
                TableQuerySegment<ConfigurationEntity> segment = await _table.ExecuteQuerySegmentedAsync(query, token);

                list.AddRange(segment.Results);
                token = segment.ContinuationToken;
            } while (token != null);

            return list
                .Select(x => new KeyValuePair<string, string>(x.Key, x.Value))
                .ToList(); ;
        }

        public async Task DeleteAllRecords(IWorkContext context)
        {
            // query all rows
            var query = new TableQuery<ConfigurationEntity>();
            var result = await _table.ExecuteQuerySegmentedAsync(query, null);
            if (result.Results.Count == 0) return;

            // Create the batch operation.
            TableBatchOperation batchDeleteOperation = new TableBatchOperation();

            foreach (var row in result)
            {
                batchDeleteOperation.Delete(row);
            }

            // Execute the batch operation.
            await _table.ExecuteBatchAsync(batchDeleteOperation);
        }
    }
}
