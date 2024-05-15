"""Database manager"""

import asyncio
import logging
from datetime import datetime

import asyncpg
from azure.identity import DefaultAzureCredential
from fastapi import HTTPException

# 1 hr recycle time
RECYCLE_TIME_SECONDS = 60

logging.basicConfig(level=logging.INFO)


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
        self.logging = logging.getLogger(__name__)

        if not connection_string:
            if not host or not user:
                raise HTTPException(
                    status_code=500,
                    detail="Please set the environment variables POSTGRES_SERVER, POSTGRES_USER",
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
            self.logging.info("Using Postgres Entra Token Authorization")
            azure_credential = DefaultAzureCredential()
            token = azure_credential.get_token(
                "https://ossrdbms-aad.database.windows.net/.default"
            ).token

            connection_string = (
                f"postgresql://{self.user}:{token}@{self.host}:{self.port}/{self.database}"
            )
        else:
            self.logging.info("Using Postgres Password Authorization")
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
        self.conn = None
        self.pool_timestamp = datetime.min

    async def create_pool(self):
        """create database pool"""
        self.logging.info("Creating connection pool")
        try:
            self.db_pool = await asyncpg.create_pool(
                self.db_config.get_connection_string(), max_size=30, command_timeout=60
            )
            self.logging.info("Connection pool created")
            self.pool_timestamp = datetime.now()
        except asyncpg.exceptions.PostgresError as error:
            self.logging.error("Postgres error: %s", str(error))
            raise HTTPException(
                status_code=503, detail=f"Postgres error opening pool exp {str(error)}"
            ) from error

        except Exception as exception:
            self.logging.error("Error: %s", str(exception))
            raise HTTPException(
                status_code=503, detail=f"Postgres error opening pool exp {str(exception)}"
            ) from exception

    async def close_pool(self):
        """close database pool"""
        self.logging.info("Closing connection pool")
        try:
            await self.db_pool.close()
        except asyncpg.exceptions.PostgresError as error:
            self.logging.error("Postgres error: %s", str(error))
            raise HTTPException(
                status_code=503, detail=f"Postgres error closing pool {str(error)}"
            ) from error

        except Exception as exception:
            self.logging.error("Error: %s", str(exception))
            raise HTTPException(
                status_code=503, detail=f"Postgres exception closing pool {str(exception)}"
            ) from exception

    def get_postgres_encryption_key(self):
        """get postgres encryption key"""
        return self.db_config.encryption_key

    async def recycle_connection_pool(self):
        """Recycle the connection pool to renew the managed identity"""
        retries = 3
        for attempt in range(1, retries + 1):
            try:
                self.logging.info("Closing connection pool. Attempt: %d", attempt)
                await asyncio.wait_for(self.db_pool.close(), timeout=10)
                break  # Break the loop if successful
            except TimeoutError:
                self.logging.error(
                    "Attempt %d: Timeout occurred while recycling the connection pool", attempt
                )
                if attempt == retries:
                    # Final attempt failed, handle accordingly
                    self.logging.error(
                        "Final attempt to recycle the connection pool failed. pool terminated."
                    )
                    self.db_pool.terminate()  # brutal but fair!
            except asyncpg.exceptions.PostgresError as pg_error:
                self.logging.error(
                    "Postgres exception recycling the connection pool. Attempt: %d: Error: %s",
                    attempt,
                    str(pg_error),
                )
                self.db_pool.terminate()
                break

        self.logging.info("Opening new connection pool.")
        await self.create_pool()

    async def __aenter__(self):
        """Get a connection from the pool"""
        # If the pool is older than RECYCLE_TIME_SECONDS, recycle it to renew the managed identity
        if (datetime.now() - self.pool_timestamp).total_seconds() > RECYCLE_TIME_SECONDS:
            await self.recycle_connection_pool()

        self.conn = await self.db_pool.acquire()
        return self.conn

    async def __aexit__(self, exc_type, exc_value, exc_tb):
        """Release the connection back to the pool"""
        await self.db_pool.release(self.conn)
