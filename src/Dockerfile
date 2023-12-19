FROM python:3.11

ENV ENVIRONMENT production

# https://learn.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server?view=sql-server-ver16&tabs=ubuntu18-install%2Calpine17-install%2Cdebian8-install%2Credhat7-13-install%2Crhel7-offline#18

RUN if ! [[ "18.04 20.04 22.04 23.04" == *"$(lsb_release -rs)"* ]]; \
    then \
        echo "Ubuntu $(lsb_release -rs) is not currently supported."; \
        exit; \
    fi

RUN curl https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc
RUN curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list

RUN apt-get update && ACCEPT_EULA=Y apt-get install -y msodbcsql18 && ACCEPT_EULA=Y apt-get install -y mssql-tools18
RUN echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc && source ~/.bashrc


# Install curl and unzip
RUN apt-get update && apt-get install -y curl unzip unixodbc-dev

WORKDIR /code

COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

COPY . .

# Install curl and unzip
RUN apt-get update && apt-get install -y curl unzip

# Install NVM
ENV NVM_DIR /root/.nvm
ENV NODE_VERSION 20

RUN curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.5/install.sh | bash \
    && . $NVM_DIR/nvm.sh \
    && nvm install $NODE_VERSION \
    && nvm alias default $NODE_VERSION \
    && nvm use default

# Add Node.js and NPM path to environment
ENV NODE_PATH $NVM_DIR/v$NODE_VERSION/lib/node_modules
ENV PATH $NVM_DIR/versions/node/v$NODE_VERSION/bin:$PATH

RUN . $NVM_DIR/nvm.sh \
    && cd playground \
    && npm install \
    && npm run build


EXPOSE 3100

CMD ["gunicorn", "api.main:app"]
