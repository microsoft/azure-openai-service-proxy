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

echo "AzureAD__ClientId:$app_id"
echo "AzureAD__TenantId:$(az account show | jq -r '.tenantId')"

echo "Ensuring admin app is registered for $app_name"

app_registration=$(az ad app show --id $app_id -o json)

if [ $ENVIRONMENT == 'development' ]; then
    echo Ensuring localhost is registered as a redirect uri
    if $(echo $app_registration | jq '.web.redirectUris' | jq 'contains(["http://localhost:5175$signin_path"])'); then
        echo "http://localhost:5175$signin_path already registered as a redirect uri"
    else
        echo "Registering http://localhost:5175$signin_path as a redirect uri"
        az ad app update --id $app_id --web-redirect-uris "http://localhost:5175$signin_path"
    fi
fi

if [ $SERVICE_ADMIN_URI ]; then
    echo Ensuring $SERVICE_ADMIN_URI is registered as a redirect uri
    if $(echo $app_registration | jq '.web.redirectUris' | jq 'contains(["$SERVICE_ADMIN_URI$signin_path"])'); then
        echo "$SERVICE_ADMIN_URI$signin_path already registered as a redirect uri"
    else
        echo "Registering $SERVICE_ADMIN_URI$signin_path as a redirect uri"
        az ad app update --id $app_id --web-redirect-uris "$SERVICE_ADMIN_URI$signin_path"
    fi
fi
