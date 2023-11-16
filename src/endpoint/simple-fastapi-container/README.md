This repository includes a simple Python FastAPI app with a single route that returns JSON.
You can use this project as a starting point for your own APIs.

The repository is designed for use with [Docker containers](https://www.docker.com/), both for local development and deployment, and includes infrastructure files for deployment to [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/overview). 🐳

The code istested with [pytest](https://docs.pytest.org/en/7.2.x/),
linted with [ruff](https://github.com/charliermarsh/ruff), and formatted with [black](https://black.readthedocs.io/en/stable/).
Code quality issues are all checked with both [pre-commit](https://pre-commit.com/) and Github actions.

## Opening the project

This project has [Dev Container support](https://code.visualstudio.com/docs/devcontainers/containers), so it will be be setup automatically if you open it in Github Codespaces or in local VS Code with the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers).

If you're not using one of those options for opening the project, then you'll need to:

1. Create a [Python virtual environment](https://docs.python.org/3/tutorial/venv.html#creating-virtual-environments) and activate it.

2. Install the requirements:

    ```shell
    python3 -m pip install -r requirements-dev.txt
    ```

3. Install the pre-commit hooks:

    ```shell
    pre-commit install
    ```

## Local development

1. Run the local server:

    ```shell
    uvicorn src.api.main:app --port 3100 --reload
    ```

3. Click 'http://127.0.0.1:3100' in the terminal, which should open a new tab in the browser.

4. Try the API at '/generate_name' and try passing in a parameter at the end of the URL, like '/generate_name?start_with=N'.

### Local development with Docker

You can also run this app with Docker, thanks to the `Dockerfile`.

You need to either have Docker Desktop installed or have this open in Github Codespaces for these commands to work. ⚠️ If you're on an Apple M1/M2, you won't be able to run `docker` commands inside a Dev Container; either use Codespaces or do not open the Dev Container.

1. Build the image:

    ```
    docker build --tag fastapi-app ./src
    ```

2. Run the image:

    ```
    docker run --publish 3100:3100 fastapi-app
    ```

### Deployment

This repo is set up for deployment on Azure Container Apps using the configuration files in the `infra` folder.

This diagram shows the architecture of the deployment:

![Diagram of app architecture: Azure Container Apps environment, Azure Container App, Azure Container Registry, Container, and Key Vault](readme_diagram.png)

Steps for deployment:

1. Sign up for a [free Azure account](https://azure.microsoft.com/free/) and create an Azure Subscription.
2. Install the [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd). (If you open this repository in Codespaces or with the VS Code Dev Containers extension, that part will be done for you.)
3. Login to Azure:

    ```shell
    azd auth login
    ```

4. Provision and deploy all the resources:

    ```shell
    azd up
    ```
    It will prompt you to provide an `azd` environment name (like "fastapi-app"), select a subscription from your Azure account, and select a location (like "eastus"). Then it will provision the resources in your account and deploy the latest code. If you get an error with deployment, changing the location can help, as there may be availability constraints for some of the resources.

5. When `azd` has finished deploying, you'll see an endpoint URI in the command output. Visit that URI, and you should see the API output! 🎉
6. When you've made any changes to the app code, you can just run:

    ```shell
    azd deploy
    ```

### Costs

Pricing varies per region and usage, so it isn't possible to predict exact costs for your usage.
The majority of the Azure resources used in this infrastructure are on usage-based pricing tiers.
However, Azure Container Registry has a fixed cost per registry per day.

You can try the [Azure pricing calculator](https://azure.com/e/9f8185b239d240b398e201078d0c4e7a) for the resources:

- Azure Container App: Consumption tier with 0.5 CPU, 1GiB memory/storage. Pricing is based on resource allocation, and each month allows for a certain amount of free usage. [Pricing](https://azure.microsoft.com/pricing/details/container-apps/)
- Azure Container Registry: Basic tier. [Pricing](https://azure.microsoft.com/pricing/details/container-registry/)
- Log analytics: Pay-as-you-go tier. Costs based on data ingested. [Pricing](https://azure.microsoft.com/pricing/details/monitor/)

⚠️ To avoid unnecessary costs, remember to take down your app if it's no longer in use,
either by deleting the resource group in the Portal or running `azd down`.


## Getting help

If you're working with this project and running into issues, please post in **Discussions**.
