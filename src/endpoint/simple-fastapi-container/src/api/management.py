""" Management API"""
import logging
import uuid
from datetime import datetime
from enum import Enum
from pydantic import BaseModel
from fastapi import HTTPException
from azure.core.exceptions import AzureError
from azure.data.tables import TableClient
from azure.data.tables.aio import TableClient as AsyncTableClient


logging.basicConfig(level=logging.WARNING)

MANAGEMENT_TABLE_NAME = "management"
EVENT_AUTHORIZATION_TABLE_NAME = "authorization"
EVENT_AUTHORISATION_PARTITION_KEY = "event"
CONFIGURATION_TABLE_NAME = "configuration"


# create a model type enum
class DeploymentClass(Enum):
    """Deployment Class."""

    OPENAI_CHAT = "openai-chat"
    OPENAI_COMPLETIONS = "openai-completions"
    OPENAI_EMBEDDINGS = "openai-embeddings"
    OPENAI_IMAGES_GENERATIONS = "openai-images-generations"
    OPENAI_IMAGES = "openai-images"


class ModelDeploymentRequest(BaseModel):
    """Model Deployment Request."""

    deployment_class: DeploymentClass
    friendly_name: str
    deployment_name: str
    endpoint_key: str
    resource_name: str
    active: bool = True


class ModelDeploymentResponse(BaseModel):
    """Model Deployment Response."""

    deployment_class: DeploymentClass
    friendly_name: str
    deployment_name: str
    endpoint_key: str
    resource_name: str
    active: bool = True

    def __init__(
        self,
        deployment_class: DeploymentClass,
        friendly_name: str,
        deployment_name: str,
        endpoint_key: str,
        resource_name: str,
        active: bool,
    ) -> None:
        super().__init__(
            deployment_class=deployment_class,
            friendly_name=friendly_name,
            deployment_name=deployment_name,
            endpoint_key=endpoint_key,
            resource_name=resource_name,
            active=active,
        )


class EventItemResponse(BaseModel):
    """Event item object for Authorize class."""

    event_name: str
    start_utc: datetime
    end_utc: datetime
    max_token_cap: int
    event_url: str
    event_url_text: str
    organizer_name: str
    organizer_email: str
    active: bool

    def __init__(
        self,
        event_name: str,
        start_utc: datetime,
        end_utc: datetime,
        max_token_cap: int,
        event_url: str,
        event_url_text: str,
        organizer_name: str,
        organizer_email: str,
        active: bool,
    ) -> None:
        super().__init__(
            event_name=event_name,
            start_utc=start_utc,
            end_utc=end_utc,
            max_token_cap=max_token_cap,
            event_url=event_url,
            event_url_text=event_url_text,
            organizer_name=organizer_name,
            organizer_email=organizer_email,
            active=active,
        )


class NewEventRequest(BaseModel):
    """New event object for Authorize class."""

    event_name: str
    start_utc: datetime
    end_utc: datetime
    max_token_cap: int
    event_url: str
    event_url_text: str
    organizer_name: str
    organizer_email: str

    def __init__(
        self,
        event_name: str,
        start_utc: datetime,
        end_utc: datetime,
        max_token_cap: int,
        event_url: str,
        event_url_text: str,
        organizer_name: str,
        organizer_email: str,
    ) -> None:
        super().__init__(
            event_name=event_name,
            start_utc=start_utc,
            end_utc=end_utc,
            max_token_cap=max_token_cap,
            event_url=event_url,
            event_url_text=event_url_text,
            organizer_name=organizer_name,
            organizer_email=organizer_email,
        )


# create a new EventResponse class
class NewEventResponse(BaseModel):
    """Response object for Authorize class."""

    event_code: str
    organizer_name: str
    organizer_email: str
    start_utc: datetime
    end_utc: datetime
    event_url: str
    event_name: str


