""" Database manager """

import logging

import asyncpg
from fastapi import HTTPException

logging.basicConfig(level=logging.WARNING)


class DBManager:
    """Database manager"""

    def __init__(self, connection_string: str) -> None:
        self.connection_string = connection_string
        self.sql_conn = None
        self.logging = logging.getLogger(__name__)

    async def get_connection(self):
        """connect to database"""
        if self.sql_conn is None or self.sql_conn.is_closed():
            try:
                self.sql_conn = await asyncpg.connect(self.connection_string)
            except asyncpg.exceptions.PostgresError as error:
                self.logging.error("Postgres error connecting to server: %s", str(error))
                raise HTTPException(
                    status_code=503,
                    detail="Postgres error connecting to server.",
                ) from error

        return self.sql_conn
