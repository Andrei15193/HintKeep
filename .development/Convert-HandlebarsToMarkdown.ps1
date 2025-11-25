function Convert-HandlebarsToMarkdown
{
    [OutputType([void])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $TemplatesDirectory,

        [Parameter(Mandatory = $true)]
        [string] $DestinationDirectory
    )

    New-Item $DestinationDirectory `
        -ItemType Directory `
        -ErrorAction SilentlyContinue `
    | Out-Null

    dotnet run `
        --project (Join-Path $PSScriptRoot '..' '.docs' 'HandlebarsMarkdownWriter') `
        --no-launch-profile `
        (Resolve-Path $TemplatesDirectory) `
        (Resolve-Path $DestinationDirectory)
}