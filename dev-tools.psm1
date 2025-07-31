Get-ChildItem (Join-Path $PSScriptRoot '.development') -File -Recurse -Include '*.ps1' `
| ForEach-Object {
    [string] $scriptFileLocation = $_.FullName
    . $scriptFileLocation
}

Export-ModuleMember -Function (
    Get-Command -CommandType Function `
    | Where-Object { $_.ScriptBlock.File -ilike (Join-Path $PSScriptRoot '.development' '*') } `
)