[string] $azuriteConnectionString = 'DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;'

function Update-AzuriteTables {
    [OutputType([void])]
    param ()

    Write-Host "Ensuring Azure Storage Tables Exist"
    az bicep build --file (Path-Join $PSScriptRoot '..' '.infrastructure' 'environment.bicep') --stdout `
    | ConvertFrom-Json `
    | Select-Object -ExpandProperty variables `
    | Select-Object -ExpandProperty tableNames `
    | ForEach-Object {
        Write-Host "    Ensuring '$_' exists"

        az storage table create `
            --name $_ `
            --connection-string $azuriteConnectionString `
        | Out-Null
    }
    Write-Host "Ensured Azure Storage Tables Exist"
}