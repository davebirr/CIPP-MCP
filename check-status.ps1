#!/usr/bin/env pwsh
# Repository Status Check - Pre-deployment validation

Write-Host "🔍 CIPP-MCP Repository Status Check" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

$issues = @()
$warnings = @()

# Check required files
$requiredFiles = @(
    "src/CIPP-MCP.csproj",
    "src/Program.cs",
    "deployment/AzureDeploymentTemplate.json",
    "deployment/README.md",
    ".env.template",
    ".gitignore",
    "README.md",
    "CONTRIBUTING.md",
    "azure.yaml"
)

Write-Host "📁 Checking required files..." -ForegroundColor Yellow
foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "   ✅ $file" -ForegroundColor Green
    } else {
        Write-Host "   ❌ $file" -ForegroundColor Red
        $issues += "Missing required file: $file"
    }
}

# Check for sensitive files that shouldn't be committed
$sensitiveFiles = @(
    ".env",
    "local.settings.json",
    "secrets.json",
    "test-*.ps1",
    "debug-*.ps1",
    "get-*.ps1"
)

Write-Host ""
Write-Host "🔒 Checking for sensitive files..." -ForegroundColor Yellow
$foundSensitive = $false
foreach ($pattern in $sensitiveFiles) {
    $files = Get-ChildItem -Path . -Name $pattern -Recurse -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        if ($file -notmatch "\.template$" -and $file -ne "test-auth-quick.ps1") {
            Write-Host "   ⚠️  $file (should be git-ignored)" -ForegroundColor Yellow
            $warnings += "Sensitive file found: $file"
            $foundSensitive = $true
        }
    }
}

if (-not $foundSensitive) {
    Write-Host "   ✅ No sensitive files found" -ForegroundColor Green
}

# Check .gitignore
Write-Host ""
Write-Host "📝 Checking .gitignore..." -ForegroundColor Yellow
if (Test-Path ".gitignore") {
    $gitignoreContent = Get-Content ".gitignore" -Raw
    $requiredIgnores = @(".env", "*.env", "local.settings.json", "secrets.json", "test-*.ps1")
    
    foreach ($ignore in $requiredIgnores) {
        if ($gitignoreContent -match [regex]::Escape($ignore)) {
            Write-Host "   ✅ $ignore is ignored" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  $ignore not in .gitignore" -ForegroundColor Yellow
            $warnings += ".gitignore missing: $ignore"
        }
    }
} else {
    $issues += "Missing .gitignore file"
}

# Check ARM template syntax
Write-Host ""
Write-Host "🏗️  Checking ARM template..." -ForegroundColor Yellow
if (Test-Path "deployment/AzureDeploymentTemplate.json") {
    try {
        $template = Get-Content "deployment/AzureDeploymentTemplate.json" -Raw | ConvertFrom-Json
        Write-Host "   ✅ ARM template JSON is valid" -ForegroundColor Green
        
        # Check required sections
        if ($template.parameters) {
            Write-Host "   ✅ Parameters section exists" -ForegroundColor Green
        } else {
            $issues += "ARM template missing parameters section"
        }
        
        if ($template.resources) {
            Write-Host "   ✅ Resources section exists ($($template.resources.Count) resources)" -ForegroundColor Green
        } else {
            $issues += "ARM template missing resources section"
        }
        
        if ($template.outputs) {
            Write-Host "   ✅ Outputs section exists" -ForegroundColor Green
        } else {
            $warnings += "ARM template missing outputs section"
        }
        
    } catch {
        $issues += "ARM template JSON is invalid: $($_.Exception.Message)"
    }
} else {
    $issues += "ARM template file not found"
}

