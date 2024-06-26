""" OpenAI Embeddings API route """

from typing import Any

from app.config import Deployment
from app.routes.request_manager import RequestManager
from fastapi import Request, Response
from pydantic import RootModel

# pylint: disable=E0402
from ..openai_async import OpenAIAsyncManager


class AiSearchRequest(RootModel):
    """OpenAI Chat Request"""

    root: dict[str, Any]


class AzureAISearch(RequestManager):
    """Embeddings route"""

    def include_router(self):
        """include router"""

        # Search search service API with POST method
        # https://learn.microsoft.com/en-us/azure/search/search-get-started-rest#search-an-index
        @self.router.post(
            "/indexes/{index}/docs/search",
            status_code=200,
            response_model=None,
        )
        # Search service API with Alternate OData syntax
        # https://learn.microsoft.com/en-us/rest/api/searchservice/support-for-odata#search-service-api-with-alternate-odata-syntax
        @self.router.post(
            "/indexes('{index}')/docs/search.post.search",
            status_code=200,
            response_model=None,
        )
        async def azure_ai_search_post_search(
            search: AiSearchRequest,
            request: Request,
            response: Response,
            index: str,
        ) -> Any:
            """OpenAI chat completion response"""

            completion, status_code = await self.process_request(
                deployment_name=index,
                request=request,
                model=search.root,
                call_method=self.ai_search_post_search_request,
            )

            response.status_code = status_code
            return completion

        return self.router

    async def ai_search_post_search_request(
        self,
        search: object,
        deployment: Deployment,
        request: Request,
    ) -> Any:
        """call openai with retry"""

        url = (
            f"{deployment.endpoint_url}/indexes/"
            f"{deployment.deployment_name}/docs/search"
            f"?api-version={self.api_version}"
        )

        async_mgr = OpenAIAsyncManager(deployment)
        response, http_status_code = await async_mgr.async_openai_post(search, url)

        return response, http_status_code
