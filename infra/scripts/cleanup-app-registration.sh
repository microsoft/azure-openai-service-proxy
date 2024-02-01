#!/bin/bash

account_info=$(az account show 2>&1)
if [[ $? -ne 0 ]]; then
    echo "You must be logged in to Azure to run this script"
    echo "Run 'az login' to log in to Azure"
    exit 1
fi

echo "Loading azd .env file from current environment"
# Use the `get-values` azd command to retrieve environment variables from the `.env` file
while IFS='=' read -r key value; do
    value=$(echo "$value" | sed 's/^"//' | sed 's/"$//')
    export "$key=$value"
done <<EOF
$(azd env get-values)
EOF

if [[ -z "${AUTH_CLIENT_ID}" ]]; then
    echo "AUTH_CLIENT_ID is not set. No app to delete."
fi

echo Deleting app registration for $AUTH_CLIENT_ID
az ad app delete --id $AUTH_CLIENT_ID

azd env set AUTH_CLIENT_ID ""
azd env set AUTH_TENANT_ID ""
