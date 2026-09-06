$project = Join-Path $PSScriptRoot "AgentsDashboard.csproj"
$url = "https://localhost:5001"

Start-Process dotnet `
    -ArgumentList "run --project `"$project`" --urls `"$url`"" `
    -WindowStyle Hidden

for ($i = 0; $i -lt 30; $i++) {
    try {
        $response = Invoke-WebRequest $url -SkipCertificateCheck -UseBasicParsing -TimeoutSec 1
        break
    }
    catch {
        Start-Sleep -Milliseconds 500
    }
}


Start-Process "chrome.exe" $url