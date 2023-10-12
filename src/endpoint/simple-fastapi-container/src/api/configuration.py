""" configuration manager"""

import logging

# caching config from azure table storage yet to be implemented.
# for now, we will use an environment variables and in-memory config

CACHE_EXPIRY_MINUTES = 10
PARTITION_KEY = "openai"
TABLE_NAME = "configuration"

logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)


class Deployment:
    """Deployment"""

    def __init__(
        self,
        *,
        endpoint_location: str,
        endpoint_key: str,
        deployment_name: str,
        api_version: str,
    ):
        """init deployment"""
        self.endpoint_location = endpoint_location
        self.endpoint_key = endpoint_key
        self.deployment_name = deployment_name
        self.api_version = api_version


class OpenAIConfig:
    """OpenAI Parameters"""

    def __init__(
        self,
        *,
        openai_version: str,
        deployments: list[dict[str, str]],
        config_connection_string: str,
    ):
        """init in memory config manager"""
        self.openai_version = openai_version
        self.deployments = deployments
        self.config_connection_string = config_connection_string

        self.round_robin = 0

    def get_deployment(self) -> Deployment:
        """get deployment by name"""

        index = int(self.round_robin % len(self.deployments))
        self.round_robin += 1

        deployment = self.deployments[index]
        endpoint_location = deployment["endpoint_location"]
        endpoint_key = deployment["endpoint_key"]
        deployment_name = deployment["deployment_name"]

        return Deployment(
            endpoint_location=endpoint_location,
            endpoint_key=endpoint_key,
            deployment_name=deployment_name,
            api_version=self.openai_version,
        )
