""" Database manager """

import asyncpg


class DBManager:
    """Database manager"""

    def __init__(self, connection_string: str) -> None:
        self.connection_string = connection_string
        self.sql_conn = None

    async def get_connection(self):
        """connect to database"""
        if self.sql_conn is None or self.sql_conn.is_closed():
            self.sql_conn = await asyncpg.connect(self.connection_string)

        return self.sql_conn
