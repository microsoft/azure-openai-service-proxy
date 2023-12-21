# Exporting the database

## Dump

```bash
pg_dump -h HOST_NAME -d aoai-proxy -U admin -n aoai  -s -W -f aoai-proxy.sql
```

## Restore

```bash
psql -U admin -d aoai-proxy -h localhost -w -f ./database/aoai-proxy.sql
```
