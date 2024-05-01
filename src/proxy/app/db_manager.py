""" Database manager """

import logging

import asyncpg
from azure.identity import DefaultAzureCredential
from fastapi import HTTPException

logging.basicConfig(level=logging.NOTSET)


class DBConfig:
    """Database configuration"""

    def __init__(
        self,
        host: str,
        port: int,
        database: str,
        user: str,
        password: str,
        encryption_key: str,
        connection_string: str,
    ) -> None:
        self.host = host.strip() if host else None
        self.port = port
        self.database = database.strip() if database else None
        self.user = user.strip() if user else None
        self.password = password.strip() if password else None
        self.encryption_key = encryption_key.strip() if encryption_key else None
        self.connection_string = connection_string.strip() if connection_string else None

        if not connection_string:
            if not host or not user:
                raise HTTPException(
                    status_code=500,
                    detail=(
                        "Please set the environment variables " "POSTGRES_SERVER, POSTGRES_USER"
                    ),
                )

        if not encryption_key:
            raise HTTPException(
                status_code=500,
                detail="Please set the environment variable POSTGRES_ENCRYPTION_KEY",
            )

    def get_connection_string(self):
        """get connection string"""
        if self.connection_string:
            return self.connection_string

        if not self.password:
            azure_credential = DefaultAzureCredential()
            self.password = azure_credential.get_token(
                "https://ossrdbms-aad.database.windows.net/.default"
            ).token

        connection_string = (
            f"postgresql://{self.user}:{self.password}@{self.host}:{self.port}/{self.database}"
        )

        return connection_string


class DBManager:
    """Database manager"""

    def __init__(self, db_config: DBConfig) -> None:
        self.logging = logging.getLogger(__name__)
        self.db_config = db_config
        self.db_pool = None

    async def create_pool(self):
        """create database pool"""
        print("Creating connection pool")
        try:
            self.db_pool = await asyncpg.create_pool(
                self.db_config.get_connection_string(),
                max_size=30,
                max_inactive_connection_lifetime=180,
            )
            self.logging.info("Connection pool created")
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
            await self.db_pool.close()
        except asyncpg.exceptions.PostgresError as error:
            self.logging.error("Postgres error: %s", str(error))
            raise HTTPException(
                status_code=501, detail=f"Postgres error closing pool {str(error)}"
            ) from error

        except Exception as exception:
            self.logging.error("Error: %s", str(exception))
            raise HTTPException(
                status_code=501, detail=f"Postgres exception closing pool {str(exception)}"
            ) from exception

    async def get_connection(self):
        """connect to database"""
        return self.db_pool

    def get_postgres_encryption_key(self):
        """get postgres encryption key"""
        return self.db_config.encryption_key
