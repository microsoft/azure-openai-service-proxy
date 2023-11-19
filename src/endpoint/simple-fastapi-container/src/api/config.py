""" configuration manager"""

import logging
import random

from datetime import datetime, timedelta
from fastapi import HTTPException
from azure.data.tables import TableServiceClient
from azure.data.tables.aio import TableClient
from azure.core.exceptions import (
    AzureError,
)

CACHE_EXPIRY_MINUTES = 8
CONFIGURATION_TABLE_NAME = "configuration"

logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

# initiase the random number generator
random.seed()


class Deployment:
    """Deployment"""

    def __init__(
        self,
        *,
        friendly_name: str = "",
        endpoint_key: str,
        deployment_name: str,
        resource_name: str,
    ):
        """init deployment"""
        self.friendly_name = friendly_name
        self.endpoint_key = endpoint_key
        self.deployment_name = deployment_name
        self.resource_name = resource_name


class OpenAIConfig:
    """OpenAI Parameters"""

    def __init__(
        self,
        *,
        connection_string: str,
        model_class: str,
    ):
        """init in memory config manager"""
        self.connection_string = connection_string
        self.model_class = model_class

        self.round_robin = 0
        self.config = None
        self.cache_expiry = None
        self.deployments = {}

        # Create configuration table if it does not exist
        try:
            table_service_client = TableServiceClient.from_connection_string(
                conn_str=self.connection_string
            )

            table_service_client.create_table_if_not_exists(
                table_name=CONFIGURATION_TABLE_NAME
            )
        except Exception as exception:
            logging.error("General exception creating table: %s", str(exception))
            raise

    async def __load_config(self) -> dict[str, str]:
        """load configuration from azure table storage"""

        try:
            async with TableClient.from_connection_string(
                conn_str=self.connection_string, table_name=CONFIGURATION_TABLE_NAME
            ) as table_client:
                config = []

                query_filter = (
                    f"PartitionKey eq '{self.model_class}' and Active eq true"
                )
                # get all columns from the table
                queried_entities = table_client.query_entities(
                    query_filter=query_filter,
                    select=[
                        "*",
                    ],
                )

                async for entity in queried_entities:
                    deployment_item = Deployment(
                        friendly_name=entity.get("RowKey", "").strip(),
                        endpoint_key=entity.get("EndpointKey", "").strip(),
                        deployment_name=entity.get("DeploymentName", "").strip(),
                        resource_name=entity.get("ResourceName", "").strip(),
                    )

                    config.append(deployment_item)

                if len(config) == 0:
                    logger.warning("No active OpenAI model deployments found.")
                    # 503 Service Unavailable
                    # The server cannot handle the request (because it is overloaded or down for
                    # maintenance). Generally, this is a temporary state
                    # https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
                    raise HTTPException(
                        detail="No active OpenAI model deployments found.",
                        status_code=503,
                    )
                else:
                    logger.warning(
                        "Found %s active OpenAI model deployments.", len(config)
                    )
                    # pretty print the config
                    for deployment in config:
                        logger.warning(
                            "Friendly Name: %s, Deployment Name: %s, Resource Name: %s",
                            deployment.friendly_name,
                            deployment.deployment_name,
                            deployment.resource_name,
                        )

                return config

        except HTTPException:
            raise

        except AzureError as azure_error:
            logger.warning("AzureError: %s", str(azure_error))
            raise HTTPException(
                status_code=504,
                detail="AzureError: Can't connect to the Azure Configuration Table",
            ) from azure_error

        except Exception as exception:
            logger.warning("Exception: %s", str(exception))
            raise HTTPException(
                status_code=500,
                detail="Exception: Can't connect to the Azure Configuration Table",
            ) from exception

    async def get_deployment(self) -> Deployment | None:
        """get deployment by name"""

        # set cache_expiry to current time plus CACHE_EXPIRY_MINUTES minutes
        if self.cache_expiry is None or datetime.now() > self.cache_expiry:
            logger.warning("Loading configuration from Azure Table Storage")
            self.deployments = await self.__load_config()
            self.cache_expiry = datetime.now() + timedelta(
                minutes=CACHE_EXPIRY_MINUTES + random.randint(0, 4)
            )

        index = int(self.round_robin % len(self.deployments))
        self.round_robin += 1

        deployment = self.deployments[index]

        return Deployment(
            friendly_name=deployment.friendly_name,
            endpoint_key=deployment.endpoint_key,
            deployment_name=deployment.deployment_name,
            resource_name=deployment.resource_name,
        )
