#!/bin/bash

echo "Loading azd .env file from current environment"

# Use the `get-values` azd command to retrieve environment variables from the `.env` file
while IFS='=' read -r key value; do
    value=$(echo "$value" | sed 's/^"//' | sed 's/"$//')
    export "$key=$value"
done <<EOF
$(azd env get-values)
EOF

signin_path='/signin-oidc'

app_name=${1:-"aoai-proxy-admin"}
app_registrations=$(az ad app list --filter "displayname eq '$app_name'" -o json)
app_count=$(echo $app_registrations | jq '. | length')

if [ $app_count -eq 0 ]; then
    echo "Creating app registration for $app_name"
    app_id=$(az ad app create --display-name $app_name --sign-in-audience AzureADMyOrg --enable-id-token-issuance true | jq -r '.appId')
else
    echo "App registration for $app_name already exists"

    app_id=$(echo $app_registrations | jq -r '.[0].appId')
fi
tenantId=$(az account show | jq -r '.tenantId')

if [ $ENVIRONMENT == 'development' ]; then
    echo Update the appsettings.Development.json file with the following values:
    echo "    \"AzureAd\": {"
    echo "        \"TenantId\": \"$tenantId\","
    echo "        \"ClientId\": \"$app_id\""
    echo "    }"
fi

echo Adding environment variables to azd environment
azd env set AUTH_TENANT_ID $tenantId
azd env set AUTH_CLIENT_ID $app_id

echo App Registration complete
