using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSys.Core.Security;

public static class PermissionConstants
{
    // User Management Permissions
    public const string ViewUsers = "Users.View";
    public const string CreateUsers = "Users.Create";
    public const string EditUsers = "Users.Edit";
    public const string DeleteUsers = "Users.Delete";
    public const string ManageUserRoles = "Users.ManageRoles";
    public const string ViewUserProfiles = "Users.ViewProfiles";
    public const string ExportUsers = "Users.Export";
    public const string ImportUsers = "Users.Import";

    // Role & Permission Management
    public const string ViewRoles = "Roles.View";
    public const string CreateRoles = "Roles.Create";
    public const string EditRoles = "Roles.Edit";
    public const string DeleteRoles = "Roles.Delete";
    public const string ManagePermissions = "Permissions.Manage";
    public const string ViewPermissionMatrix = "Permissions.ViewMatrix";
    public const string EditPermissionMatrix = "Permissions.EditMatrix";

    // Department Management
    public const string ViewDepartments = "Departments.View";
    public const string CreateDepartments = "Departments.Create";
    public const string EditDepartments = "Departments.Edit";
    public const string DeleteDepartments = "Departments.Delete";

    // System Administration
    public const string ViewSystemSettings = "System.ViewSettings";
    public const string EditSystemSettings = "System.EditSettings";
    public const string ViewAuditLogs = "System.ViewAuditLogs";
    public const string ManageBackups = "System.ManageBackups";
    public const string ViewSystemHealth = "System.ViewHealth";

    // Dashboard & Reports
    public const string ViewDashboard = "Dashboard.View";
    public const string ViewReports = "Reports.View";
    public const string CreateReports = "Reports.Create";
    public const string ExportReports = "Reports.Export";
    public const string ScheduleReports = "Reports.Schedule";

    // Inventory Management (for future modules)
    public const string ViewInventory = "Inventory.View";
    public const string ManageInventory = "Inventory.Manage";
    public const string ViewProducts = "Products.View";
    public const string ManageProducts = "Products.Manage";

    // Super Admin (Full Access)
    public const string SuperAdmin = "SuperAdmin";

    // Permission Categories for UI Organization
    public static readonly Dictionary<string, List<string>> PermissionCategories = new()
    {
        ["User Management"] = new List<string>
        {
            ViewUsers, CreateUsers, EditUsers, DeleteUsers, 
            ManageUserRoles, ViewUserProfiles, ExportUsers, ImportUsers
        },
        ["Role & Permission Management"] = new List<string>
        {
            ViewRoles, CreateRoles, EditRoles, DeleteRoles,
            ManagePermissions, ViewPermissionMatrix, EditPermissionMatrix
        },
        ["Department Management"] = new List<string>
        {
            ViewDepartments, CreateDepartments, EditDepartments, DeleteDepartments
        },
        ["System Administration"] = new List<string>
        {
            ViewSystemSettings, EditSystemSettings, ViewAuditLogs, 
            ManageBackups, ViewSystemHealth
        },
        ["Dashboard & Reports"] = new List<string>
        {
            ViewDashboard, ViewReports, CreateReports, ExportReports, ScheduleReports
        },
        ["Inventory Management"] = new List<string>
        {
            ViewInventory, ManageInventory, ViewProducts, ManageProducts
        }
    };

    // Role Templates for Quick Setup
    public static readonly Dictionary<string, List<string>> RoleTemplates = new()
    {
        ["Super Administrator"] = new List<string> { SuperAdmin },
        ["System Administrator"] = new List<string>
        {
            ViewUsers, CreateUsers, EditUsers, DeleteUsers, ManageUserRoles,
            ViewRoles, CreateRoles, EditRoles, ManagePermissions, ViewPermissionMatrix, EditPermissionMatrix,
            ViewDepartments, CreateDepartments, EditDepartments, DeleteDepartments,
            ViewSystemSettings, EditSystemSettings, ViewAuditLogs, ViewSystemHealth,
            ViewDashboard, ViewReports, CreateReports, ExportReports
        },
        ["HR Manager"] = new List<string>
        {
            ViewUsers, CreateUsers, EditUsers, ViewUserProfiles, ExportUsers, ImportUsers,
            ViewDepartments, CreateDepartments, EditDepartments,
            ViewDashboard, ViewReports
        },
        ["Department Manager"] = new List<string>
        {
            ViewUsers, ViewUserProfiles, ViewDepartments,
            ViewDashboard, ViewReports
        },
        ["Regular User"] = new List<string>
        {
            ViewDashboard, ViewUserProfiles
        },
        ["Read Only"] = new List<string>
        {
            ViewUsers, ViewDepartments, ViewDashboard
        }
    };

    // Permission Descriptions for Better UX
    public static readonly Dictionary<string, string> PermissionDescriptions = new()
    {
        [ViewUsers] = "View user list and basic information",
        [CreateUsers] = "Create new user accounts",
        [EditUsers] = "Edit existing user information",
        [DeleteUsers] = "Delete or deactivate user accounts",
        [ManageUserRoles] = "Assign and modify user roles",
        [ViewUserProfiles] = "View detailed user profiles",
        [ExportUsers] = "Export user data to files",
        [ImportUsers] = "Import users from external sources",
        
        [ViewRoles] = "View available roles in the system",
        [CreateRoles] = "Create new roles",
        [EditRoles] = "Modify existing roles",
        [DeleteRoles] = "Delete roles from the system",
        [ManagePermissions] = "Create and manage permissions",
        [ViewPermissionMatrix] = "View role-permission assignments",
        [EditPermissionMatrix] = "Modify role-permission assignments",
        
        [ViewDepartments] = "View department information",
        [CreateDepartments] = "Create new departments",
        [EditDepartments] = "Modify department details",
        [DeleteDepartments] = "Delete departments",
        
        [ViewSystemSettings] = "View system configuration",
        [EditSystemSettings] = "Modify system settings",
        [ViewAuditLogs] = "Access system audit trails",
        [ManageBackups] = "Create and restore system backups",
        [ViewSystemHealth] = "Monitor system performance",
        
        [ViewDashboard] = "Access main dashboard",
        [ViewReports] = "View system reports",
        [CreateReports] = "Create custom reports",
        [ExportReports] = "Export reports to files",
        [ScheduleReports] = "Schedule automated reports",
        
        [SuperAdmin] = "Full system access (use with caution)"
    };
}

