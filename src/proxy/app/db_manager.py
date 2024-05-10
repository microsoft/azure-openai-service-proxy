"""Database manager"""

import asyncio
import logging
from datetime import datetime

import asyncpg
from azure.core.exceptions import ClientAuthenticationError
from azure.identity import DefaultAzureCredential
from fastapi import HTTPException

# 1 hr recycle time
RECYCLE_TIME_SECONDS = 60 * 60 * 1

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

    def get_auth_token(self, logger):
        """get token"""
        max_retry = 0
        while max_retry < 5:
            try:
                azure_credential = DefaultAzureCredential()
                logger.info("Getting Entra Auth Token")
                return azure_credential.get_token(
                    "https://ossrdbms-aad.database.windows.net/.default"
                ).token
            except ClientAuthenticationError as cae:
                asyncio.sleep(2)
                max_retry += 1
                logger.warning(f"Retrying to get Entra Auth Token {cae.message}")
            except Exception as exception:
                raise HTTPException(
                    status_code=503, detail=f"Exception: Getting Entra Auth Token {str(exception)}"
                ) from exception

        raise HTTPException(
            status_code=503, detail="Retry exceeed: Getting Entra Auth Token failed after 5 retries"
        )

    def get_connection_string(self, logger):
        """get connection string"""
        if self.connection_string:
            return self.connection_string

        if not self.password:
            logger.info("Using Postgres Entra Token Authorization")
            token = self.get_auth_token(logger)

            connection_string = (
                f"postgresql://{self.user}:{token}@{self.host}:{self.port}/{self.database}"
            )
        else:
            logger.info("Using Postgres Password Authorization")
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
                self.db_config.get_connection_string(self.logging),
                max_size=30,
                max_inactive_connection_lifetime=180,
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

    # https://realpython.com/python-with-statement/
    async def __aenter__(self):
        """Get a connection from the pool"""
        # If the pool is older than RECYCLE_TIME_SECONDS, recycle it to renew the managed identity
        if (datetime.now() - self.pool_timestamp).total_seconds() > RECYCLE_TIME_SECONDS:
            self.logging.info("Connection pool recycled")
            await self.db_pool.close()
            await self.create_pool()

        self.conn = await self.db_pool.acquire()
        return self.conn

    async def __aexit__(self, exc_type, exc_value, exc_tb):
        """Release the connection back to the pool"""
        await self.db_pool.release(self.conn)
