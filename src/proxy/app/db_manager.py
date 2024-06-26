"""Database manager"""

import asyncio
import contextvars
import logging
import os
from datetime import datetime

import asyncpg
from azure.identity import DefaultAzureCredential
from fastapi import HTTPException

# 6 hr recycle time
RECYCLE_TIME_SECONDS = 60 * 60 * 6

logging.basicConfig(level=logging.INFO)

# Create a context variable for the connection
conn_var = contextvars.ContextVar("conn_var")


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
        self.pool_timestamp = datetime.min
        self.ssl_mode = "development" not in os.environ.get("ENVIRONMENT").lower()
        # The command_timeout=60 is used when creating a connection from the pool.
        # The default is None/Indefinite. A connection stuck on indefinite wait will cause
        # close pool to wait indefinitly. Note, close pool is wrapped in a timeout
        # https://magicstack.github.io/asyncpg/current/api/index.html
        self.connection_kwargs = {"command_timeout": 60, "ssl": self.ssl_mode, "timeout": 30}

    async def create_pool(self):
        """create database pool"""
        self.logging.info("Creating connection pool")
        try:
            self.db_pool = await asyncpg.create_pool(
                dsn=self.db_config.get_connection_string(),
                max_size=50,
                **self.connection_kwargs,
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
            await asyncio.wait_for(self.db_pool.close(), timeout=60)
        except TimeoutError as error:
            self.logging.error("Postgres close timeout exception: %s", str(error))
            self.logging.error("Postgres pool terminated: %s", str(error))
            self.db_pool.terminate()
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

    async def __aenter__(self):
        """Get a connection from the pool"""
        # If the pool is older than RECYCLE_TIME_SECONDS, generate new connection string
        if (datetime.now() - self.pool_timestamp).total_seconds() > RECYCLE_TIME_SECONDS:
            self.logging.info("Renewing connection pool connection string")
            # generate a new connection string for new connections to use in the pool
            self.db_pool.set_connect_args(
                dsn=self.db_config.get_connection_string(), **self.connection_kwargs
            )
            # expire all connections using previous connection string
            await self.db_pool.expire_connections()
            self.pool_timestamp = datetime.now()

        # Acquire a new connection and store it in the context variable
        conn = await self.db_pool.acquire(timeout=30)
        conn_var.set(conn)
        return conn

    async def __aexit__(self, exc_type, exc_value, exc_tb):
        """Release the connection back to the pool"""
        # Fetch the connection from the context variable
        conn = conn_var.get()
        await self.db_pool.release(conn, timeout=15)
