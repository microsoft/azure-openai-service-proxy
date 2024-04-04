""" Database manager """

import logging

import asyncpg
from fastapi import FastAPI, HTTPException

logging.basicConfig(level=logging.WARNING)


class DBManager:
    """Database manager"""

    def __init__(self, app: FastAPI, postgres_encryption_key: str) -> None:
        self.logging = logging.getLogger(__name__)
        self.app = app
        self.postgres_encryption_key = postgres_encryption_key

    async def create_pool(self, connection_string: str):
        """create database pool"""
        print("Creating connection pool")
        try:
            self.app.pool = await asyncpg.create_pool(
                connection_string, max_size=30, max_inactive_connection_lifetime=180
            )
        except asyncpg.exceptions.PostgresError as error:
            self.logging.error("Postgres error: %s", str(error))
            raise HTTPException(
                status_code=501, detail=f"Postgres error opening pool exp {str(error)}"
            ) from error

        except Exception as exception:
            self.logging.error("Error: %s", str(exception))
            raise HTTPException(
                status_code=501, detail=f"Postgres error opening pool exp {str(exception)}"
            ) from exception

    async def close_pool(self):
        """close database pool"""
        print("Closing connection pool")
        try:
            await self.app.pool.close()
        except asyncpg.exceptions.PostgresError as error:
            self.logging.error("Postgres error: %s", str(error))
            raise HTTPException(
                status_code=501, detail=f"Postgres error closing pool {str(error)}"
            ) from error

        except Exception as exception:
            self.logging.error("Error: %s", str(exception))
            raise HTTPException(
                status_code=501, detail=f"Postgres error closing pool {str(exception)}"
            ) from exception

    async def get_connection(self):
        """connect to database"""
        return self.app.pool

    def get_postgres_encryption_key(self):
        """get postgres encryption key"""
        return self.postgres_encryption_key
