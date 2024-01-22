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

echo "Ensuring admin app is registered for $AUTH_CLIENT_ID"

app_registration=$(az ad app show --id $AUTH_CLIENT_ID -o json)
existing_redirects=$(echo $app_registration | jq '.web.redirectUris')

if [ $ENVIRONMENT == 'development' ]; then
    echo Ensuring localhost is registered as a redirect uri
    if $(echo $existing_redirects | jq "contains([\"http://localhost:5175${signin_path}\"])"); then
        echo "http://localhost:5175$signin_path already registered as a redirect uri"
    else
        echo "Registering http://localhost:5175$signin_path as a redirect uri"
        az ad app update --id $AUTH_CLIENT_ID --web-redirect-uris $(echo $existing_redirects | jq -r 'join(" ")') "http://localhost:5175${signin_path}"

        # Refresh app registration and redirect uris
        app_registration=$(az ad app show --id $AUTH_CLIENT_ID -o json)
        existing_redirects=$(echo $app_registration | jq '.web.redirectUris')
    fi
fi

if [ $SERVICE_ADMIN_URI ]; then
    echo Ensuring $SERVICE_ADMIN_URI is registered as a redirect uri
    if $(echo $existing_redirects | jq "contains([\"${SERVICE_ADMIN_URI}${signin_path}\"])"); then
        echo "$SERVICE_ADMIN_URI$signin_path already registered as a redirect uri"
    else
        echo "Registering $SERVICE_ADMIN_URI$signin_path as a redirect uri"
        az ad app update --id $app_id --web-redirect-uris $(echo $existing_redirects | jq -r 'join(" ")') "${SERVICE_ADMIN_URI}${signin_path}"

        # Refresh app registration and redirect uris
        app_registration=$(az ad app show --id $app_id -o json)
        existing_redirects=$(echo $app_registration | jq '.web.redirectUris')
    fi
fi
