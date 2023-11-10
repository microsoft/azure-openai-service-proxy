# Event authorization

Access to the REST endpoint is controlled by an event code. The REST endpoint is accessible when the current UTC is between the StartUTC and the EndUTC times and the event is active. The event code is passed in the `openai-event-code` header. If the event code is not passed, or the event code is not active, or the current UTC is not between the StartUTC and the EndUTC times, the REST endpoint will return a `401` unauthorized error.

Event code details are stored in an Azure Storage account table named `playgroundauthorization`. This table is created when the app is deployed and starts. The table has the following schema:

| Property     | Type     | Description   |
| ------------ | -------- | ------------- |
| PartitionKey | string   | Must be 'playground' |
| RowKey       | string   | The event code must be between 6 and 20 characters long. For example myevent2022. Note, you can't use the following characters in the event name: 'The forward slash (/), backslash (\\), number sign (#), and question mark (?) characters' as they aren't allowed for an Azure Storage Table RowKey property name. |
| Active       | boolean  | Is the event active, true, or false |
| MaxTokenCap  | int      | The maximum number of tokens per request. This overrides the user set Max Token value for load balancing  |
| StartUTC     | datetime | The start date and time of the event in UTC |
| EndUTC       | datetime | The end date and time of the event in UTC   |
| EventName    | string   | The name of the event                       |
| ContactName  | string   | The name of the event contact               |
| ContactEmail | string   | The email address of the event contact      |