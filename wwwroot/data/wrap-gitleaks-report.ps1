param(
    [string]$RepoName,
    [string]$ReportPath
)

if (!(Test-Path $ReportPath)) {
    Write-Error "Gitleaks report not found: $ReportPath"
    exit 1
}

$raw = Get-Content $ReportPath -Raw | ConvertFrom-Json

$wrapped = [PSCustomObject]@{
    repoName = $RepoName
    scanDate = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    findings = $raw
}

$wrapped | ConvertTo-Json -Depth 10 | Set-Content $ReportPath -Encoding UTF8
