$files = Get-ChildItem -Recurse -Include *.cs, *.ts, *.html, *.css, *.json, *.csproj, *.sln -Path "C:\task-management-ui" |
    Where-Object {
        $_.FullName -notmatch "node_modules|\\\\bin\\\\|\\\\obj\\\\|dist|\\\\.angular" -and
        $_.FullName -notlike "*package-lock*"
    } | Sort-Object FullName

$output = @()
foreach ($f in $files) {
    $output += "=== FILE: $($f.FullName) ==="
    $output += (Get-Content $f.FullName -Raw)
    $output += ""
}

$output | Out-File "C:\task-management-ui\codebase.txt" -Encoding UTF8
Write-Host "Done! Generated $($files.Count) files"
