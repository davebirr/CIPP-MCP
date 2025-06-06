# sync-mcp-toolmap.ps1
# Auto-generates the static tool map section in McpToolMap.cs from cipp-api-openapi.json
# Usage: Run from the project root. Requires PowerShell 7+

$ErrorActionPreference = 'Stop'

$openApiPath = Join-Path $PSScriptRoot '..' 'cipp-api-openapi.json'
$toolMapPath = Join-Path $PSScriptRoot '..' 'McpToolMap.cs'

if (!(Test-Path $openApiPath)) {
    Write-Error "OpenAPI file not found: $openApiPath"
    exit 1
}
if (!(Test-Path $toolMapPath)) {
    Write-Error "McpToolMap.cs not found: $toolMapPath"
    exit 1
}

# Read OpenAPI spec
$openApi = Get-Content $openApiPath -Raw | ConvertFrom-Json

# Helper: Convert OpenAPI type to C# type (simple mapping)
function Get-CSharpType($openApiType) {
    switch ($openApiType) {
        'integer' { 'int' }
        'boolean' { 'bool' }
        default { 'string' }
    }
}

# Build tool entries
$toolEntries = @()
foreach ($path in $openApi.paths.PSObject.Properties) {
    $endpoint = $path.Name
    foreach ($method in $path.Value.PSObject.Properties) {
        $httpMethod = $method.Name.ToUpper()
        $enabled = if ($httpMethod -eq 'GET') { 'true' } else { 'false' }
        $meta = $method.Value
        $summary = $meta.summary
        $desc = $meta.description
        $tags = $meta.tags
        $toolName = if ($tags -and $tags.Count -gt 0) { $tags[0] } elseif ($summary) { $summary -replace '[^A-Za-z0-9]', '' } else { $endpoint.TrimStart('/') }
        $group = 'CIPP/Other'
        $params = @()
        if ($httpMethod -eq 'GET' -and $meta.parameters) {
            foreach ($p in $meta.parameters) {
                $pname = $p.name
                $ptype = Get-CSharpType $p.schema.type
                $preq = if ($p.required) { 'true' } else { 'false' }
                $pdesc = $p.description
                $params += "new ParameterRequirement(`"$pname`", `"$ptype`", $preq, `"$pdesc`")"
            }
        } elseif ($httpMethod -eq 'POST' -and $meta.requestBody.content.'application/json'.schema.properties) {
            $required = $meta.requestBody.content.'application/json'.schema.required
            foreach ($p in $meta.requestBody.content.'application/json'.schema.properties.PSObject.Properties) {
                $pname = $p.Name
                $ptype = Get-CSharpType $p.Value.type
                $preq = if ($required -and $required -contains $pname) { 'true' } else { 'false' }
                $pdesc = ''
                $params += "new ParameterRequirement(`"$pname`", `"$ptype`", $preq, `"$pdesc`")"
            }
        }
        $paramsList = if ($params.Count -gt 0) { "new List<ParameterRequirement> { $(($params -join ', ')) }" } else { 'new List<ParameterRequirement>()' }
        $descEscaped = $desc -replace '"', '\"'
        $httpMethodCSharp = if ($httpMethod -eq 'GET') { 'HttpMethod.Get' } elseif ($httpMethod -eq 'POST') { 'HttpMethod.Post' } elseif ($httpMethod -eq 'PUT') { 'HttpMethod.Put' } elseif ($httpMethod -eq 'DELETE') { 'HttpMethod.Delete' } else { 'new HttpMethod("' + $httpMethod + '")' }
        $toolEntries += "    { `"$toolName`", new ToolMetadata( `"$endpoint`", $httpMethodCSharp, `"$group`", $enabled, `"$descEscaped`", $paramsList ) },"
    }
}

# Read McpToolMap.cs and replace the auto-generated section
$csLines = Get-Content $toolMapPath
# Find section markers robustly (ignore indentation/whitespace)
$startIdx = -1
$endIdx = -1
for ($i = 0; $i -lt $csLines.Count; $i++) {
    if ($csLines[$i] -match 'BEGIN: AUTO-GENERATED FROM OpenAPI') { $startIdx = $i }
    if ($csLines[$i] -match 'END: AUTO-GENERATED FROM OpenAPI') { $endIdx = $i }
}
if ($startIdx -lt 0 -or $endIdx -lt 0 -or $endIdx -le $startIdx) {
    Write-Error "Could not find auto-generated section markers in McpToolMap.cs"
    exit 1
}

$before = $csLines[0..$startIdx]
$after = $csLines[$endIdx..($csLines.Count-1)]

$newSection = @('// --- BEGIN: AUTO-GENERATED FROM OpenAPI ---',
    '            // This section is auto-generated at build time from cipp-api-openapi.json.',
    '            // Each tool entry includes endpoint, method, group, enabled, description, and parameter requirements.',
    '            // DO NOT EDIT MANUALLY. To update, re-run the OpenAPI-to-toolmap sync script.'
) + $toolEntries + @('            // --- END: AUTO-GENERATED FROM OpenAPI ---')

$final = $before + $newSection + $after[1..($after.Count-1)]
Set-Content $toolMapPath -Value $final -Encoding UTF8

Write-Host "McpToolMap.cs static tool map updated from OpenAPI spec."
