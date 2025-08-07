#!/usr/bin/env pwsh
param(
    [ValidateSet("browser", "keyvault", "setup", "show")]
    [string]$Mode = "setup",
    [switch]$Help
)

function Show-EnvStatus {
    Write-Host "📄 Current .env file status:" -ForegroundColor Cyan
    
    if (Test-Path ".env") {
        Write-Host "✅ .env file exists" -ForegroundColor Green
        
        $content = Get-Content ".env" -Raw
        
        # Show current AUTH_MODE
        if ($content -match "AUTH_MODE=(.*)") {
            Write-Host "   Current AUTH_MODE: $($matches[1])" -ForegroundColor Gray
        } else {
            Write-Host "   AUTH_MODE: Not set" -ForegroundColor Yellow
        }
        
        # Show key configuration presence
        $configs = @(
            @{ Name = "AZURE_RESOURCE_GROUP"; Pattern = "AZURE_RESOURCE_GROUP=" },
            @{ Name = "AZURE_SUBSCRIPTION_ID"; Pattern = "AZURE_SUBSCRIPTION_ID=" },
            @{ Name = "AZURE_CLIENT_ID"; Pattern = "AZURE_CLIENT_ID=" },
            @{ Name = "KEY_VAULT_NAME"; Pattern = "KEY_VAULT_NAME=" },
            @{ Name = "CIPP_API_BASE_URL"; Pattern = "CIPP_API_BASE_URL=" },
            @{ Name = "BROWSER_AUTH_COOKIE"; Pattern = "BROWSER_AUTH_COOKIE=" }
        )
        
        foreach ($config in $configs) {
            if ($content -match $config.Pattern) {
                Write-Host "   ✅ $($config.Name) configured" -ForegroundColor Green
            } else {
                Write-Host "   ❌ $($config.Name) missing" -ForegroundColor Red
            }
        }
        
    } else {
        Write-Host "❌ No .env file found" -ForegroundColor Red
    }
    Write-Host ""
}

if ($Help) {
    Write-Host "🔐 CIPP-MCP Authentication Setup" -ForegroundColor Cyan
    Write-Host "===============================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "This script helps set up authentication for CIPP-MCP development." -ForegroundColor Gray
    Write-Host "It safely updates your .env file by backing up the original first." -ForegroundColor Gray
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\setup-auth.ps1 -Mode setup     # Show setup instructions" -ForegroundColor Gray
    Write-Host "  .\setup-auth.ps1 -Mode show      # Show current .env status" -ForegroundColor Gray
    Write-Host "  .\setup-auth.ps1 -Mode browser   # Set up browser authentication" -ForegroundColor Gray
    Write-Host "  .\setup-auth.ps1 -Mode keyvault  # Set up Key Vault authentication" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\setup-auth.ps1 -Mode show      # Check current configuration" -ForegroundColor Gray
    Write-Host "  .\setup-auth.ps1 -Mode browser   # Easiest for development" -ForegroundColor Gray
    Write-Host "  .\setup-auth.ps1 -Mode keyvault  # Production-like testing" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Safety:" -ForegroundColor Green
    Write-Host "  • Always backs up existing .env before changes" -ForegroundColor Gray
    Write-Host "  • Only modifies AUTH_MODE and related settings" -ForegroundColor Gray
    Write-Host "  • Preserves Azure credentials and resource configuration" -ForegroundColor Gray
    exit 0
}

Write-Host "🔐 CIPP-MCP Authentication Setup" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan
Write-Host ""

