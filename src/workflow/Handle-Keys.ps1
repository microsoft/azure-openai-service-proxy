# Update sensitive values in Power Automate workflows before checking in and out
Param(
    [string]
    [Parameter(Mandatory=$false)]
    $ManagementKey = "aoai-proxy-api-management-key",

    [switch]
    [Parameter(Mandatory=$false)]
    $IsLocal,

    [switch]
    [Parameter(Mandatory=$false)]
    $Help
)

function Show-Usage {
    Write-Output "    This updates sensitive values in Power Automate workflows before checking in and out

    Usage: $(Split-Path $MyInvocation.ScriptName -Leaf) ``
            [-ManagementKey <Management key>] ``
            [-IsLocal       <Switch indicating whether it's local or not>] ``

            [-Help]

    Options:
        -ManagementKey    Management key. Default is 'aoai-proxy-api-management-key'.
        -IsLocal          Switch indicating whether it's local or not.

        -Help:            Show this message.
"

    Exit 0
}

# Show usage
$needHelp = $Help -eq $true
if ($needHelp -eq $true) {
    Show-Usage
    Exit 0
}

$root = ($IsLocal -eq $true) ? $(git rev-parse --show-toplevel) : $env:GITHUB_WORKSPACE

$path = "$root/src/workflow/hacktogether/environmentvariabledefinitions/juyoo_ProxyApiManagementKey/environmentvariablevalues.json"
$environmentVariables = Get-Content $path | ConvertFrom-Json
$environmentVariables.environmentvariablevalues.environmentvariablevalue.value = $ManagementKey
$environmentVariables | ConvertTo-Json -Depth 100 | Out-File $path -Encoding utf8 -Force

$path = "$root/src/workflow/hacktogether/Workflows/HackTogetherAOAIProxyAccessCode-811B940D-2996-EE11-BE37-6045BD0554FA.json"
$workflow = Get-Content $path | ConvertFrom-Json
$workflow.properties.definition.parameters.'Proxy API Management Key (juyoo_ProxyApiManagementKey)'.defaultValue = $ManagementKey
$workflow | ConvertTo-Json -Depth 100 | Out-File $path -Encoding utf8 -Force
