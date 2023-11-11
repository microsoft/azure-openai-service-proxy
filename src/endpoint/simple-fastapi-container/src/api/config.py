""" configuration manager"""

import logging
import random

from datetime import datetime, timedelta

from azure.data.tables import TableServiceClient
from azure.data.tables.aio import TableClient
from azure.core.exceptions import (
    HttpResponseError,
    ServiceRequestError,
    ClientAuthenticationError,
)

CACHE_EXPIRY_MINUTES = 8
TABLE_NAME = "configuration"

logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

# initiase the random number generator
random.seed()


class ConfigError(Exception):
    pass


class Deployment:
    """Deployment"""

    def __init__(
        self,
        *,
        friendly_name: str = "",
        # endpoint_location: str,
        endpoint_key: str,
        deployment_name: str,
        api_version: str,
        resource_name: str,
    ):
        """init deployment"""
        self.friendly_name = friendly_name
        # self.endpoint_location = endpoint_location
        self.endpoint_key = endpoint_key
        self.deployment_name = deployment_name
        self.api_version = api_version
        self.resource_name = resource_name


class OpenAIConfig:
    """OpenAI Parameters"""

    def __init__(
        self,
        *,
        api_version: str,
        connection_string: str,
        model_class: str,
    ):
        """init in memory config manager"""
        self.api_version = api_version
        self.connection_string = connection_string
        self.model_class = model_class

        self.round_robin = 0
        self.config = None
        self.cache_expiry = None

        # Create events playground authorization table if it does not exist
        try:
            table_service_client = TableServiceClient.from_connection_string(
                conn_str=self.connection_string
            )

            table_service_client.create_table_if_not_exists(table_name=TABLE_NAME)
        except Exception as exception:
            logging.error("General exception creating table: %s", str(exception))
            raise

    async def __load_config(self) -> dict[str, str]:
        """load configuration from azure table storage"""

        try:
            async with TableClient.from_connection_string(
                conn_str=self.connection_string, table_name=TABLE_NAME
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
                        # endpoint_location=entity.get("Location", "").strip(),
                        endpoint_key=entity.get("EndpointKey", "").strip(),
                        deployment_name=entity.get("DeploymentName", "").strip(),
                        resource_name=entity.get("ResourceName", "").strip(),
                        api_version=self.api_version.strip(),
                    )

                    config.append(deployment_item)

                if len(config) == 0:
                    logger.warning("No active OpenAI model deployments found.")
                    raise ConfigError(("No active OpenAI model deployments found."))

                return config

        except ClientAuthenticationError as auth_error:
            logger.warning("ClientAuthenticationError: %s", str(auth_error))
            raise ConfigError(
                "ClientAuthenticationError: Can't connect to the Azure Configuration Table"
            )

        except ServiceRequestError as request_error:
            logger.warning("ServiceRequestError: %s", str(request_error))
            raise ConfigError(
                "ServiceResponseError: Can't connect to the Azure Configuration Table",
            )

        except HttpResponseError as response_error:
            logger.warning("HttpResponseError: %s", str(response_error))
            raise ConfigError(
                "HttpResponseError: Can't connect to the Azure Configuration Table"
            )

        except ConfigError:
            raise

        except Exception as exception:
            logger.warning("Exception: %s", str(exception))
            raise ConfigError(f"Exception in {__name__}: {str(exception)}")

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
            # endpoint_location=deployment.endpoint_location,
            endpoint_key=deployment.endpoint_key,
            deployment_name=deployment.deployment_name,
            resource_name=deployment.resource_name,
            api_version=self.api_version,
        )
