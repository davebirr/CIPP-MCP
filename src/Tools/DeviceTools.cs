using CippMcp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace CippMcp.Tools;

[McpServerToolType]
public class DeviceTools
{
    private readonly CippApiService _cippApiService;

    public DeviceTools(CippApiService cippApiService)
    {
        _cippApiService = cippApiService;
    }

    [McpServerTool, Description("List managed devices in a specific tenant")]
    public async Task<string> ListDevices(
        [Description("The tenant ID or domain name")] string tenantFilter,
        [Description("Filter devices by name or type (optional)")] string? deviceFilter = null)
    {
        if (string.IsNullOrEmpty(tenantFilter))
        {
            return "Error: Tenant filter is required";
        }

        try
        {
            var queryParams = new Dictionary<string, string>
            {
                ["tenantFilter"] = tenantFilter
            };

            if (!string.IsNullOrEmpty(deviceFilter))
            {
                queryParams["$filter"] = deviceFilter;
            }

            var devicesJson = await _cippApiService.GetStringAsync("ListDevices", queryParams);
            
            if (devicesJson.StartsWith("Error:"))
            {
                return devicesJson;
            }

            var filterNote = string.IsNullOrEmpty(deviceFilter) ? "" : $" (filtered by: {deviceFilter})";
            return $"Managed devices in {tenantFilter}{filterNote}:\n\n{devicesJson}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving devices: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get detailed information about a specific device")]
    public async Task<string> GetDeviceDetails(
        [Description("The tenant ID or domain name")] string tenantFilter,
        [Description("The device ID or name")] string deviceId)
    {
        if (string.IsNullOrEmpty(tenantFilter) || string.IsNullOrEmpty(deviceId))
        {
            return "Error: Both tenant filter and device ID are required";
        }

        try
        {
            var queryParams = new Dictionary<string, string>
            {
                ["TenantFilter"] = tenantFilter,
                ["DeviceId"] = deviceId
            };

            var deviceDetails = await _cippApiService.GetStringAsync("ListDevices", queryParams);
            
            if (deviceDetails.StartsWith("Error:"))
            {
                return deviceDetails;
            }

            return $"Device details for {deviceId} in {tenantFilter}:\n\n{deviceDetails}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving device details: {ex.Message}";
        }
    }

    [McpServerTool, Description("List device compliance status and policies")]
    public async Task<string> GetDeviceCompliance(
        [Description("The tenant ID or domain name")] string tenantFilter,
        [Description("The device ID (optional, lists all if not provided)")] string? deviceId = null)
    {
        if (string.IsNullOrEmpty(tenantFilter))
        {
            return "Error: Tenant filter is required";
        }

        try
        {
            var queryParams = new Dictionary<string, string>
            {
                ["TenantFilter"] = tenantFilter
            };

            if (!string.IsNullOrEmpty(deviceId))
            {
                queryParams["DeviceId"] = deviceId;
            }

            var complianceJson = await _cippApiService.GetStringAsync("ListDeviceCompliance", queryParams);
            
            if (complianceJson.StartsWith("Error:"))
            {
                return complianceJson;
            }

            var deviceNote = string.IsNullOrEmpty(deviceId) ? "all devices" : deviceId;
            return $"Compliance status for {deviceNote} in {tenantFilter}:\n\n{complianceJson}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving compliance information: {ex.Message}";
        }
    }

    [McpServerTool, Description("List installed applications on managed devices")]
    public async Task<string> ListDeviceApplications(
        [Description("The tenant ID or domain name")] string tenantFilter,
        [Description("The device ID")] string deviceId)
    {
        if (string.IsNullOrEmpty(tenantFilter) || string.IsNullOrEmpty(deviceId))
        {
            return "Error: Both tenant filter and device ID are required";
        }

        try
        {
            var queryParams = new Dictionary<string, string>
            {
                ["TenantFilter"] = tenantFilter,
                ["DeviceId"] = deviceId
            };

            var appsJson = await _cippApiService.GetStringAsync("ListApps", queryParams);
            
            if (appsJson.StartsWith("Error:"))
            {
                return appsJson;
            }

            return $"Applications on device {deviceId} in {tenantFilter}:\n\n{appsJson}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving device applications: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get comprehensive device analytics and compliance overview for a tenant")]
    public async Task<string> GetDeviceAnalytics(
        [Description("The tenant ID or domain name")] string tenantFilter)
    {
        if (string.IsNullOrEmpty(tenantFilter))
        {
            return "Error: Tenant filter is required";
        }

        try
        {
            var analyticsData = new List<string>();
            
            analyticsData.Add("=== DEVICE ANALYTICS DASHBOARD ===");
            analyticsData.Add($"Tenant: {tenantFilter}");
            
            // Get all devices
            var devicesData = await _cippApiService.GetStringAsync("ListDevices", new Dictionary<string, string> { ["tenantFilter"] = tenantFilter });
            analyticsData.Add("\n=== DEVICE OVERVIEW ===");
            if (!devicesData.StartsWith("Error:"))
            {
                var deviceSummary = devicesData.Length > 300 ? devicesData.Substring(0, 300) + "..." : devicesData;
                analyticsData.Add($"Devices: {deviceSummary}");
            }
            else
            {
                analyticsData.Add($"Devices: {devicesData}");
            }
            
            // Get compliance status using Graph API request
            var complianceParams = new Dictionary<string, string>
            {
                ["tenantFilter"] = tenantFilter,
                ["endpoint"] = "deviceManagement/managedDevices",
                ["$select"] = "id,deviceName,complianceState,operatingSystem"
            };
            
            var complianceData = await _cippApiService.GetStringAsync("ListGraphRequest", complianceParams);
            analyticsData.Add("\n=== COMPLIANCE STATUS ===");
            if (!complianceData.StartsWith("Error:"))
            {
                var complianceSummary = complianceData.Length > 300 ? complianceData.Substring(0, 300) + "..." : complianceData;
                analyticsData.Add($"Compliance: {complianceSummary}");
            }
            else
            {
                analyticsData.Add($"Compliance: {complianceData}");
            }
            
            return string.Join("\n", analyticsData);
        }
        catch (Exception ex)
        {
            return $"Error generating device analytics: {ex.Message}";
        }
    }
}
