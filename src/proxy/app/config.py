""" Config Manager """

import logging
import random

import asyncpg
from fastapi import HTTPException

# pylint: disable=E0402
from .authorize import AuthorizeResponse
from .lru_cache_with_expiry import lru_cache_with_expiry

# initiase the random number generator
random.seed()

logging.basicConfig(level=logging.INFO)


class Deployment:
    """Deployment"""

    def __init__(self, *, endpoint_key: str, deployment_name: str, model_type: str, resource_name: str):
        """init deployment"""
        self.endpoint_key = endpoint_key
        self.model_type = model_type
        self.deployment_name = deployment_name
        self.resource_name = resource_name


class Config:
    """Config Manager"""

    def __init__(self, db_manager):
        self.db_manager = db_manager
        print(type(self.db_manager))
        self.logging = logging.getLogger(__name__)

    @lru_cache_with_expiry(maxsize=128, ttl=300)
    async def get_event_catalog(self, event_id: str, deployment_name: str | None) -> list[Deployment]:
        """get config"""

        config = []

        try:
            conn = await self.db_manager.get_connection()

            if deployment_name is None:
                result = await conn.fetch("SELECT * FROM aoai.get_models_by_event($1)", event_id)
            else:
                result = await conn.fetch(
                    "SELECT * FROM aoai.get_models_by_deployment_name($1, $2)", event_id, deployment_name
                )

            for row in result:
                deployment_item = Deployment(
                    endpoint_key=row.get("endpoint_key").strip(),
                    deployment_name=row.get("deployment_name").strip(),
                    model_type=row.get("model_type").strip(),
                    resource_name=row.get("resource_name").strip(),
                )

                config.append(deployment_item)

            return config

        except asyncpg.exceptions.PostgresError as error:
            self.logging.error("Postgres error: %s", str(error))
            raise HTTPException(
                status_code=503,
                detail="Error reading model catalog.",
            ) from error

        except Exception as exp:
            self.logging.error("Postgres exception: %s", str(exp))
            self.logging.error(exp)
            raise HTTPException(
                detail="Error reading model catalog.",
                status_code=503,
            ) from exp

    async def get_catalog_by_deployment_name(self, authorize_response: AuthorizeResponse) -> Deployment:
        """get config"""

        deployments = await self.get_event_catalog(authorize_response.event_id, authorize_response.deployment_name)

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

    async def get_owner_catalog(self, authorize_response: AuthorizeResponse) -> list[str]:
        """get model class list from config deployment table"""

        deployments = await self.get_event_catalog(authorize_response.event_id, None)

        # get unique model classes from the deployment list
        model_classes = set()

        for deployment in deployments:
            model_classes.add(deployment.model_class)

        return list(model_classes)
