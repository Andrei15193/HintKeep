function New-HmacKey {
    [OutputType([string])]
    param ()

    [byte[]] $buffer = New-Object byte[] 64
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($buffer)
    [string] $hmacKey = [Convert]::ToBase64String($buffer)

    return $hmacKey
}