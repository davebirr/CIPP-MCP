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

function Prompt-ForToolSelection($tools) {
    Write-Host "\nWhich tool would you like to invoke?" -ForegroundColor Cyan
    for ($i = 0; $i -lt $tools.Count; $i++) {
        Write-Host ("[$i] " + $tools[$i].name + ": " + $tools[$i].description) -ForegroundColor White
    }
    Write-Host ("[x] Exit") -ForegroundColor Yellow
    $choice = Read-Host "Enter the number of the tool to invoke, or 'x' to exit"
    return $choice
}

Write-Host "\nWelcome to the MCP Monkey Tool Test Script!" -ForegroundColor Green
Write-Host "This script will help you explore the available tools and monkey data." -ForegroundColor Cyan

# Query available tools (once, at the start)
Write-Info "\nQuerying available tools..."
$listToolsBody = '{"jsonrpc":"2.0","method":"tools/list","params":{},"id":1}'
try {
    $rawResponse = Invoke-WebRequest -Uri $baseUrl -Method Post -Headers $headers -Body $listToolsBody -UseBasicParsing | Select-Object -ExpandProperty Content
    if ($rawResponse -match 'data: (\{.*\})') {
        $json = $matches[1]
        $listToolsResponse = $json | ConvertFrom-Json
    }
    else {
        Write-ErrorMsg "Could not extract JSON from SSE response."
        exit 1
    }
    if ($listToolsResponse.result -and $listToolsResponse.result.tools.Count -gt 0) {
        $tools = $listToolsResponse.result.tools
        Write-Success "Available tools:"
        foreach ($tool in $tools) {
            Write-Host ("- " + $tool.name + ": " + $tool.description) -ForegroundColor White
            if ($tool.inputSchema) {
                Write-Host ("  Input schema:") -ForegroundColor DarkGray
                Write-Host ($tool.inputSchema | ConvertTo-Json -Depth 10) -ForegroundColor DarkGray
            }
        }
    }
    else {
        Write-WarningMsg "No tools available to call."
        exit 1
    }
}
catch {
    Write-ErrorMsg "Failed to query tools: $($_.Exception.Message)"
    exit 1
}

Write-Host "\nPress any key to see all monkeys..." -ForegroundColor Yellow
[void][System.Console]::ReadKey($true)

# Call the 'monkey' tool with action 'list' to get all monkeys
$monkeyTool = $tools | Where-Object { $_.name -eq 'monkey' }
if ($null -eq $monkeyTool) {
    Write-ErrorMsg "Monkey tool not found."
    exit 1
}
$callBody = @{ jsonrpc = "2.0"; method = "tools/call"; params = @{ name = 'monkey'; arguments = @{ action = 'getmonkeys' } }; id = 2 } | ConvertTo-Json -Compress
try {
    $callRaw = Invoke-WebRequest -Uri $baseUrl -Method Post -Headers $headers -Body $callBody -UseBasicParsing | Select-Object -ExpandProperty Content
    Write-Host "\nAll monkeys:" -ForegroundColor Cyan
    if ($callRaw -match 'data: (\{.*\})') {
        $callJson = $matches[1] | ConvertFrom-Json
        # Try to extract the list of monkeys from the response
        if ($callJson.result -and $callJson.result.content) {
            $monkeyList = $null
            try {
                $monkeyList = $callJson.result.content[0].text | ConvertFrom-Json
            }
            catch {}
            if ($monkeyList -and $monkeyList.Count -gt 0) {
                $monkeyList | Format-Table Name, Species, Age, Bananas, InterestingFact -AutoSize
            }
            else {
                Write-Host ($callJson | ConvertTo-Json -Depth 10) -ForegroundColor White
            }
        }
        else {
            Write-Host ($callJson | ConvertTo-Json -Depth 10) -ForegroundColor White
        }
    }
    else {
        Write-ErrorMsg "Could not extract JSON from SSE response for tool call."
    }
}
catch {
    Write-ErrorMsg "Tool call failed: $($_.Exception.Message)"
}

# After showing all monkeys, dynamically prompt for actions based on inputSchema

