
azd env set ENTRA_AUTHORIZATION_TOKEN $(az account get-access-token --resource-type oss-rdbms --query accessToken -o tsv)
