""" Database manager """

import asyncio
import logging

import asyncpg

MAX_RETRIES = 6

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
            for i in range(MAX_RETRIES):
                try:
                    self.pool = await asyncpg.create_pool(
                        self.connection_string, max_size=200, max_inactive_connection_lifetime=2
                    )
                    break  # If the connection is successful, break the loop
                except asyncpg.exceptions.PostgresError:
                    if i < MAX_RETRIES - 1:  # i is zero indexed
                        await asyncio.sleep(2**i)  # exponential backoff
                        print(f"Retrying connection to database. Attempt {i+1} of {MAX_RETRIES}")
                    else:
                        raise  # If this was the last retry and it still failed, raise the exception

        return self.pool
