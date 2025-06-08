# Basic MCP HTTP Test Sequence with Colorful Output
# This script demonstrates how to:
# 1. Query available tools (POST)
# 2. Call a tool (POST)
# 3. Close the session (DELETE)

$baseUrl = "http://localhost:3001/mcp"

$headers = @{
    "Accept"       = "application/json, text/event-stream"
    "Content-Type" = "application/json"
}

function Write-Success($msg) {
    Write-Host $msg -ForegroundColor Green
}
function Write-ErrorMsg($msg) {
    Write-Host $msg -ForegroundColor Red
}
function Write-WarningMsg($msg) {
    Write-Host $msg -ForegroundColor Yellow
}
function Write-Info($msg) {
    Write-Host $msg -ForegroundColor Cyan
}

# 1. Query available tools
Write-Info "Querying available tools..."
$listToolsBody = '{"jsonrpc":"2.0","method":"tools/list","params":{},"id":1}'
try {
    $rawResponse = Invoke-WebRequest -Uri $baseUrl -Method Post -Headers $headers -Body $listToolsBody -UseBasicParsing | Select-Object -ExpandProperty Content
    Write-Host "Raw response:" -ForegroundColor DarkGray
    Write-Host $rawResponse -ForegroundColor DarkGray
    # Extract JSON from SSE (data: ...)
    if ($rawResponse -match 'data: (\{.*\})') {
        $json = $matches[1]
        $listToolsResponse = $json | ConvertFrom-Json
    }
    else {
        Write-ErrorMsg "Could not extract JSON from SSE response."
        exit 1
    }
    if ($listToolsResponse.result -and $listToolsResponse.result.tools.Count -gt 0) {
        Write-Success "Available tools:"
        foreach ($tool in $listToolsResponse.result.tools) {
            Write-Host ("- " + $tool.name + ": " + $tool.description) -ForegroundColor White
            if ($tool.inputSchema) {
                Write-Host ("  Input schema:") -ForegroundColor DarkGray
                Write-Host ($tool.inputSchema | ConvertTo-Json -Depth 10) -ForegroundColor DarkGray
            }
        }
    }
    else {
        Write-WarningMsg "No tools available to call."
    }
}
catch {
    Write-ErrorMsg "Failed to query tools: $($_.Exception.Message)"
    exit 1
}

# Example: Call the first tool if available
if ($listToolsResponse.result -and $listToolsResponse.result.tools.Count -gt 0) {
    $firstTool = $listToolsResponse.result.tools[0]
    Write-Info "\nCalling first tool: $($firstTool.name)"
    # Build a dummy arguments object based on the inputSchema (if any)
    $args = @{}
    if ($firstTool.inputSchema -and $firstTool.inputSchema.properties) {
        foreach ($prop in $firstTool.inputSchema.properties.PSObject.Properties) {
            $propName = $prop.Name
            $propType = $prop.Value.type
            # Provide a dummy value based on type
            switch ($propType) {
                "string" { $args[$propName] = "test" }
                "number" { $args[$propName] = 1 }
                default { $args[$propName] = $null }
            }
        }
    }
    $callBody = @{ jsonrpc = "2.0"; method = "tools/call"; params = @{ name = $firstTool.name; arguments = $args }; id = 2 } | ConvertTo-Json -Compress
    try {
        $callResponse = Invoke-RestMethod -Uri $baseUrl -Method Post -Headers $headers -Body $callBody
        Write-Success "Tool call response:"
        Write-Host ($callResponse | ConvertTo-Json -Depth 10) -ForegroundColor White
    }
    catch {
        Write-ErrorMsg "Tool call failed: $($_.Exception.Message)"
    }
}

# 2. Call monkey tool: GetMonkeys (list)
Write-Info "\nCalling monkey tool: GetMonkeys (list)"
$monkeyListBody = @{ jsonrpc = "2.0"; method = "tools/call"; params = @{ name = "monkey"; arguments = @{ action = "list" } }; id = 2 } | ConvertTo-Json -Compress
try {
    $monkeyListRaw = Invoke-WebRequest -Uri $baseUrl -Method Post -Headers $headers -Body $monkeyListBody -UseBasicParsing | Select-Object -ExpandProperty Content
    Write-Host "Raw response:" -ForegroundColor DarkGray
    Write-Host $monkeyListRaw -ForegroundColor DarkGray
    if ($monkeyListRaw -match 'data: (\{.*\})') {
        $monkeyListJson = $matches[1] | ConvertFrom-Json
        Write-Success "Monkey list response:"
        Write-Host ($monkeyListJson | ConvertTo-Json -Depth 10) -ForegroundColor White
        # Try to parse the monkey list for the next call
        $monkeyArray = @()
        if ($monkeyListJson.result -and $monkeyListJson.result.content -and $monkeyListJson.result.content.Count -gt 0) {
            $content = $monkeyListJson.result.content[0]
            if ($content.type -eq "application/json" -and $content.text) {
                $monkeyArray = $content.text | ConvertFrom-Json
            }
        }
        if ($monkeyArray.Count -gt 0) {
            $randomMonkey = Get-Random -InputObject $monkeyArray
            $monkeyName = $randomMonkey.Name
            Write-Info "\nCalling monkey tool: GetMonkey (get) for random monkey '$monkeyName'"
            $monkeyGetBody = @{ jsonrpc = "2.0"; method = "tools/call"; params = @{ name = "monkey"; arguments = @{ action = "get"; name = $monkeyName } }; id = 3 } | ConvertTo-Json -Compress
            $monkeyGetRaw = Invoke-WebRequest -Uri $baseUrl -Method Post -Headers $headers -Body $monkeyGetBody -UseBasicParsing | Select-Object -ExpandProperty Content
            Write-Host "Raw response:" -ForegroundColor DarkGray
            Write-Host $monkeyGetRaw -ForegroundColor DarkGray
            if ($monkeyGetRaw -match 'data: (\{.*\})') {
                $monkeyGetJson = $matches[1] | ConvertFrom-Json
                Write-Success "Monkey get response:"
                Write-Host ($monkeyGetJson | ConvertTo-Json -Depth 10) -ForegroundColor White
            }
            else {
                Write-ErrorMsg "Could not extract JSON from SSE response for GetMonkey."
            }
        }
        else {
            Write-WarningMsg "No monkeys found in list response."
        }
    }
    else {
        Write-ErrorMsg "Could not extract JSON from SSE response for GetMonkeys."
    }
}
catch {
    Write-ErrorMsg "Monkey tool call failed: $($_.Exception.Message)"
}

# 3. Close the session (DELETE)
Write-Info "Closing MCP session..."
try {
    $closeResponse = Invoke-RestMethod -Uri $baseUrl -Method Delete
    Write-Success "Session closed: $($closeResponse | ConvertTo-Json)"
}
catch {
    Write-WarningMsg "Session close returned error (this is normal if no session was open): $($_.Exception.Message)"
}
