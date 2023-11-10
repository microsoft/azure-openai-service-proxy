# Scaling the Proxy Service

The proxy service is stateless and scales vertically and horizontally. By default, the proxy service is configured to scale up to 10 replicas. The proxy service is configured to scale up to 10 replicas. The number of replicas can be changed from the Azure Portal or from the az cli. For example, to scale to 30 replicas using the az cli, change the:

```shell
az containerapp update -n $APP_NAME -g $RESOURCE_GROUP --subscription $SUBSCRIPTION_ID --replica 30
```
