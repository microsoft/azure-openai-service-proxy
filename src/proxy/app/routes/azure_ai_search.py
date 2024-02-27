""" OpenAI Embeddings API route """

from typing import Any

from fastapi import Request, Response
from pydantic import RootModel

# pylint: disable=E0402
from ..config import Deployment
from ..openai_async import OpenAIAsyncManager
from .request_manager import RequestManager


class AiSearchRequest(RootModel):
    """OpenAI Chat Request"""

    root: dict[str, Any]


class AzureAISearch(RequestManager):
    """Embeddings route"""

    def include_router(self):
        """include router"""

        @self.router.post(
            "/indexes/{index}/docs/search",
            status_code=200,
            response_model=None,
        )
        # Python AI Search SDK support
        @self.router.post(
            "/indexes('{index}')/docs/search.post.search",
            status_code=200,
            response_model=None,
        )
        async def azure_ai_search_search(
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
                call_method=self.ai_search_search,
            )

            response.status_code = status_code
            return completion

        return self.router

    async def ai_search_search(
        self,
        search: object,
        deployment: Deployment,
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
