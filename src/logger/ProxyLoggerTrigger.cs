
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Advocacy.Function
{
    public class ProxyLoggerTrigger
    {
        [FunctionName("ProxyLoggerTrigger")]
        [Singleton(Mode = SingletonMode.Function)]
        public static async Task Run(
            [QueueTrigger("monitor", Connection = "AzureProxyStorageAccount")] string myQueueItem,
            [Table("authorization", Connection = "AzureProxyStorageAccount")] CloudTable table,
            ILogger log)
        {

            // Deserialize the queue item into a dynamic object
            dynamic data = JsonConvert.DeserializeObject(myQueueItem);

            // Get the event_code from the data
            string event_code = data?.event_code;

            // Define the retrieve operation
            TableOperation retrieveOperation = TableOperation.Retrieve<DynamicTableEntity>("event", event_code);

            // Execute the operation
            TableResult result = await table.ExecuteAsync(retrieveOperation);

            // Get the entity from the result
            DynamicTableEntity entity = result.Result as DynamicTableEntity;

            if (entity != null)
            {
                // Read the request_count property
                if (entity.Properties.ContainsKey("RequestCount"))
                {
                    EntityProperty requestCountProperty = entity.Properties["RequestCount"];
                    int requestCount = requestCountProperty.Int32Value.GetValueOrDefault();
                    // increment the request_count property
                    requestCount++;
                    entity.Properties["RequestCount"] = new EntityProperty(requestCount);
                }
                else
                {
                    // if the property doesn't exist, set it to 1
                    entity.Properties.Add("RequestCount", new EntityProperty(1));
                }

                // Write the updated entity to the table
                TableOperation updateOperation = TableOperation.InsertOrReplace(entity);
                await table.ExecuteAsync(updateOperation);
            }
            else
            {
                log.LogInformation($"Entity with PartitionKey: event, RowKey: {event_code} not found");
            }
        }
    }
}
