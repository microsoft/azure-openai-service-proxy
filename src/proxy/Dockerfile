FROM python:3.11

ENV ENVIRONMENT production

WORKDIR /code

COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

COPY . .

EXPOSE 3100

CMD ["gunicorn", "app.main:app"]
