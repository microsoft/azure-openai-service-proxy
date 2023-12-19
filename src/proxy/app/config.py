""" Config Manager """

import logging
import random

import pyodbc
from fastapi import HTTPException

# pylint: disable=E0402
from .authorize import AuthorizeResponse
from .lru_cache_with_expiry import lru_cache_with_expiry

# initiase the random number generator
random.seed()

logging.basicConfig(level=logging.INFO)


class Deployment:
    """Deployment"""

    def __init__(
        self,
        *,
        friendly_name: str = "",
        endpoint_key: str,
        deployment_name: str,
        resource_name: str,
        model_class: str,
    ):
        """init deployment"""
        self.friendly_name = friendly_name
        self.endpoint_key = endpoint_key
        self.deployment_name = deployment_name
        self.resource_name = resource_name
        self.model_class = model_class


class Config:
    """Config Manager"""

    def __init__(self, sql_conn: pyodbc.Connection):
        self.sql_conn = sql_conn
        self.logging = logging.getLogger(__name__)

    @lru_cache_with_expiry(maxsize=128, ttl=300)
    async def get_event_catalog(self, event_id: str, deployment_class: str) -> list[Deployment]:
        """get config"""

        config = []

        try:
            cursor = self.sql_conn.cursor()

            if deployment_class == "*":
                cursor.execute("{CALL dbo.EventCatalogList(?)}", (event_id,))
            else:
                cursor.execute("{CALL dbo.EventCatalogGetByEvent(?, ?)}", (event_id, deployment_class))

            result = cursor.fetchall()

            for row in result:
                deployment_item = Deployment(
                    friendly_name=row.FriendlyName.strip(),
                    endpoint_key=row.EndpointKey.strip(),
                    deployment_name=row.DeploymentName.strip(),
                    resource_name=row.ResourceName.strip(),
                    model_class=row.ModelClass.strip(),
                )

                config.append(deployment_item)

            return config

        except pyodbc.Error as error:
            self.logging.error("pyodbc error: %s", str(error))
            raise HTTPException(
                status_code=401,
                detail="Deployment failed. pyodbc error.",
            ) from error

        except Exception as e:
            self.logging.error("Error reading config from Azure Table Storage")
            self.logging.error(e)
            raise HTTPException(
                detail="Error reading config from Azure Table Storage",
                status_code=503,
            ) from e

    async def get_catalog_by_model_class(self, authorize_response: AuthorizeResponse) -> Deployment:
        """get config"""

        deployments = await self.get_event_catalog(
            authorize_response.event_id,
            authorize_response.request_class,
        )

        deployment_count = len(deployments)

        if deployment_count == 0:
            self.logging.warning("No active OpenAI model deployments found.")
            # 501 The server either does not recognize the request method,
            # or it lacks the ability to fulfil the request.
            # Usually this implies future availability (e.g., a new feature of a web-service API).
            # https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
            raise HTTPException(
                detail="No active OpenAI model deployments found.",
                status_code=501,
            )
        # get a random deployment to balance load
        index = random.randint(0, deployment_count - 1)

        return deployments[index]

    async def get_catalog_by_friendly_name(
        self,
        friendly_name: str,
        authorise_response: AuthorizeResponse,
    ) -> Deployment:
        """get config"""

        deployments = await self.get_event_catalog(
            authorise_response.event_id,
            authorise_response.request_class,
        )

        for deployment in deployments:
            if deployment.friendly_name == friendly_name:
                return deployment

        self.logging.warning("No active OpenAI model deployments found.")
        # 501 The server either does not recognize the request method,
        # or it lacks the ability to fulfil the request.
        # Usually this implies future availability (e.g., a new feature of a web-service API).
        # https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
        # https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
        raise HTTPException(
            detail="No active OpenAI model deployments found.",
            status_code=501,
        )

    async def get_owner_catalog(self, authorize_response: AuthorizeResponse) -> list[str]:
        """get model class list from config deployment table"""

        deployments = await self.get_event_catalog(
            authorize_response.event_id,
            "*",
        )

        # get unique model classes from the deployment list
        model_classes = set()
        for deployment in deployments:
            model_classes.add(deployment.model_class)

        return list(model_classes)
