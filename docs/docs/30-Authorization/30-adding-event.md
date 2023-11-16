# Adding an event code

For now, you add an event via the Azure Storage Account `Storage browser`. The `Storage browser` is available in the Azure Portal, under the `Storage account` resource.

1. Select the Azure Storage Account resource, then select `Storage explorer (preview)` from the left-hand menu, then select `Tables` from the left-hand menu.
1. Next, select the `playgroundauthorization` table. 
2. Add an entry using the above schema, noting that the `PartitionKey` must be set to `playground` and the column names are case sensitive, and you must enter dates in [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601) format in UTC. 

## UTC Time

The [worldtimebuddy](https://www.worldtimebuddy.com) is a great time resource to convert your local time to UTC.

![](../media/world_time_buddy.png)

## Example

Here is an example

```text
PartitionKey: playground
RowKey: myevent2022
Active: true
MaxTokenCap: 1024
StartUTC: 2023-10-01T00:00:00Z
EndUTC: 2023-10-02T00:00:00Z
EventName: My Event 2023
ContactName: John Smith
ContactEmail: jsmith@example.com
```

## Event Code Cache

Event data, namely the `EventCode`, `StartUTC`, `EndUTC`, and `MaxTokenCap` are cached by the proxy service. The cache is refreshed every 10 minutes. Caching is implemented to improve performance by reducing the number of calls to the Azure Storage Account table. Because of caching, it may take up to 10 minutes for the changes to be reflected in the proxy service.