# Get possible actions hint from inputSchema
$actionHint = $null
$actionProp = $monkeyTool.inputSchema.properties.action
if ($actionProp -and $actionProp.description) {
    $actionHint = $actionProp.description
}

# Dynamically discover available actions from MonkeyTools methods (simulate by hardcoding for now, or fetch from backend if available)
$validActions = @('getmonkeys', 'getmonkey', 'getbananacount', 'getrandomfact', 'givebanana')

# Ask user if they want raw output mode
$rawMode = $false
$rawPrompt = Read-Host "Do you want to see raw JSON output by default? (y/N)"
if ($rawPrompt -eq 'y' -or $rawPrompt -eq 'Y') { $rawMode = $true }

while ($true) {
    Write-Host "\nWhat would you like to do next?" -ForegroundColor Cyan
    Write-Host ("Available actions: " + ($validActions -join ', ')) -ForegroundColor DarkGray
    Write-Host ("Raw output mode: " + ($rawMode ? 'ON' : 'OFF')) -ForegroundColor Yellow
    $action = Read-Host "Enter an action (or 'x' to exit, or 'raw' to toggle raw mode)"
    if ($action -eq 'x') { break }
    if ($action -eq 'raw') { $rawMode = -not $rawMode; Write-Host ("Raw output mode is now: " + ($rawMode ? 'ON' : 'OFF')) -ForegroundColor Yellow; continue }
    if ($validActions -notcontains $action.ToLower()) {
        Write-WarningMsg "Unknown or unsupported action. Please try again."
        continue
    }
    $toolArgs = @{ action = $action }
    if ($action -eq 'getmonkey' -or $action -eq 'getbananacount' -or $action -eq 'givebanana') {
        $name = Read-Host "Enter the monkey's name [required]"
        if (-not $name) {
            Write-WarningMsg "Name is required for this action."
            continue
        }
        $toolArgs.name = $name
    }
    $callBody = @{ jsonrpc = "2.0"; method = "tools/call"; params = @{ name = 'monkey'; arguments = $toolArgs }; id = 3 } | ConvertTo-Json -Compress
    try {
        $callRaw = Invoke-WebRequest -Uri $baseUrl -Method Post -Headers $headers -Body $callBody -UseBasicParsing | Select-Object -ExpandProperty Content
        Write-Host "\nResponse:" -ForegroundColor Cyan
        if ($callRaw -match 'data: (\{.*\})') {
            $callJson = $matches[1] | ConvertFrom-Json
            # Try to extract the main result content
            $content = $null
            if ($callJson.result -and $callJson.result.content) {
                try { $content = $callJson.result.content[0].text | ConvertFrom-Json } catch {}
            }
            if ($rawMode -or $null -eq $content) {
                Write-Host ($callJson | ConvertTo-Json -Depth 10) -ForegroundColor White
            }
            else {
                # Table output for known actions
                switch ($action.ToLower()) {
                    'getmonkeys' {
                        $content | Format-Table Name, Species, Age, Bananas, InterestingFact -AutoSize
                    }
                    'getmonkey' {
                        $content | Format-Table Name, Species, Age, Bananas, InterestingFact -AutoSize
                    }
                    'getbananacount' {
                        $content | Format-Table Name, Bananas, Description -AutoSize
                    }
                    'givebanana' {
                        $content | Format-Table Name, Bananas, Message -AutoSize
                    }
                    'getrandomfact' {
                        Write-Host $content -ForegroundColor Green
                    }
                    default {
                        Write-Host ($content | ConvertTo-Json -Depth 10) -ForegroundColor White
                    }
                }
            }
        }
        else {
            Write-ErrorMsg "Could not extract JSON from SSE response for tool call."
        }
    }
    catch {
        Write-ErrorMsg "Tool call failed: $($_.Exception.Message)"
    }
}

# Close the session (DELETE)
Write-Info "Closing MCP session..."
try {
    $closeResponse = Invoke-RestMethod -Uri $baseUrl -Method Delete
    Write-Success "Session closed: $($closeResponse | ConvertTo-Json)"
}
catch {
    Write-WarningMsg "Session close returned error (this is normal if no session was open): $($_.Exception.Message)"
}
