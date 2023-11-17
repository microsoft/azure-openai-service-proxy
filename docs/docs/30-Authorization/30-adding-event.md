# Adding an event code

There are two ways to add an event to the system:

1. Using the Management API (preferred).
2. Using the Azure Storage Account `Storage browser`.

## Using the Management API

There is a Management API for adding events. The the path to the API is available from the url of the proxy service container. For example, if the proxy service is deployed to `https://myproxy.azurewebsites.net`, then the Management API is available at `https://myproxy.azurewebsites.net/v1/management/addevent`. This makes it easy to add events to the system using tools like [Power Automate](https://www.microsoft.com/power-platform/products/power-automate) or CLI tools like [curl](https://curl.se/).

The Management API is secured with the Managament ID token in the Azure OpenAI Proxy Storage Account `management` table.

The following is an example of a `curl` command to add an event to the system.

```shell
curl -X POST -H "Content-Type: application/json" -H "Authorization: Bearer YOUR_MANAGEMENT_ID" -d '{
    "event_name": ".NET OpenAI Hack",
    "start_utc" : "2023-11-16T00:00:00",
    "end_utc": "2023-12-16T00:00:00",
    "max_token_cap": 2048,
    "event_url": "http://www.example.com/event_name",
    "event_url_text": "Join the .NET OpenAI Hack",
    "organizer_name": "Ant Blogs",
    "organizer_email": "ablogs@example.com"
}' https://YOUR_OPENAI_PROXY_ENDPOINT/v1/management/addevent | jq
```



## Using the Azure Storage Account `Storage browser`

For now, you add an event via the Azure Storage Account `Storage browser`. The `Storage browser` is available in the Azure Portal, under the `Storage account` resource.

1. Select the Azure Storage Account resource, then select `Storage explorer (preview)` from the left-hand menu, then select `Tables` from the left-hand menu.
2. Next, select the `authorization` table. 
3. Add an entry using the above schema, noting that the `PartitionKey` must be set to `playground` and the column names are case sensitive, and you must enter dates in [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601) format in UTC. 

## UTC Time

The [worldtimebuddy](https://www.worldtimebuddy.com) is a great time resource to convert your local time to UTC.

![The image is an example of ](../media/world_time_buddy.png)

## Example

Here is an example

```text
PartitionKey: event
RowKey: myevent2022
Active: true
MaxTokenCap: 1024
StartUTC: 2023-10-01T00:00:00Z
EndUTC: 2023-10-02T00:00:00Z
EventName: My Event 2023
OrganizerName: John Smith
OrganizerEmail: jsmith@example.com
```

## Event Code Cache

Event data, namely the `EventCode`, `StartUTC`, `EndUTC`, and `MaxTokenCap` are cached by the proxy service. The cache is refreshed every 10 minutes. Caching is implemented to improve performance by reducing the number of calls to the Azure Storage Account table. Because of caching, it may take up to 10 minutes for the changes to be reflected in the proxy service.
