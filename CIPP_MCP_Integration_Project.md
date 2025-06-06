# CIPP MCP Integration Project

## Project Overview and Goals
The goal of this project is to enable M365 Copilot to use a specialized agent that communicates with the CIPP project using the Model Context Protocol (MCP). This will allow Copilot to solve tasks or fetch data through the existing CIPP API.

> **References:**
> - [MCP documentation](https://modelcontextprotocol.io/introduction)
> - [Microsoft MCP Blog: Introducing Model Context Protocol (MCP) in Copilot Studio](https://www.microsoft.com/en-us/microsoft-copilot/blog/copilot-studio/introducing-model-context-protocol-mcp-in-copilot-studio-simplified-integration-with-ai-apps-and-agents)
> - [CIPP Feature Request #3975](https://github.com/KelvinTegelaar/CIPP/issues/3975)

## Current Architecture
- **CIPP:** Node.js (JavaScript) user interface hosted as an Azure Static Web App.
- **CIPP-API:** Azure Function App written in PowerShell, primarily making calls to the Microsoft Graph API and other Microsoft APIs to fetch data and perform tasks for the end user using the static web app.

## Decision to Add a New CIPP-MCP Azure Function App
- **Language:** C# (.NET)
- **Purpose:** Implement MCP streamable HTTP endpoints to facilitate communication between M365 Copilot and the CIPP project.

## Integration Approach
1. **M365 Copilot (via MCP agent)** sends a request to the MCP streamable HTTP endpoint.
2. **MCP Endpoint (C#)** receives the request, parses the MCP payload, and determines which PowerShell function(s) to call.
3. **MCP Endpoint** calls the appropriate PowerShell function(s) via HTTP.
4. **PowerShell Function** executes the business logic and returns results.
5. **MCP Endpoint** streams the response back to the agent, following MCP streaming conventions.

## Who Makes the API Call?
In the MCP architecture:

MCP Clients (embedded in AI agents or hosts like Claude Desktop, Copilot Studio, etc.) initiate requests for tools, data, or prompts.
MCP Servers are the components that actually execute those requests, including making outbound API calls to third-party services or internal systems.
This means:

All API calls are made by the MCP server, not directly by the AI agent or host.

### Why This Matters for Security
Your security model—restricting API access to only the MCP server—is aligned with best practices. Here's why:

MCP servers act as controlled gateways: They expose only the tools, data, and prompts we explicitly configure. Access to CIPP-API will be externally controlled by Azure, the same way it is the SWA
OAuth 2.1 and gateway policies can be applied at the MCP server level to enforce authentication, authorization, and auditing 4.

### Security Considerations
We cannot allow the MCP server to proxy requests from a third-party API

To mitigate this, ensure:
Use dynamic client registration where possible.
Validate redirect URIs and enforce strict token scoping.
Isolate user sessions and avoid leaking tokens to clients.

## Current HTTP Endpoints in CIPP-API
| Function Name | Function Group |
|---|---|
| ExecAddAlert | HTTP Functions\CIPP\Core |
| ExecAzBobbyTables | HTTP Functions\CIPP\Core |
| ExecCippFunction | HTTP Functions\CIPP\Core |
| ExecCPVRefresh | HTTP Functions\CIPP\Core |
| ExecDurableFunctions | HTTP Functions\CIPP\Core |
| ExecEditTemplate | HTTP Functions\CIPP\Core |
| ExecGeoIPLookup | HTTP Functions\CIPP\Core |
| ExecGraphRequest | HTTP Functions\CIPP\Core |
| ExecListBackup | HTTP Functions\CIPP\Core |
| ExecPartnerWebhook | HTTP Functions\CIPP\Core |
| ExecServicePrincipals | HTTP Functions\CIPP\Core |
| ExecSetCIPPAutoBackup | HTTP Functions\CIPP\Core |
| GetCippAlerts | HTTP Functions\CIPP\Core |
| GetVersion | HTTP Functions\CIPP\Core |
| ListApiTest | HTTP Functions\CIPP\Core |
| ListDirectoryObjects | HTTP Functions\CIPP\Core |
| ListEmptyResults | HTTP Functions\CIPP\Core |
| ListExtensionCacheData | HTTP Functions\CIPP\Core |
| ListGraphBulkRequest | HTTP Functions\CIPP\Core |
| ListGraphRequest | HTTP Functions\CIPP\Core |
| PublicPing | HTTP Functions\CIPP\Core |
| ExecExtensionMapping | HTTP Functions\CIPP\Extensions |
| ExecExtensionsConfig | HTTP Functions\CIPP\Extensions |
| ExecExtensionSync | HTTP Functions\CIPP\Extensions |
| ExecExtensionTest | HTTP Functions\CIPP\Extensions |
| ListExtensionSync | HTTP Functions\CIPP\Extensions |
| AddScheduledItem | HTTP Functions\CIPP\Scheduler |
| ListScheduledItemDetails | HTTP Functions\CIPP\Scheduler |
| ListScheduledItems | HTTP Functions\CIPP\Scheduler |
| RemoveScheduledItem | HTTP Functions\CIPP\Scheduler |
| ExecAccessChecks | HTTP Functions\CIPP\Settings |
| ExecAddTrustedIP | HTTP Functions\CIPP\Settings |
| ExecApiClient | HTTP Functions\CIPP\Settings |
| ExecAPIPermissionList | HTTP Functions\CIPP\Settings |
| ExecBackendURLs | HTTP Functions\CIPP\Settings |
| ExecCippReplacemap | HTTP Functions\CIPP\Settings |
| ExecCPVPermissions | HTTP Functions\CIPP\Settings |
| ExecCustomData | HTTP Functions\CIPP\Settings |
| ExecCustomRole | HTTP Functions\CIPP\Settings |
| ExecDnsConfig | HTTP Functions\CIPP\Settings |
| ExecExcludeLicenses | HTTP Functions\CIPP\Settings |
| ExecExcludeTenant | HTTP Functions\CIPP\Settings |
| ExecMaintenanceScripts | HTTP Functions\CIPP\Settings |
| ExecNotificationConfig | HTTP Functions\CIPP\Settings |
| ExecOffloadFunctions | HTTP Functions\CIPP\Settings |
| ExecPartnerMode | HTTP Functions\CIPP\Settings |
| ExecPasswordConfig | HTTP Functions\CIPP\Settings |
| ExecPermissionRepair | HTTP Functions\CIPP\Settings |
| ExecRemoveTenant | HTTP Functions\CIPP\Settings |
| ExecRestoreBackup | HTTP Functions\CIPP\Settings |
| ExecRunBackup | HTTP Functions\CIPP\Settings |
| ExecSAMAppPermissions | HTTP Functions\CIPP\Settings |
| ExecSAMRoles | HTTP Functions\CIPP\Settings |
| ExecTenantGroup | HTTP Functions\CIPP\Settings |
| ExecWebhookSubscriptions | HTTP Functions\CIPP\Settings |
| ListCustomRole | HTTP Functions\CIPP\Settings |
| ListTenantGroups | HTTP Functions\CIPP\Settings |
| ExecAddTenant | HTTP Functions\CIPP\Setup |
| ExecCombinedSetup | HTTP Functions\CIPP\Setup |
| ExecCreateSAMApp | HTTP Functions\CIPP\Setup |
| ExecDeviceCodeLogon | HTTP Functions\CIPP\Setup |
| ExecSAMSetup | HTTP Functions\CIPP\Setup |
| ExecTokenExchange | HTTP Functions\CIPP\Setup |
| ExecUpdateRefreshToken | HTTP Functions\CIPP\Setup |
| AddContact | HTTP Functions\Email-Exchange\Administration |
| AddSharedMailbox | HTTP Functions\Email-Exchange\Administration |
| EditContact | HTTP Functions\Email-Exchange\Administration |
| ExecConvertMailbox | HTTP Functions\Email-Exchange\Administration |
| ExecCopyForSent | HTTP Functions\Email-Exchange\Administration |
| ExecEditCalendarPermissions | HTTP Functions\Email-Exchange\Administration |
| ExecEditMailboxPermissions | HTTP Functions\Email-Exchange\Administration |
| ExecEmailForward | HTTP Functions\Email-Exchange\Administration |
| ExecEnableArchive | HTTP Functions\Email-Exchange\Administration |
| ExecEnableAutoExpandingArchive | HTTP Functions\Email-Exchange\Administration |
| ExecGroupsDelete | HTTP Functions\Email-Exchange\Administration |
| ExecGroupsDeliveryManagement | HTTP Functions\Email-Exchange\Administration |
| ExecGroupsHideFromGAL | HTTP Functions\Email-Exchange\Administration |
| ExecHideFromGAL | HTTP Functions\Email-Exchange\Administration |
| ExecMailboxMobileDevices | HTTP Functions\Email-Exchange\Administration |
| ExecModifyCalPerms | HTTP Functions\Email-Exchange\Administration |
| ExecModifyMBPerms | HTTP Functions\Email-Exchange\Administration |
| ExecRemoveMailboxRule | HTTP Functions\Email-Exchange\Administration |
| ExecSetLitigationHold | HTTP Functions\Email-Exchange\Administration |
| ExecSetMailboxEmailSize | HTTP Functions\Email-Exchange\Administration |
| ExecSetMailboxLocale | HTTP Functions\Email-Exchange\Administration |
| ExecSetMailboxQuota | HTTP Functions\Email-Exchange\Administration |
| ExecSetMailboxRule | HTTP Functions\Email-Exchange\Administration |
| ExecSetOoO | HTTP Functions\Email-Exchange\Administration |
| ExecSetRecipientLimits | HTTP Functions\Email-Exchange\Administration |
| ExecSetRetentionHold | HTTP Functions\Email-Exchange\Administration |
| ExecStartManagedFolderAssistant | HTTP Functions\Email-Exchange\Administration |
| ListCalendarPermissions | HTTP Functions\Email-Exchange\Administration |
| ListContacts | HTTP Functions\Email-Exchange\Administration |
| ListMailboxes | HTTP Functions\Email-Exchange\Administration |
| ListMailboxMobileDevices | HTTP Functions\Email-Exchange\Administration |
| ListmailboxPermissions | HTTP Functions\Email-Exchange\Administration |
| ListMailboxRules | HTTP Functions\Email-Exchange\Administration |
| ListOoO | HTTP Functions\Email-Exchange\Administration |
| ListSharedMailboxStatistics | HTTP Functions\Email-Exchange\Administration |
| RemoveContact | HTTP Functions\Email-Exchange\Administration |
| ListAntiPhishingFilters | HTTP Functions\Email-Exchange\Reports |
| ListGlobalAddressList | HTTP Functions\Email-Exchange\Reports |
| ListMailboxCAS | HTTP Functions\Email-Exchange\Reports |
| ListMalwareFilters | HTTP Functions\Email-Exchange\Reports |
| ListSafeAttachmentsFilters | HTTP Functions\Email-Exchange\Reports |
| ListSafeLinksFilters | HTTP Functions\Email-Exchange\Reports |
| ListSharedMailboxAccountEnabled | HTTP Functions\Email-Exchange\Reports |
| AddRoomMailbox | HTTP Functions\Email-Exchange\Resources |
| EditRoomMailbox | HTTP Functions\Email-Exchange\Resources |
| ListRoomLists | HTTP Functions\Email-Exchange\Resources |
| ListRooms | HTTP Functions\Email-Exchange\Resources |
| AddQuarantinePolicy | HTTP Functions\Email-Exchange\Spamfilter |
| AddSpamFilter | HTTP Functions\Email-Exchange\Spamfilter |
| AddSpamFilterTemplate | HTTP Functions\Email-Exchange\Spamfilter |
| AddTenantAllowBlockList | HTTP Functions\Email-Exchange\Spamfilter |
| EditAntiPhishingFilter | HTTP Functions\Email-Exchange\Spamfilter |
| EditMalwareFilter | HTTP Functions\Email-Exchange\Spamfilter |
| EditQuarantinePolicy | HTTP Functions\Email-Exchange\Spamfilter |
| EditSafeAttachmentsFilter | HTTP Functions\Email-Exchange\Spamfilter |
| EditSafeLinksFilter | HTTP Functions\Email-Exchange\Spamfilter |
| EditSpamFilter | HTTP Functions\Email-Exchange\Spamfilter |
| ExecQuarantineManagement | HTTP Functions\Email-Exchange\Spamfilter |
| ListConnectionFilter | HTTP Functions\Email-Exchange\Spamfilter |
| ListConnectionFilterTemplates | HTTP Functions\Email-Exchange\Spamfilter |
| ListMailQuarantine | HTTP Functions\Email-Exchange\Spamfilter |
| ListMailQuarantineMessage | HTTP Functions\Email-Exchange\Spamfilter |
| ListQuarantinePolicy | HTTP Functions\Email-Exchange\Spamfilter |
| ListSpamfilter | HTTP Functions\Email-Exchange\Spamfilter |
| ListSpamFilterTemplates | HTTP Functions\Email-Exchange\Spamfilter |
| RemoveConnectionfilterTemplate | HTTP Functions\Email-Exchange\Spamfilter |
| RemoveQuarantinePolicy | HTTP Functions\Email-Exchange\Spamfilter |
| RemoveSpamfilter | HTTP Functions\Email-Exchange\Spamfilter |
| RemoveSpamfilterTemplate | HTTP Functions\Email-Exchange\Spamfilter |
| ExecMailboxRestore | HTTP Functions\Email-Exchange\Tools |
| ExecMailTest | HTTP Functions\Email-Exchange\Tools |
| ListExoRequest | HTTP Functions\Email-Exchange\Tools |
| ListMailboxRestores | HTTP Functions\Email-Exchange\Tools |
| ListMessageTrace | HTTP Functions\Email-Exchange\Tools |
| AddConnectionFilter | HTTP Functions\Email-Exchange\Transport |
| AddConnectionFilterTemplate | HTTP Functions\Email-Exchange\Transport |
| AddExConnector | HTTP Functions\Email-Exchange\Transport |
| AddExConnectorTemplate | HTTP Functions\Email-Exchange\Transport |
| AddTransportRule | HTTP Functions\Email-Exchange\Transport |
| AddTransportTemplate | HTTP Functions\Email-Exchange\Transport |
| EditExConnector | HTTP Functions\Email-Exchange\Transport |
| EditTransportRule | HTTP Functions\Email-Exchange\Transport |
| ListExchangeConnectors | HTTP Functions\Email-Exchange\Transport |
| ListExConnectorTemplates | HTTP Functions\Email-Exchange\Transport |
| ListTransportRules | HTTP Functions\Email-Exchange\Transport |
| ListTransportRulesTemplates | HTTP Functions\Email-Exchange\Transport |
| RemoveExConnector | HTTP Functions\Email-Exchange\Transport |
| RemoveExConnectorTemplate | HTTP Functions\Email-Exchange\Transport |
| RemoveTransportRule | HTTP Functions\Email-Exchange\Transport |
| RemoveTransportRuleTemplate | HTTP Functions\Email-Exchange\Transport |
| AddChocoApp | HTTP Functions\Endpoint\Applications |
| AddMSPApp | HTTP Functions\Endpoint\Applications |
| AddOfficeApp | HTTP Functions\Endpoint\Applications |
| AddStoreApp | HTTP Functions\Endpoint\Applications |
| ExecAppUpload | HTTP Functions\Endpoint\Applications |
| ExecAssignApp | HTTP Functions\Endpoint\Applications |
| ListApplicationQueue | HTTP Functions\Endpoint\Applications |
| ListApps | HTTP Functions\Endpoint\Applications |
| ListAppsRepository | HTTP Functions\Endpoint\Applications |
| RemoveApp | HTTP Functions\Endpoint\Applications |
| AddAPDevice | HTTP Functions\Endpoint\Autopilot |
| AddAutopilotConfig | HTTP Functions\Endpoint\Autopilot |
| AddEnrollment | HTTP Functions\Endpoint\Autopilot |
| ExecAssignAPDevice | HTTP Functions\Endpoint\Autopilot |
| ExecRenameAPDevice | HTTP Functions\Endpoint\Autopilot |
| ExecSetAPDeviceGroupTag | HTTP Functions\Endpoint\Autopilot |
| ExecSyncAPDevices | HTTP Functions\Endpoint\Autopilot |
| ListAPDevices | HTTP Functions\Endpoint\Autopilot |
| ListAutopilotconfig | HTTP Functions\Endpoint\Autopilot |
| RemoveAPDevice | HTTP Functions\Endpoint\Autopilot |
| AddDefenderDeployment | HTTP Functions\Endpoint\MEM |
| AddIntuneTemplate | HTTP Functions\Endpoint\MEM |
| AddPolicy | HTTP Functions\Endpoint\MEM |
| EditIntuneScript | HTTP Functions\Endpoint\MEM |
| EditPolicy | HTTP Functions\Endpoint\MEM |
| ExecAssignPolicy | HTTP Functions\Endpoint\MEM |
| ExecDeviceAction | HTTP Functions\Endpoint\MEM |
| ExecGetLocalAdminPassword | HTTP Functions\Endpoint\MEM |
| ExecGetRecoveryKey | HTTP Functions\Endpoint\MEM |
| ListDefenderState | HTTP Functions\Endpoint\MEM |
| ListDefenderTVM | HTTP Functions\Endpoint\MEM |
| ListIntunePolicy | HTTP Functions\Endpoint\MEM |
| ListIntuneScript | HTTP Functions\Endpoint\MEM |
| ListIntuneTemplates | HTTP Functions\Endpoint\MEM |
| RemoveIntuneScript | HTTP Functions\Endpoint\MEM |
| RemoveIntuneTemplate | HTTP Functions\Endpoint\MEM |
| RemovePolicy | HTTP Functions\Endpoint\MEM |
| ListDevices | HTTP Functions\Endpoint\Reports |
| ExecDeviceDelete | HTTP Functions\Identity\Administration\Devices |
| AddGroup | HTTP Functions\Identity\Administration\Groups |
| AddGroupTemplate | HTTP Functions\Identity\Administration\Groups |
| EditGroup | HTTP Functions\Identity\Administration\Groups |
| ListGroups | HTTP Functions\Identity\Administration\Groups |
| ListGroupSenderAuthentication | HTTP Functions\Identity\Administration\Groups |
| ListGroupTemplates | HTTP Functions\Identity\Administration\Groups |
| RemoveGroupTemplate | HTTP Functions\Identity\Administration\Groups |
| AddGuest | HTTP Functions\Identity\Administration\Users |
| AddUser | HTTP Functions\Identity\Administration\Users |
| AddUserBulk | HTTP Functions\Identity\Administration\Users |
| CIPPOffboardingJob | HTTP Functions\Identity\Administration\Users |
| EditUser | HTTP Functions\Identity\Administration\Users |
| ExecBECCheck | HTTP Functions\Identity\Administration\Users |
| ExecBECRemediate | HTTP Functions\Identity\Administration\Users |
| ExecClrImmId | HTTP Functions\Identity\Administration\Users |
| ExecCreateTAP | HTTP Functions\Identity\Administration\Users |
| ExecDisableUser | HTTP Functions\Identity\Administration\Users |
| ExecDismissRiskyUser | HTTP Functions\Identity\Administration\Users |
| ExecJITAdmin | HTTP Functions\Identity\Administration\Users |
| ExecOffboard_Mailboxpermissions | HTTP Functions\Identity\Administration\Users |
| ExecOffboardUser | HTTP Functions\Identity\Administration\Users |
| ExecOneDriveProvision | HTTP Functions\Identity\Administration\Users |
| ExecOneDriveShortCut | HTTP Functions\Identity\Administration\Users |
| ExecPasswordNeverExpires | HTTP Functions\Identity\Administration\Users |
| ExecPerUserMFA | HTTP Functions\Identity\Administration\Users |
| ExecPerUserMFAAllUsers | HTTP Functions\Identity\Administration\Users |
| ExecResetMFA | HTTP Functions\Identity\Administration\Users |
| ExecResetPass | HTTP Functions\Identity\Administration\Users |
| ExecRestoreDeleted | HTTP Functions\Identity\Administration\Users |
| ExecRevokeSessions | HTTP Functions\Identity\Administration\Users |
| ExecSendPush | HTTP Functions\Identity\Administration\Users |
| ListDeletedItems | HTTP Functions\Identity\Administration\Users |
| ListPerUserMFA | HTTP Functions\Identity\Administration\Users |
| ListUserConditionalAccessPolicies | HTTP Functions\Identity\Administration\Users |
| ListUserCounts | HTTP Functions\Identity\Administration\Users |
| ListUserDevices | HTTP Functions\Identity\Administration\Users |
| ListUserGroups | HTTP Functions\Identity\Administration\Users |
| ListUserMailboxDetails | HTTP Functions\Identity\Administration\Users |
| ListUserMailboxRules | HTTP Functions\Identity\Administration\Users |
| ListUserPhoto | HTTP Functions\Identity\Administration\Users |
| ListUsers | HTTP Functions\Identity\Administration\Users |
| ListUserSettings | HTTP Functions\Identity\Administration\Users |
| ListUserSigninLogs | HTTP Functions\Identity\Administration\Users |
| RemoveDeletedObject | HTTP Functions\Identity\Administration\Users |
| RemoveUser | HTTP Functions\Identity\Administration\Users |
| SetUserAliases | HTTP Functions\Identity\Administration\Users |
| ListAzureADConnectStatus | HTTP Functions\Identity\Reports |
| ListBasicAuth | HTTP Functions\Identity\Reports |
| ListInactiveAccounts | HTTP Functions\Identity\Reports |
| ListMFAUsers | HTTP Functions\Identity\Reports |
| ListSignIns | HTTP Functions\Identity\Reports |
| ExecAlertsList | HTTP Functions\Security |
| ExecIncidentsList | HTTP Functions\Security |
| ExecSetSecurityAlert | HTTP Functions\Security |
| ExecSetSecurityIncident | HTTP Functions\Security |
| AddSite | HTTP Functions\Teams-Sharepoint |
| AddSiteBulk | HTTP Functions\Teams-Sharepoint |
| AddTeam | HTTP Functions\Teams-Sharepoint |
| ExecRemoveTeamsVoicePhoneNumberAssignment | HTTP Functions\Teams-Sharepoint |
| ExecSetSharePointMember | HTTP Functions\Teams-Sharepoint |
| ExecSharePointPerms | HTTP Functions\Teams-Sharepoint |
| ExecTeamsVoicePhoneNumberAssignment | HTTP Functions\Teams-Sharepoint |
| ListSharepointAdminUrl | HTTP Functions\Teams-Sharepoint |
| ListSharepointQuota | HTTP Functions\Teams-Sharepoint |
| ListSharepointSettings | HTTP Functions\Teams-Sharepoint |
| ListSites | HTTP Functions\Teams-Sharepoint |
| ListTeams | HTTP Functions\Teams-Sharepoint |
| ListTeamsActivity | HTTP Functions\Teams-Sharepoint |
| ListTeamsLisLocation | HTTP Functions\Teams-Sharepoint |
| ListTeamsVoice | HTTP Functions\Teams-Sharepoint |
| ExecAddSPN | HTTP Functions\Tenant\Administration |
| ExecOffboardTenant | HTTP Functions\Tenant\Administration |
| ExecOnboardTenant | HTTP Functions\Tenant\Administration |
| ExecUpdateSecureScore | HTTP Functions\Tenant\Administration |
| ListAppConsentRequests | HTTP Functions\Tenant\Administration |
| ListDomains | HTTP Functions\Tenant\Administration |
| ListTenantOnboarding | HTTP Functions\Tenant\Administration |
| SetAuthMethod | HTTP Functions\Tenant\Administration |
| AddAlert | HTTP Functions\Tenant\Administration\Alerts |
| ExecAuditLogSearch | HTTP Functions\Tenant\Administration\Alerts |
| ListAlertsQueue | HTTP Functions\Tenant\Administration\Alerts |
| ListAuditLogs | HTTP Functions\Tenant\Administration\Alerts |
| ListAuditLogSearches | HTTP Functions\Tenant\Administration\Alerts |
| ListAuditLogTest | HTTP Functions\Tenant\Administration\Alerts |
| ListWebhookAlert | HTTP Functions\Tenant\Administration\Alerts |
| PublicWebhooks | HTTP Functions\Tenant\Administration\Alerts |
| RemoveQueuedAlert | HTTP Functions\Tenant\Administration\Alerts |
| ExecAddMultiTenantApp | HTTP Functions\Tenant\Administration\Application Approval |
| ExecAppApproval | HTTP Functions\Tenant\Administration\Application Approval |
| ExecAppApprovalTemplate | HTTP Functions\Tenant\Administration\Application Approval |
| ExecAppPermissionTemplate | HTTP Functions\Tenant\Administration\Application Approval |
| ListAppApprovalTemplates | HTTP Functions\Tenant\Administration\Application Approval |
| AddTenant | HTTP Functions\Tenant\Administration\Tenant |
| EditTenant | HTTP Functions\Tenant\Administration\Tenant |
| ListTenantDetails | HTTP Functions\Tenant\Administration\Tenant |
| ListTenants | HTTP Functions\Tenant\Administration\Tenant |
| AddCAPolicy | HTTP Functions\Tenant\Conditional |
| AddCATemplate | HTTP Functions\Tenant\Conditional |
| AddNamedLocation | HTTP Functions\Tenant\Conditional |
| EditCAPolicy | HTTP Functions\Tenant\Conditional |
| ExecCaCheck | HTTP Functions\Tenant\Conditional |
| ExecCAExclusion | HTTP Functions\Tenant\Conditional |
| ExecNamedLocation | HTTP Functions\Tenant\Conditional |
| ListCAtemplates | HTTP Functions\Tenant\Conditional |
| ListConditionalAccessPolicies | HTTP Functions\Tenant\Conditional |
| ListConditionalAccessPolicyChanges | HTTP Functions\Tenant\Conditional |
| RemoveCAPolicy | HTTP Functions\Tenant\Conditional |
| RemoveCATemplate | HTTP Functions\Tenant\Conditional |
| ExecAddGDAPRole | HTTP Functions\Tenant\GDAP |
| ExecAutoExtendGDAP | HTTP Functions\Tenant\GDAP |
| ExecDeleteGDAPRelationship | HTTP Functions\Tenant\GDAP |
| ExecDeleteGDAPRoleMapping | HTTP Functions\Tenant\GDAP |
| ExecGDAPAccessAssignment | HTTP Functions\Tenant\GDAP |
| ExecGDAPInvite | HTTP Functions\Tenant\GDAP |
| ExecGDAPInviteApproved | HTTP Functions\Tenant\GDAP |
| ExecGDAPRemoveGArole | HTTP Functions\Tenant\GDAP |
| ExecGDAPRoleTemplate | HTTP Functions\Tenant\GDAP |
| ListGDAPAccessAssignments | HTTP Functions\Tenant\GDAP |
| ListGDAPInvite | HTTP Functions\Tenant\GDAP |
| ListGDAPRoles | HTTP Functions\Tenant\GDAP |
| ListLicenses | HTTP Functions\Tenant\Reports |
| ListOAuthApps | HTTP Functions\Tenant\Reports |
| ListServiceHealth | HTTP Functions\Tenant\Reports |
| AddStandardsDeploy | HTTP Functions\Tenant\Standards |
| AddStandardsTemplate | HTTP Functions\Tenant\Standards |
| BestPracticeAnalyser_List | HTTP Functions\Tenant\Standards |
| CIPPStandardsRun | HTTP Functions\Tenant\Standards |
| DomainAnalyser_List | HTTP Functions\Tenant\Standards |
| ExecBPA | HTTP Functions\Tenant\Standards |
| ExecDomainAnalyser | HTTP Functions\Tenant\Standards |
| ExecStandardConvert | HTTP Functions\Tenant\Standards |
| ExecStandardsRun | HTTP Functions\Tenant\Standards |
| ListBPA | HTTP Functions\Tenant\Standards |
| ListBPATemplates | HTTP Functions\Tenant\Standards |
| ListDomainAnalyser | HTTP Functions\Tenant\Standards |
| ListDomainHealth | HTTP Functions\Tenant\Standards |
| ListStandards | HTTP Functions\Tenant\Standards |
| ListStandardsCompare | HTTP Functions\Tenant\Standards |
| listStandardTemplates | HTTP Functions\Tenant\Standards |
| RemoveBPATemplate | HTTP Functions\Tenant\Standards |
| RemoveStandard | HTTP Functions\Tenant\Standards |
| RemoveStandardTemplate | HTTP Functions\Tenant\Standards |
| AddBPATemplate | HTTP Functions\Tenant\Tools |
| ExecGraphExplorerPreset | HTTP Functions\Tenant\Tools |
| ExecCommunityRepo | HTTP Functions\Tools\GitHub |
| ExecGitHubAction | HTTP Functions\Tools\GitHub |
| ListCommunityRepos | HTTP Functions\Tools\GitHub |
