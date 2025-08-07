#!/usr/bin/env pwsh
# List available MCP tools

$mcpRequest = @{
    jsonrpc = "2.0"
    id = 1
    method = "tools/list"
}

try {
    $mcpResponse = Invoke-RestMethod -Uri "http://localhost:5000/mcp" -Method POST -Body ($mcpRequest | ConvertTo-Json) -ContentType "application/json"
    
    Write-Host "🛠️  Available MCP Tools:" -ForegroundColor Cyan
    foreach ($tool in $mcpResponse.result.tools) {
        Write-Host "   • $($tool.name)" -ForegroundColor White
        if ($tool.description) {
            Write-Host "     $($tool.description)" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "❌ Failed to list tools: $($_.Exception.Message)" -ForegroundColor Red
}
