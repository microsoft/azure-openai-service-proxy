# Import Power Automate Workflow Solution to Your Power Platform Environment

This shows a step-by-step instruction how to import a Power Automate Workflow Solution to your Power Platform environment.

## Prerequisites

- Your [Power Platform environment](https://learn.microsoft.com/power-platform/admin/environments-overview) to import the solution to
- [Power Platform CLI](https://learn.microsoft.com/power-platform/developer/cli/introduction) or [Power Platform CLI extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=microsoft-IsvExpTools.powerplatform-vscode)
- [PowerShell](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)

## Getting Started

1. Login to Power Platform environment using Power Platform CLI.

    ```powershell
    # This can be run in either bash/zsh or PowerShell
    pac auth create --name <your environment name> --environment <your environment ID>.crm.dynamics.com
    ```

1. Replace management key to the real one provided by admin.

    ```powershell
    # Run this in PowerShell
    ./src/workflow/Handle-Keys.ps1 -IsLocal -ManagementKey <your management key>
    ```

1. Package the solution.

    ```powershell
    # Run this in bash/zsh
    root=$(git rev-parse --show-toplevel)
    pac solution pack --zipfile "$root/src/workflow/hacktogether.zip" --folder "$root/src/workflow/hacktogether" --packagetype Unmanaged

    # Run this in PowerShell
    $root = $(git rev-parse --show-toplevel)
    pac solution pack --zipfile "$root/src/workflow/hacktogether.zip" --folder "$root/src/workflow/hacktogether" --packagetype Unmanaged
    ```

1. Import the solution.

    ```powershell
    # Run this in bash/zsh
    root=$(git rev-parse --show-toplevel)
    pac solution import --path "$root/src/workflow/hacktogether.zip" --force-overwrite

    # Run this in PowerShell
    $root = $(git rev-parse --show-toplevel)
    pac solution import --path "$root/src/workflow/hacktogether.zip" --force-overwrite
    ```

1. Go to the [Power Automate](https://make.powerautomate.com) portal and update the connection and Microsoft Froms references.

