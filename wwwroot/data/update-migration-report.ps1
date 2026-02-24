param(
    [string]$RepoName,
    [string]$RepoSizeMB,
    [int]$SensitiveCount,
    [string]$Complexity
)

$JsonFile = "$PSScriptRoot\migrations_report.json"

if (Test-Path $JsonFile) {
    $Report = Get-Content $JsonFile -Raw | ConvertFrom-Json
} else {
    $Report = [PSCustomObject]@{
        title = "TFS to GitHub Migration Assessment Report"
        repositories = @()
    }
}

$Existing = $Report.repositories | Where-Object { $_.repoName -eq $RepoName }

if ($Existing) {
    $Existing.repoSizeMB = "$RepoSizeMB MB"
    $Existing.sensitiveDataOccurrences = $SensitiveCount
    $Existing.complexity = $Complexity
} else {
    $Report.repositories += [PSCustomObject]@{
        sno = $Report.repositories.Count + 1
        repoName = $RepoName
        repoSizeMB = "$RepoSizeMB MB"
        sensitiveDataOccurrences = $SensitiveCount
        complexity = $Complexity
        reportFile = "$RepoName.json"
    }
}

$Report | ConvertTo-Json -Depth 5 | Set-Content $JsonFile -Encoding UTF8
