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

# echo Setting up .NET environment...
# cd ../
# dotnet tool restore
# dotnet restore

# sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -i ../database/schema.sql

# install postgresql client
sudo apt-get update
sudo apt install postgresql-client -y

psql -U admin -d aoai-proxy -h localhost -W -f ../database/aoai-proxy.sql