class ManagementService:
    """Management class for the API"""

    def __init__(self, connection_string: str):
        self.connection_string = connection_string
        self.logging = logging.getLogger(__name__)
        # check the management table exists if not, create it
        self._create_table()

    def _create_table(self):
        """Create the management table if it does not exist"""

        # Create management authorization table if it does not exist
        try:
            # check if there is a row in the table with a partition key of "management"
            table_client = TableClient.from_connection_string(
                conn_str=self.connection_string, table_name=MANAGEMENT_TABLE_NAME
            )

            with TableClient.from_connection_string(
                conn_str=self.connection_string, table_name=MANAGEMENT_TABLE_NAME
            ) as table_client:
                query_filter = "PartitionKey eq 'management'"
                # get all columns from the table
                queried_entities = table_client.query_entities(
                    query_filter=query_filter,
                    select=[
                        "*",
                    ],
                )

                # read the first row
                partition_key = None
                for entity in queried_entities:
                    partition_key = entity["PartitionKey"]

                if partition_key is None:
                    # create a new row with a partition key of "management" and a row key of of uuid
                    row_key = str(uuid.uuid4())

                    event_entity = {
                        "PartitionKey": "management",
                        "RowKey": row_key,
                        "Active": True,
                    }

                    # Add the event request to the table
                    table_client.create_entity(entity=event_entity)

        except AzureError as azure_error:
            self.logging.error(
                "HTTP response exception adding event request: %s", azure_error.message
            )
            raise HTTPException(
                status_code=504,
                detail="Create management authorization table failed. AzureError.",
            ) from azure_error

        except Exception as exception:
            self.logging.error(
                "General exception creating table: %s", exception.args[0]
            )
            raise HTTPException(
                status_code=500,
                detail="Create management authorization table failed.",
            ) from exception

    async def add_new_event(self, event_request: NewEventRequest):
        """Add an event request to the management table"""

        row_key = str(uuid.uuid4())

        event_entity = {
            "PartitionKey": EVENT_AUTHORISATION_PARTITION_KEY,
            "RowKey": row_key,
            "Active": True,
            "EventName": event_request.event_name,
            "StartUTC": event_request.start_utc,
            "EndUTC": event_request.end_utc,
            "MaxTokenCap": event_request.max_token_cap,
            "EventUrl": event_request.event_url,
            "EventUrlText": event_request.event_url_text,
            "OrganizerName": event_request.organizer_name,
            "OrganizerEmail": event_request.organizer_email,
        }

        try:
            table_client = TableClient.from_connection_string(
                conn_str=self.connection_string,
                table_name=EVENT_AUTHORIZATION_TABLE_NAME,
            )

            # Add the event request to the table
            table_client.create_entity(entity=event_entity)

            # create and return a NewEventResponse
            return NewEventResponse(
                event_code=row_key,
                organizer_name=event_request.organizer_name,
                organizer_email=event_request.organizer_email,
                start_utc=event_request.start_utc,
                end_utc=event_request.end_utc,
                event_name=event_request.event_name,
                event_url=event_request.event_url,
            )

        except AzureError as azure_error:
            self.logging.error(
                "HTTP response exception adding event request: %s", azure_error.message
            )
            raise HTTPException(
                status_code=504,
                detail="Add event request failed. AzureError.",
            ) from azure_error

        except Exception as exception:
            self.logging.error(
                "General exception adding event request: %s", exception.args[0]
            )
            raise HTTPException(
                status_code=500,
                detail="Add event request failed.",
            ) from exception

    async def list_events(self, query: str):
        """Get an event request from the management table"""

        try:
            async with AsyncTableClient.from_connection_string(
                conn_str=self.connection_string,
                table_name=EVENT_AUTHORIZATION_TABLE_NAME,
            ) as table_client:
                events = []
                current_utc = datetime.utcnow().isoformat()
                query = query.lower()

                if query == "all":
                    query_filter = "PartitionKey eq 'event'"
                elif query == "active":
                    query_filter = (
                        f"PartitionKey eq 'event' and Active eq true "
                        f"and StartUTC le datetime'{current_utc}' "
                        f"and EndUTC ge datetime'{current_utc}'"
                    )
                else:
                    raise HTTPException(
                        status_code=400,
                        detail="Invalid query path.",
                    )

                # get all columns from the table
                queried_entities = table_client.query_entities(
                    query_filter=query_filter,
                    select=[
                        "*",
                    ],
                )

                async for entity in queried_entities:
                    event_items = EventItemResponse(
                        event_name=entity.get("EventName", "").strip(),
                        start_utc=entity.get("StartUTC", ""),
                        end_utc=entity.get("EndUTC", ""),
                        max_token_cap=entity.get("MaxTokenCap", ""),
                        event_url=entity.get("EventUrl", "").strip(),
                        event_url_text=entity.get("EventUrlText", "").strip(),
                        organizer_name=entity.get("OrganizerName", "").strip(),
                        organizer_email=entity.get("OrganizerEmail", "").strip(),
                        active=entity.get("Active"),
                    )

                    events.append(event_items)

                return events

        except HTTPException:
            raise

        except AzureError as azure_error:
            self.logging.error(
                "HTTP response exception getting event request: %s", azure_error.message
            )
            raise HTTPException(
                status_code=504,
                detail="Get event request failed. AzureError.",
            ) from azure_error

        except Exception as exception:
            self.logging.error(
                "General exception getting event request: %s", exception.args[0]
            )
            raise HTTPException(
                status_code=500,
                detail="Get event request failed.",
            ) from exception

    async def upsert_model_deployment(self, model_deployment: ModelDeploymentRequest):
        """Upsert a model deployment to the configuration table"""

        model_entity = {
            "PartitionKey": model_deployment.deployment_class.value,
            "RowKey": model_deployment.friendly_name,
            "DeploymentName": model_deployment.deployment_name,
            "Active": model_deployment.active,
            "EndpointKey": model_deployment.endpoint_key,
            "ResourceName": model_deployment.resource_name,
        }

        try:
            table_client = TableClient.from_connection_string(
                conn_str=self.connection_string,
                table_name=CONFIGURATION_TABLE_NAME,
            )

            table_client.upsert_entity(entity=model_entity)

        except AzureError as azure_error:
            self.logging.error(
                "HTTP response exception adding model deployment: %s",
                azure_error.message,
            )
            raise HTTPException(
                status_code=504,
                detail="Add model deployment failed. AzureError.",
            ) from azure_error

        except Exception as exception:
            self.logging.error(
                "General exception adding model deployment: %s", exception.args[0]
            )
            raise HTTPException(
                status_code=500,
                detail="Add model deployment failed.",
            ) from exception

    async def list_model_deployments(self, query: str):
        """list model deployments from the configuration table"""

        try:
            async with AsyncTableClient.from_connection_string(
                conn_str=self.connection_string,
                table_name=CONFIGURATION_TABLE_NAME,
            ) as table_client:
                model_deployments = []
                query = query.lower()

                if query == "all":
                    query_filter = ""
                elif query == "active":
                    query_filter = "Active eq true"
                else:
                    raise HTTPException(
                        status_code=400,
                        detail="Invalid query path.",
                    )

                # get all columns from the table
                queried_entities = table_client.query_entities(
                    query_filter=query_filter,
                    select=[
                        "*",
                    ],
                )

                async for entity in queried_entities:
                    try:
                        deployment_class = DeploymentClass(entity.get("PartitionKey"))

                        model_deployment = ModelDeploymentResponse(
                            deployment_class=deployment_class,
                            friendly_name=entity.get("RowKey", "").strip(),
                            deployment_name=entity.get("DeploymentName", "").strip(),
                            endpoint_key="*********",
                            resource_name=entity.get("ResourceName", "").strip(),
                            active=entity.get("Active"),
                        )

                        model_deployments.append(model_deployment)
                    except ValueError:
                        self.logging.error(
                            "Invalid deployment class: %s", entity.get("PartitionKey")
                        )

                return model_deployments

        except HTTPException:
            raise

        except AzureError as azure_error:
            self.logging.error(
                "HTTP response exception getting model deployment: %s",
                azure_error.message,
            )
            raise HTTPException(
                status_code=504,
                detail="Get model deployment failed. AzureError.",
            ) from azure_error

        except Exception as exception:
            self.logging.error(
                "General exception getting model deployment: %s", exception.args[0]
            )
            raise HTTPException(
                status_code=500,
                detail="Get model deployment failed.",
            ) from exception
