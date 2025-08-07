using CippMcp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace CippMcp.Tools;

[McpServerToolType]
public class UserTools
{
    private readonly CippApiService _cippApiService;

    public UserTools(CippApiService cippApiService)
    {
        _cippApiService = cippApiService;
    }

    [McpServerTool, Description("List users in a specific tenant")]
    public async Task<string> ListUsers(
        [Description("The tenant ID or domain name")] string tenantFilter,
        [Description("Search filter for user names or emails (optional)")] string? userFilter = null)
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

            if (!string.IsNullOrEmpty(userFilter))
            {
                queryParams["$filter"] = userFilter;
            }

            var usersJson = await _cippApiService.GetStringAsync("ListUsers", queryParams);
            
            if (usersJson.StartsWith("Error:"))
            {
                return usersJson;
            }

            var filterNote = string.IsNullOrEmpty(userFilter) ? "" : $" (filtered by: {userFilter})";
            return $"Users in {tenantFilter}{filterNote}:\n\n{usersJson}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving users: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get detailed information about a specific user")]
    public async Task<string> GetUserDetails(
        [Description("The tenant ID or domain name")] string tenantFilter,
        [Description("The user ID or email address")] string userId)
    {
        if (string.IsNullOrEmpty(tenantFilter) || string.IsNullOrEmpty(userId))
        {
            return "Error: Both tenant filter and user ID are required";
        }

        try
        {
            var queryParams = new Dictionary<string, string>
            {
                ["TenantFilter"] = tenantFilter,
                ["UserId"] = userId
            };

            var userDetails = await _cippApiService.GetStringAsync("ListUsers", queryParams);
            
            if (userDetails.StartsWith("Error:"))
            {
                return userDetails;
            }

            return $"User Details for {userId} in {tenantFilter}:\n\n{userDetails}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving user details: {ex.Message}";
        }
    }

    [McpServerTool, Description("List user licenses and assignments")]
    public async Task<string> ListUserLicenses(
        [Description("The tenant ID or domain name")] string tenantFilter,
        [Description("The user ID or email address (optional, lists all if not provided)")] string? userId = null)
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

            if (!string.IsNullOrEmpty(userId))
            {
                queryParams["UserId"] = userId;
            }

            var licensesJson = await _cippApiService.GetStringAsync("ListLicenses", queryParams);
            
            if (licensesJson.StartsWith("Error:"))
            {
                return licensesJson;
            }

            var userNote = string.IsNullOrEmpty(userId) ? "all users" : userId;
            return $"License information for {userNote} in {tenantFilter}:\n\n{licensesJson}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving license information: {ex.Message}";
        }
    }

    [McpServerTool, Description("Check user sign-in activity and last logon")]
    public async Task<string> GetUserSignInActivity(
        [Description("The tenant ID or domain name")] string tenantFilter,
        [Description("The user ID or email address")] string userId)
    {
        if (string.IsNullOrEmpty(tenantFilter) || string.IsNullOrEmpty(userId))
        {
            return "Error: Both tenant filter and user ID are required";
        }

        try
        {
            var queryParams = new Dictionary<string, string>
            {
                ["TenantFilter"] = tenantFilter,
                ["UserId"] = userId
            };

            var activityJson = await _cippApiService.GetStringAsync("ListSignIns", queryParams);
            
            if (activityJson.StartsWith("Error:"))
            {
                return activityJson;
            }

            return $"Sign-in activity for {userId} in {tenantFilter}:\n\n{activityJson}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving sign-in activity: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get comprehensive user analytics and insights for a tenant")]
    public async Task<string> GetUserAnalytics(
        [Description("The tenant ID or domain name")] string tenantFilter)
    {
        if (string.IsNullOrEmpty(tenantFilter))
        {
            return "Error: Tenant filter is required";
        }

        try
        {
            var analyticsData = new List<string>();
            
            analyticsData.Add("=== USER ANALYTICS DASHBOARD ===");
            analyticsData.Add($"Tenant: {tenantFilter}");
            
            // Get all users
            var usersData = await _cippApiService.GetStringAsync("ListUsers", new Dictionary<string, string> { ["tenantFilter"] = tenantFilter });
            analyticsData.Add("\n=== USER OVERVIEW ===");
            if (!usersData.StartsWith("Error:"))
            {
                var userSummary = usersData.Length > 300 ? usersData.Substring(0, 300) + "..." : usersData;
                analyticsData.Add($"Users: {userSummary}");
            }
            else
            {
                analyticsData.Add($"Users: {usersData}");
            }
            
            // Get MFA status
            var mfaData = await _cippApiService.GetStringAsync("ListMFAUsers", new Dictionary<string, string> { ["tenantFilter"] = tenantFilter });
            analyticsData.Add("\n=== MFA ADOPTION ===");
            if (!mfaData.StartsWith("Error:"))
            {
                var mfaSummary = mfaData.Length > 300 ? mfaData.Substring(0, 300) + "..." : mfaData;
                analyticsData.Add($"MFA Status: {mfaSummary}");
            }
            else
            {
                analyticsData.Add($"MFA Status: {mfaData}");
            }
            
            // Get sign-in reports
            var signInData = await _cippApiService.GetStringAsync("ListSignIns", new Dictionary<string, string> { ["tenantFilter"] = tenantFilter });
            analyticsData.Add("\n=== RECENT SIGN-IN ACTIVITY ===");
            if (!signInData.StartsWith("Error:"))
            {
                var signInSummary = signInData.Length > 300 ? signInData.Substring(0, 300) + "..." : signInData;
                analyticsData.Add($"Sign-ins: {signInSummary}");
            }
            else
            {
                analyticsData.Add($"Sign-ins: {signInData}");
            }
            
            return string.Join("\n", analyticsData);
        }
        catch (Exception ex)
        {
            return $"Error generating user analytics: {ex.Message}";
        }
    }
}
