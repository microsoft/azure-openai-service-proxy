# Event authorization

Access to the proxy service endpoint is controlled by an event code.

1. The proxy service is accessible when the current UTC is between the StartUTC and the EndUTC times and the event is active.

Event code details are stored in an Azure Storage account table named `authorization`. This table is created when the app is deployed and starts. The table has the following schema:

| Property     | Type     | Description   |
| ------------ | -------- | ------------- |
| PartitionKey | string   | Must be 'event' |
| RowKey       | string   | The event code must be between 6 and 40 characters long. For example myevent2022. Note, you can't use the following characters in the event name: 'The forward slash (/), backslash (\\), number sign (#), and question mark (?) characters' as they aren't allowed for an Azure Storage Table RowKey property name. |
| Active       | boolean  | Is the event active, true, or false |
| MaxTokenCap  | int      | The maximum number of tokens per request. This overrides the user set Max Token value and allows for some degree of load balance  |
| StartUTC     | datetime | The start date and time of the event in UTC |
| EndUTC       | datetime | The end date and time of the event in UTC   |
| EventName    | string   | The name of the event                       |
| OrganizerName  | string   | The name of the event contact               |
| OrganizerEmail | string   | The email address of the event contact      |