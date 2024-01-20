""" Database manager """

import logging

import asyncpg
from asyncpg import create_pool
from fastapi import HTTPException

logging.basicConfig(level=logging.WARNING)


class DBManager:
    """Database manager"""

    def __init__(self, connection_string: str) -> None:
        self.connection_string = connection_string
        self.sql_conn = None
        self.pool = None
        self.logging = logging.getLogger(__name__)

    async def get_connection(self):
        """connect to database"""
        if self.pool is None:
            try:
                # self.sql_conn = await asyncpg.connect(self.connection_string)
                self.pool = await create_pool(
                    self.connection_string, max_size=100, max_inactive_connection_lifetime=4
                )

            except asyncpg.exceptions.PostgresError as error:
                self.logging.error("Postgres error connecting to server: %s", str(error))
                raise HTTPException(
                    status_code=503,
                    detail="Postgres error connecting to server.",
                ) from error

        return self.pool
