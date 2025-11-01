function Convert-FeaturesToMarkdown
{
    [OutputType([void])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $SourceUrl,

        [Parameter(Mandatory = $true)]
        [string] $SourceDirectory,

        [Parameter(Mandatory = $true)]
        [string] $DestinationDirectory
    )

    New-Item $DestinationDirectory `
        -ItemType Directory `
        -ErrorAction SilentlyContinue `
    | Out-Null

    dotnet run `
        --project (Join-Path $PSScriptRoot '..' '.docs' 'FeatureMarkdownWriter') `
        --no-launch-profile `
        $SourceUrl `
        (Resolve-Path $SourceDirectory) `
        (Resolve-Path $DestinationDirectory)
}