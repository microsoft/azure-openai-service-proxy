#!/bin/bash

echo Setting up .NET environment...

sudo dotnet workload install aspire
dotnet tool update -g linux-dev-certs
dotnet linux-dev-certs install

dotnet restore src/AzureAIProxy.sln

dotnet user-secrets set "POSTGRES_ENCRYPTION_KEY" myencryptionkey123 --project src/AzureAIProxy.AppHost
dotnet user-secrets set "Parameters:pg-password" "mypassword123" --project src/AzureAIProxy.AppHost

echo Setting up Python environment...

python3 -m pip install -r requirements-dev.txt

echo Setting up commit hooks...
pre-commit install

echo Setting up Playground environment...
cd src/playground
. ${NVM_DIR}/nvm.sh
nvm install
npm i
npm install -g @azure/static-web-apps-cli
