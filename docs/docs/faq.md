# Frequently asked questions

1. I've create an event but no models are available to select in the Playground. What's wrong?
    Create an OpenAI Chat Completion models in Azure. From the Azure AI Proxy Admin portal, select the `Resources` tab and create a new resource of type `OpenAI-Chat` using the Azure OpenAI resource key, endpoint and deployment name. Once the resource is created, edit the event to add the resource.

2. If the first deployment with `azd up` fails, the postgresql server is already locked down to the virtual network. Running another deployment will fail because you cannot reach the server and thus database anymore. Go to the server and add the IP address that you are deploying from to the firewall rules. Example of the error messages below. Use the IP-adress from the warning 

``` shell
DeploymentScriptError: The provided script failed with multiple errors. First error:
Setting postgresql14 as the default version. Please refer to https://aka.ms/DeploymentScriptsTroubleshoot for more deployment script information.
DeploymentScriptError: * Setting postgresql14 as the default version
DeploymentScriptError: psql: error: connection to server at "gdex-openai-r2ictxhhwea2i-postgresql.database.azure.com" (4.225.117.213), port 5432 f is not valid for this server's tenant. Please acquire a new token for the tenant 43207ea0-2cda-4abb-9c84-efb8193dada8.
DeploymentScriptError: connection to server at "gdex-openai-r2ictxhhwea2i-postgresql.postgres.database.azure.com" (4.225.117.213), port 5432 failed: FATAL:
"4.225.117.213", user "gdex-openai-spn", database "postgres", no encryption
DeploymentScriptError: psql: error: connection to server at "gdex-openai-r2ictxhhwea2i-postgresql.postgres.database.azure.com" (4.225.117.213), port 5432 f is not valid for this server's tenant. Please acquire a new token for the tenant 43207ea0-2cda-4abb-9c84-efb8193dada8.
DeploymentScriptError: connection to server at "gdex-openai-r2ictxhhwea2i-postgresql.postgres.database.azure.com" (4.225.117.213), port 5432 failed: FATAL:
"4.225.117.213", user "gdex-openai-spn", database "aoai-proxy", no encryption
DeploymentScriptError: psql: error: connection to server at "gdex-openai-r2ictxhhwea2i-postgresql.database.azure.com" (4.225.117.213), port 5432 f is not valid for this server's tenant. Please acquire a new token for the tenant 43207ea0-2cda-4abb-9c84-efb8193dada8.
DeploymentScriptError: connection to server at "gdex-openai-r2ictxhhwea2i-postgresql.database.azure.com" (4.225.117.213), port 5432 failed: FATAL:
"4.225.117.213", user "gdex-openai-spn", database "aoai-proxy", no encryption
```

![Screenshot of the server firewall settings](/media/postgresql_firewall_setting .png)

> NOTE Do not forget to remove the rule after a succesful deployment!
