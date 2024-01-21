""" Database manager """

import logging

import asyncpg
from fastapi import FastAPI, HTTPException

MAX_RETRIES = 6

logging.basicConfig(level=logging.WARNING)


class DBManager:
    """Database manager"""

    def __init__(self, app: FastAPI) -> None:
        # self.connection_string = connection_string
        # self.sql_conn = None
        # self.pool = None
        self.logging = logging.getLogger(__name__)
        self.app = app

    async def create_pool(self, connection_string: str):
        """create database pool"""
        print("Creating connection pool")
        try:
            self.app.pool = await asyncpg.create_pool(
                connection_string, max_size=30, max_inactive_connection_lifetime=180
            )
        except asyncpg.exceptions.PostgresError as error:
            self.logging.error("Postgres error: %s", str(error))
            raise HTTPException(status_code=501, detail="Postgres error opening pool") from error

    async def get_connection(self):
        """connect to database"""
        return self.app.pool
