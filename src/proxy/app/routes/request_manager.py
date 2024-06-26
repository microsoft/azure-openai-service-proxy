""" Request Manager base class """

import json
import logging
from collections.abc import AsyncGenerator

from app.authorize import Authorize, AuthorizeResponse
from app.config import Config
from fastapi import APIRouter, HTTPException, Request

logging.basicConfig(level=logging.INFO)


class RequestManager:
    """Request Manager base class"""

    def __init__(self, *, authorize: Authorize, config: Config, api_version: str):
        self.authorize = authorize
        self.config = config
        self.api_version = api_version

        self.router = APIRouter()
        self.logger = logging.getLogger(__name__)

    async def authorize_request(self, deployment_name: str, request: Request) -> AuthorizeResponse:
        """authorize request"""

        authorize_response = await self.authorize.authorize_azure_api_access(
            headers=request.headers, deployment_name=deployment_name
        )

        return authorize_response

    async def process_request(
        self,
        *,
        deployment_name: str,
        request: Request,
        model: object,
        call_method: classmethod,
        validate_method: classmethod = None,
    ):
        """authorize request"""
        if validate_method:
            validate_method(model)

        if "api-version" in request.query_params:
            self.api_version = request.query_params["api-version"]

        authorize_response = await self.authorize.authorize_azure_api_access(
            headers=request.headers, deployment_name=deployment_name
        )

        if hasattr(model, "max_tokens"):
            if (
                model.max_tokens is not None
                and authorize_response.max_token_cap > 0
                and model.max_tokens > authorize_response.max_token_cap
            ):
                model.max_tokens = authorize_response.max_token_cap

        deployment = await self.config.get_catalog_by_deployment_name(authorize_response)
        response, http_status_code = await call_method(model, deployment, request)

        if not isinstance(response, AsyncGenerator) and "model" in response:
            response = self.model_to_dict(response)
            response["model"] = response["model"] + ":" + deployment.location.lower()

        if isinstance(response, AsyncGenerator):
            # there is no response object for streaming so set the usage to stream
            authorize_response.usage = '{"stream": true}'
        else:
            authorize_response.usage = json.dumps(response.get("usage", {}))

        await self.config.monitor.log_api_call(entity=authorize_response)

        return response, http_status_code

    def throw_validation_error(self, message: str, status_code: int):
        """throw validation error"""
        raise HTTPException(
            status_code=status_code,
            detail=message,
        )

    def report_exception(self, message: str, http_status_code: int) -> HTTPException:
        """report exception"""

        self.logger.warning(msg=f"{message}")

        raise HTTPException(
            status_code=http_status_code,
            detail=message,
        )

    def model_to_dict(self, model: object) -> dict:
        """model to dict and remove keys that have None values"""
        return {k: v for k, v in dict(model).items() if v is not None}