switch ($Mode) {
    "show" {
        Show-EnvStatus
    }
    
    "browser" {
        Write-Host "🌐 Setting up Browser Authentication" -ForegroundColor Yellow
        Write-Host "====================================" -ForegroundColor Yellow
        Write-Host ""
        
        # Check if .env exists and backup if needed
        if (Test-Path ".env") {
            $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
            $backupPath = ".env.backup.$timestamp"
            Copy-Item ".env" $backupPath
            Write-Host "📦 Backed up existing .env to: $backupPath" -ForegroundColor Cyan
            
            # Read existing .env and update only auth-related values
            $existingContent = Get-Content ".env" -Raw
            
            # Update AUTH_MODE
            if ($existingContent -match "AUTH_MODE=.*") {
                $existingContent = $existingContent -replace "AUTH_MODE=.*", "AUTH_MODE=browser"
            } else {
                $existingContent += "`nAUTH_MODE=browser"
            }
            
            # Ensure BROWSER_AUTH_COOKIE exists
            if ($existingContent -notmatch "BROWSER_AUTH_COOKIE=") {
                $existingContent += "`nBROWSER_AUTH_COOKIE="
            }
            
            # Update the file
            $existingContent | Out-File -FilePath ".env" -Encoding UTF8 -NoNewline
            Write-Host "✅ Updated existing .env for browser authentication" -ForegroundColor Green
            
        } else {
            # Create new .env file with generic template
            $envContent = @"
# CIPP-MCP Browser Authentication Configuration
AUTH_MODE=browser
CIPP_USER_EMAIL=your-email@yourdomain.com
CIPP_SWA_URL=https://your-cipp-frontend.azurestaticapps.net
BROWSER_AUTH_COOKIE=

# Add your browser cookie here after getting it from:
# 1. Open CIPP in browser with your email profile
# 2. F12 > Application > Cookies > StaticWebAppsAuthCookie
# 3. Copy the value and paste above
"@
            $envContent | Out-File -FilePath ".env" -Encoding UTF8
            Write-Host "✅ Created new .env with browser authentication template" -ForegroundColor Green
        }
        
        Write-Host ""
        Write-Host "📋 Next Steps:" -ForegroundColor Yellow
        Write-Host "1. Update CIPP_USER_EMAIL and CIPP_SWA_URL in .env if needed" -ForegroundColor Gray
        Write-Host "2. Run: .\scripts\auth\Get-BrowserSession.ps1" -ForegroundColor Gray
        Write-Host "3. Sign in to CIPP with your email" -ForegroundColor Gray
        Write-Host "3. Get the cookie value from Developer Tools" -ForegroundColor Gray
        Write-Host "4. Update BROWSER_AUTH_COOKIE in .env" -ForegroundColor Gray
        Write-Host "5. Test with: .\scripts\auth\Test-CookieAuth.ps1 -AuthCookie 'your-cookie'" -ForegroundColor Gray
    }
    
    "keyvault" {
        Write-Host "🔑 Setting up Key Vault Authentication" -ForegroundColor Yellow
        Write-Host "======================================" -ForegroundColor Yellow
        Write-Host ""
        
        # Check if .env exists and update auth mode
        if (Test-Path ".env") {
            $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
            $backupPath = ".env.backup.$timestamp"
            Copy-Item ".env" $backupPath
            Write-Host "📦 Backed up existing .env to: $backupPath" -ForegroundColor Cyan
            
            # Read existing content and update AUTH_MODE
            $existingContent = Get-Content ".env" -Raw
            if ($existingContent -match "AUTH_MODE=.*") {
                $existingContent = $existingContent -replace "AUTH_MODE=.*", "AUTH_MODE=keyvault"
            } else {
                $existingContent += "`nAUTH_MODE=keyvault"
            }
            
            $existingContent | Out-File -FilePath ".env" -Encoding UTF8 -NoNewline
            Write-Host "✅ Updated AUTH_MODE to keyvault in existing .env" -ForegroundColor Green
        } else {
            Write-Host "⚠️  No .env file found. Please run the Azure setup first:" -ForegroundColor Yellow
            Write-Host "   .\Scripts\Setup-Azure-Resources.ps1" -ForegroundColor Gray
        }
        
        Write-Host ""
        Write-Host "📋 Steps to complete:" -ForegroundColor Yellow
        Write-Host "1. Ensure service principal is configured (if not done already)" -ForegroundColor Gray
        Write-Host "2. Verify Key Vault access permissions" -ForegroundColor Gray
        Write-Host "3. Test authentication" -ForegroundColor Gray
        Write-Host ""
        
        Write-Host "🔧 To set up service principal (if needed):" -ForegroundColor Green
        Write-Host "   .\Scripts\Setup-Azure-Resources.ps1" -ForegroundColor Gray
        Write-Host ""
        
        Write-Host "🧪 To test Key Vault authentication:" -ForegroundColor Green
        Write-Host "   .\scripts\auth\Test-KeyVaultAuth.ps1" -ForegroundColor Gray
    }
    
    "setup" {
        Write-Host "🎯 Choose Your Authentication Method" -ForegroundColor Yellow
        Write-Host "====================================" -ForegroundColor Yellow
        Write-Host ""
        
        # Show current status first
        Show-EnvStatus
        
        Write-Host "🌐 Browser Authentication (Recommended for Development)" -ForegroundColor Green
        Write-Host "   • Easiest to set up" -ForegroundColor Gray
        Write-Host "   • Uses your existing CIPP login" -ForegroundColor Gray
        Write-Host "   • Perfect for testing and development" -ForegroundColor Gray
        Write-Host "   • Run: .\setup-auth.ps1 -Mode browser" -ForegroundColor Cyan
        Write-Host ""
        
        Write-Host "🔑 Key Vault Authentication (Production-like)" -ForegroundColor Yellow
        Write-Host "   • Uses existing service principal configuration" -ForegroundColor Gray
        Write-Host "   • Accesses real CIPP secrets from Azure Key Vault" -ForegroundColor Gray
        Write-Host "   • Production-ready authentication" -ForegroundColor Gray
        Write-Host "   • Run: .\setup-auth.ps1 -Mode keyvault" -ForegroundColor Cyan
        Write-Host ""
        
        Write-Host "💡 Safety Features:" -ForegroundColor Magenta
        Write-Host "   • Always backs up .env before changes" -ForegroundColor Gray
        Write-Host "   • Only modifies authentication settings" -ForegroundColor Gray
        Write-Host "   • Preserves your Azure resource configuration" -ForegroundColor Gray
        Write-Host ""
        
        Write-Host "📊 To check current configuration: .\setup-auth.ps1 -Mode show" -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "📚 For more information, see: AUTHENTICATION.md" -ForegroundColor Gray
