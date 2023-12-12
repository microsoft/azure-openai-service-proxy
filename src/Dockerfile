FROM python:3.11

ENV ENVIRONMENT production

# Install curl and unzip
RUN apt-get update && apt-get install -y curl unzip

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
