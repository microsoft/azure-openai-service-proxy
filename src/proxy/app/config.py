""" Config Manager """

import logging
import random
import uuid

import asyncpg
from fastapi import HTTPException

# pylint: disable=E0402
from .authorize import AuthorizeResponse
from .db_manager import DBManager
from .lru_cache_with_expiry import lru_cache_with_expiry
from .monitor import Monitor

# initiase the random number generator
random.seed()
logging.basicConfig(level=logging.INFO)


class Deployment:
    """Deployment"""

    def __init__(
        self,
        *,
        endpoint_key: str,
        deployment_name: str,
        model_type: str,
        resource_name: str,
        catalog_id: uuid,
    ):
        """init deployment"""
        self.endpoint_key = endpoint_key
        self.model_type = model_type
        self.deployment_name = deployment_name
        self.resource_name = resource_name
        self.catalog_id = catalog_id


class Config:
    """Config Manager"""

    def __init__(self, db_manager: DBManager, monitor: Monitor):
        self.db_manager = db_manager
        self.monitor = monitor
        self.logging = logging.getLogger(__name__)

    @lru_cache_with_expiry(maxsize=128, ttl=300)
    async def get_event_catalog(
        self, event_id: str, deployment_name: str | None
    ) -> list[Deployment]:
        """get config"""

        config = []

        try:
            conn = await self.db_manager.get_connection()

            if deployment_name is None:
                result = await conn.fetch("SELECT * FROM aoai.get_models_by_event($1)", event_id)
            else:
                result = await conn.fetch(
                    "SELECT * FROM aoai.get_models_by_deployment_name($1, $2)",
                    event_id,
                    deployment_name,
                )

            for row in result:
                deployment_item = Deployment(**row)
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

    async def get_catalog_by_deployment_name(
        self, authorize_response: AuthorizeResponse
    ) -> Deployment:
        """get config"""

        deployments = await self.get_event_catalog(
            authorize_response.event_id, authorize_response.deployment_name
        )
        deployment_count = len(deployments)

        if deployment_count == 0:
            self.logging.warning(
                "The request model '%s' is not available for this event.",
                authorize_response.deployment_name,
            )

            event_deploymemts = await self.get_event_deployments(authorize_response)
            # convert the deployment names to a set to deduplicate
            event_deployment_names = {
                name for deployments in event_deploymemts.values() for name in deployments
            }
            # convert the set to a comma separated string
            event_deployment_names_str = ", ".join(event_deployment_names)

            raise HTTPException(
                detail=(
                    f"The request model '{authorize_response.deployment_name}' "
                    "is not available for this event. "
                    f"Available models are: {event_deployment_names_str}"
                ),
                status_code=400,
            )

        # get a random deployment to balance load
        index = random.randint(0, deployment_count - 1)

        authorize_response.catalog_id = deployments[index].catalog_id

        await self.monitor.log_api_call(entity=authorize_response)

        return deployments[index]

    async def get_event_deployments(self, authorize_response: AuthorizeResponse) -> list[str]:
        """get model class list from config deployment table"""

        deployments = await self.get_event_catalog(authorize_response.event_id, None)
        event_model_types = {}

        # create a dictionary with a key of deployment.model_type with a list of deployment names
        for deployment in deployments:
            if deployment.model_type not in event_model_types:
                event_model_types[deployment.model_type] = []
            event_model_types[deployment.model_type].append(deployment.deployment_name)

        return event_model_types