# Check .NET project
Write-Host ""
Write-Host "🔧 Checking .NET project..." -ForegroundColor Yellow
if (Test-Path "src/CIPP-MCP.csproj") {
    try {
        $project = [xml](Get-Content "src/CIPP-MCP.csproj")
        $targetFramework = $project.Project.PropertyGroup.TargetFramework
        Write-Host "   ✅ Project file valid (Target: $targetFramework)" -ForegroundColor Green
        
        # Check for required packages
        $packages = $project.Project.ItemGroup.PackageReference
        $requiredPackages = @("Microsoft.Azure.Functions.Worker", "ModelContextProtocol")
        
        foreach ($pkg in $requiredPackages) {
            $found = $packages | Where-Object { $_.Include -like "*$pkg*" }
            if ($found) {
                Write-Host "   ✅ Package: $pkg" -ForegroundColor Green
            } else {
                $warnings += "Missing package reference: $pkg"
            }
        }
        
    } catch {
        $issues += ".NET project file is invalid: $($_.Exception.Message)"
    }
} else {
    $issues += ".NET project file not found"
}

# Check environment template
Write-Host ""
Write-Host "📋 Checking environment template..." -ForegroundColor Yellow
if (Test-Path ".env.template") {
    $envTemplate = Get-Content ".env.template" -Raw
    $requiredVars = @("CIPP_API_BASE_URL", "AUTH_MODE", "KEY_VAULT_NAME", "AZURE_TENANT_ID")
    
    foreach ($var in $requiredVars) {
        if ($envTemplate -match $var) {
            Write-Host "   ✅ $var" -ForegroundColor Green
        } else {
            $warnings += "Environment template missing: $var"
        }
    }
} else {
    $issues += "Environment template (.env.template) not found"
}

# Check documentation
Write-Host ""
Write-Host "📚 Checking documentation..." -ForegroundColor Yellow
$docs = @(
    @{ File = "README.md"; Content = @("Deploy to Azure", "Fork", "CIPP") },
    @{ File = "CONTRIBUTING.md"; Content = @("Contributing", "Pull Request", "Testing") },
    @{ File = "AUTHENTICATION.md"; Content = @("Authentication", "Key Vault") }
)

foreach ($doc in $docs) {
    if (Test-Path $doc.File) {
        $content = Get-Content $doc.File -Raw
        $missing = @()
        foreach ($term in $doc.Content) {
            if ($content -notmatch $term) {
                $missing += $term
            }
        }
        
        if ($missing.Count -eq 0) {
            Write-Host "   ✅ $($doc.File)" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  $($doc.File) missing content: $($missing -join ', ')" -ForegroundColor Yellow
            $warnings += "$($doc.File) may need content updates"
        }
    } else {
        $warnings += "Documentation file missing: $($doc.File)"
    }
}

# Summary
Write-Host ""
Write-Host "📊 Summary" -ForegroundColor Magenta
Write-Host "==========" -ForegroundColor Magenta

if ($issues.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-Host "✅ Repository is ready for deployment!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 Next steps:" -ForegroundColor Cyan
    Write-Host "1. Commit and push to your forked repository" -ForegroundColor Gray
    Write-Host "2. Update the 'Deploy to Azure' button URL with your GitHub username" -ForegroundColor Gray
    Write-Host "3. Test the ARM template deployment" -ForegroundColor Gray
    Write-Host "4. Configure CI/CD pipeline secrets if needed" -ForegroundColor Gray
} else {
    if ($issues.Count -gt 0) {
        Write-Host "❌ Critical issues found:" -ForegroundColor Red
        foreach ($issue in $issues) {
            Write-Host "   • $issue" -ForegroundColor Red
        }
        Write-Host ""
    }
    
    if ($warnings.Count -gt 0) {
        Write-Host "⚠️  Warnings:" -ForegroundColor Yellow
        foreach ($warning in $warnings) {
            Write-Host "   • $warning" -ForegroundColor Yellow
        }
        Write-Host ""
    }
    
    if ($issues.Count -gt 0) {
        Write-Host "🚫 Please fix critical issues before deployment" -ForegroundColor Red
        exit 1
    } else {
        Write-Host "⚠️  Repository has warnings but can be deployed" -ForegroundColor Yellow
        Write-Host "   Consider addressing warnings for better experience" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "💡 Deployment commands:" -ForegroundColor Blue
Write-Host "   PowerShell: .\deploy.ps1 -GitHubUsername 'YourUsername' -CippApiUrl 'https://your-cipp.azurestaticapps.net' -CippUserEmail 'admin@yourdomain.com'" -ForegroundColor Gray
Write-Host "   Azure Developer CLI: azd up" -ForegroundColor Gray
