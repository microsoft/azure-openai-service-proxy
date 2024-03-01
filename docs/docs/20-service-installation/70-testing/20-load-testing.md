# Load testing

There are several load testing tools available. The recommended tool is JMeter as the test plan can be deployed to Azure. The JMeter test plan is located in the `loadtest` folder. The test plan is configured to run 100 concurrent users, generating 4 requests per minute.

1. You'll need to update the URL in the `HTTP Request Defaults` element to point to your REST API endpoint.

    ![update url](../../media/jmeter_requests.png)

2. You'll need to update the `HTTP Header Manager` element to include your event code.

    ![update event code](../../media/jmeter-request-header.png)

### Example load test

![](../../media/example_perf_jmeter.png)
