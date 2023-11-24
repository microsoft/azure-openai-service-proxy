// function role is to read audit events from the monitor queue and update the authorization table with the number of requests for each event code

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using QueueBatch;
using System.Collections.Generic;


namespace Advocacy.Function
{
    // note, this function is a singleton, meaning only one instance will run at a time
    // this is to ensure that the table doesn't get updated by multiple instances at the same time
    // this is to maintain atomicity given there is no transaction support in Azure Table Storage
    public class MonitorTrigger
    {
        [FunctionName("MonitorTrigger")]
        [Singleton(Mode = SingletonMode.Function)]
        public static async Task Run(
            [QueueBatchTrigger("monitor", Connection = "AzureProxyStorageAccount", UseFasterQueues = true)] IMessageBatch batch,
            [Table("authorization", Connection = "AzureProxyStorageAccount")] CloudTable table,
            ILogger log)
        {

            Dictionary<string, int> eventCodeCounts = new Dictionary<string, int>();

            foreach (var msg in batch.Messages)
            {
                // convert binary msg.Payload to string
                byte[] byteArray = msg.Payload.ToArray();
                string myQueueItem = System.Text.Encoding.UTF8.GetString(byteArray);

                dynamic data = JsonConvert.DeserializeObject(myQueueItem);
                string event_code = data?.event_code;

                if (eventCodeCounts.ContainsKey(event_code))
                {
                    eventCodeCounts[event_code]++;
                }
                else
                {
                    eventCodeCounts[event_code] = 1;
                }
            }

            // now loop through the dictionary and update the table for each event code in the dictionary
            foreach (var item in eventCodeCounts)
            {
                // Console.WriteLine($"Event Code: {item.Key}, Count: {item.Value}");

                try
                {
                    TableOperation retrieveOperation = TableOperation.Retrieve<DynamicTableEntity>("event", item.Key);

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
                            // increment the request_count property by item.value
                            requestCount += item.Value;
                            entity.Properties["RequestCount"] = new EntityProperty(requestCount);
                        }
                        else
                        {
                            // if the property doesn't exist, set it to item.value
                            entity.Properties.Add("RequestCount", new EntityProperty(item.Value));
                        }

                        // Write the updated entity to the table
                        TableOperation updateOperation = TableOperation.InsertOrReplace(entity);
                        await table.ExecuteAsync(updateOperation);
                    }
                    else
                    {
                        log.LogInformation($"Entity with PartitionKey: event, RowKey: {item.Key} not found");
                    }
                }
                catch (Exception e)
                {
                    // worst case the table didn't get updated, log the exception and move on
                    log.LogInformation($"Exception: {e.Message}");
                }

            }

            batch.MarkAllAsProcessed();
        }
    }
}
