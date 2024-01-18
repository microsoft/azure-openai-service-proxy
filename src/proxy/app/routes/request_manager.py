""" Request Manager base class """

import logging
from collections.abc import AsyncGenerator

from fastapi import APIRouter, HTTPException, Request

# pylint: disable=E0402
from ..authorize import Authorize, AuthorizeResponse
from ..config import Config
from ..rate_limit import RateLimit

logging.basicConfig(level=logging.INFO)


class RequestManager:
    """Request Manager base class"""

    def __init__(self, *, authorize: Authorize, config: Config, api_version: str):
        self.authorize = authorize
        self.config = config
        self.api_version = api_version

        self.router = APIRouter()
        self.rate_limit = RateLimit()
        self.logger = logging.getLogger(__name__)
        self._is_extension = False

    @property
    def is_extension(self):
        """Getter for is_extension"""
        return self._is_extension

    @is_extension.setter
    def is_extension(self, value):
        """Setter for is_extension"""
        if isinstance(value, bool):
            self._is_extension = value
        else:
            raise ValueError("is_extension must be a boolean")

    async def authorize_request(self, deployment_name: str, request: Request) -> AuthorizeResponse:
        """authorize request"""

        authorize_response = await self.authorize.authorize_azure_api_access(
            headers=request.headers, deployment_name=deployment_name
        )

        if self.rate_limit.is_call_rate_exceeded(authorize_response.user_id):
            raise HTTPException(
                status_code=429,
                detail="Rate limit exceeded. Try again in 10 seconds",
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
            if model.max_tokens is not None and model.max_tokens > authorize_response.max_token_cap:
                model.max_tokens = authorize_response.max_token_cap

        if self.rate_limit.is_call_rate_exceeded(authorize_response.user_id):
            raise HTTPException(
                status_code=429,
                detail="Rate limit exceeded. Try again in 10 seconds",
            )

        deployment = await self.config.get_catalog_by_deployment_name(authorize_response)
        response, http_status_code = await call_method(model, deployment)

        if not isinstance(response, AsyncGenerator) and "model" in response:
            response = self.model_to_dict(response)
            response["model"] = response["model"] + ":" + deployment.location.lower()

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
