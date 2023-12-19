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

echo Setting up .NET environment...
cd ../
dotnet tool restore
dotnet restore

echo Setting up database...
# Per: https://learn.microsoft.com/en-gb/sql/tools/sqlcmd/sqlcmd-utility?view=sql-server-ver16&tabs=go%2Clinux&pivots=cs1-bash#download-and-install-sqlcmd
curl https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/20.04/prod.list)"
sudo apt-get update
sudo apt-get install sqlcmd
sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -i ../database/schema.sql
