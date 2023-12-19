FROM python:3.11

ENV ENVIRONMENT production

# Install curl and unzip
RUN apt-get update && apt-get install -y curl unzip

WORKDIR /code

COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

COPY . .

EXPOSE 3100

CMD ["gunicorn", "api.main:app"]
