# 🔑 Permission Reference Guide

## ✅ **FIXED: Permission Names**

### **The Problem:**
UserIndex.razor was checking for `"ManageUsers"` permission which **doesn't exist** in the system!

### **The Solution:**
Updated to use the **actual permission names** from `PermissionConstants.cs`:

---

## 📋 **Correct Permission Names**

### **User Management Permissions**
```csharp
"Users.View"          // View user list
"Users.Create"        // Create new users
"Users.Edit"          // Edit user information
"Users.Delete"        // Delete/restore users
"Users.ManageRoles"   // Assign roles to users
"Users.ViewProfiles"  // View detailed profiles
"Users.Export"        // Export user data
"Users.Import"        // Import users
```

### **Role & Permission Management**
```csharp
"Roles.View"                // View roles
"Roles.Create"              // Create roles
"Roles.Edit"                // Edit roles
"Roles.Delete"              // Delete roles
"Permissions.Manage"        // Manage permissions
"Permissions.ViewMatrix"    // View permission matrix
"Permissions.EditMatrix"    // Edit permission matrix
```

### **Department Management**
```csharp
"Departments.View"    // View departments
"Departments.Create"  // Create departments
"Departments.Edit"    // Edit departments
"Departments.Delete"  // Delete departments
```

### **System Administration**
```csharp
"System.ViewSettings"   // View system settings
"System.EditSettings"   // Edit system settings
"System.ViewAuditLogs"  // View audit logs
"System.ManageBackups"  // Manage backups
"System.ViewHealth"     // View system health
```

### **Dashboard & Reports**
```csharp
"Dashboard.View"      // View dashboard
"Reports.View"        // View reports
"Reports.Create"      // Create reports
"Reports.Export"      // Export reports
"Reports.Schedule"    // Schedule reports
```

### **Super Admin**
```csharp
"SuperAdmin"  // Full system access (use with caution!)
```

---

## 👥 **Role Templates**

### **System Administrator Role**
Has these permissions by default:
- ✅ `Users.View`
- ✅ `Users.Create`
- ✅ `Users.Edit`
- ✅ `Users.Delete`
- ✅ `Users.ManageRoles`
- ✅ `Roles.View`
- ✅ `Roles.Create`
- ✅ `Roles.Edit`
- ✅ `Permissions.Manage`
- ✅ `Permissions.ViewMatrix`
- ✅ `Permissions.EditMatrix`
- ✅ `Departments.View`
- ✅ `Departments.Create`
- ✅ `Departments.Edit`
- ✅ `Departments.Delete`
- ✅ `System.ViewSettings`
- ✅ `System.EditSettings`
- ✅ `System.ViewAuditLogs`
- ✅ `System.ViewHealth`
- ✅ `Dashboard.View`
- ✅ `Reports.View`
- ✅ `Reports.Create`
- ✅ `Reports.Export`

### **Super Administrator Role**
Has only:
- ✅ `SuperAdmin` (grants access to everything)

---

## 🔍 **How to Check Your Permissions**

### **Method 1: PolicyDebug Page**
1. Navigate to `/policydebug`
2. View all your current claims
3. See which permissions you have
4. Click "Refresh Claims from DB" if needed

### **Method 2: Database Query**
```sql
-- Check permissions for a specific user
SELECT 
    u.Username,
    r.RoleName,
    p.PermissionName,
    p.Description
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.RoleId
INNER JOIN RolePermissions rp ON r.RoleId = rp.RoleId
INNER JOIN Permissions p ON rp.PermissionId = p.PermissionId
WHERE u.Username = 'admin'
ORDER BY p.PermissionName;
```

---

## 🛠️ **What Was Fixed in UserIndex.razor**

### **Before (WRONG):**
```csharp
private bool CanEditUser(User user)
{
    return HasPermission("ManageUsers") || IsCurrentUser(user);  // ❌ Wrong!
}

private bool CanDeleteUser(User user)
{
    if (IsCurrentUser(user)) return false;
    return HasPermission("ManageUsers");  // ❌ Wrong!
}
```

### **After (CORRECT):**
```csharp
private bool CanEditUser(User user)
{
    return HasPermission("Users.Edit") || IsCurrentUser(user);  // ✅ Correct!
}

private bool CanDeleteUser(User user)
{
    if (IsCurrentUser(user)) return false;
    return HasPermission("Users.Delete");  // ✅ Correct!
}

private bool CanCreateUser()
{
    return HasPermission("Users.Create");  // ✅ Correct!
}

private bool CanRestoreUser()
{
    return HasPermission("Users.Delete");  // ✅ Correct!
}
```

---

## 🎯 **Testing Your Permissions**

### **Test 1: Login as System Administrator**
Expected behavior:
- ✅ See "Add User" button
- ✅ See "Show Deleted" button
- ✅ Can click Edit on any user
- ✅ Can click Delete on other users
- ❌ Cannot delete own account (disabled with tooltip)

### **Test 2: Login as Regular User**
Expected behavior:
- ❌ No "Add User" button
- ❌ No "Show Deleted" button
- ✅ Can edit own profile only
- ❌ Cannot delete any users

### **Test 3: Check PolicyDebug**
1. Go to `/policydebug`
2. Look for these claims:
   ```
   Permission = Users.View
   Permission = Users.Create
   Permission = Users.Edit
   Permission = Users.Delete
   ```
3. If missing, click "Refresh Claims from DB"

---

## 🔧 **Troubleshooting**

### **Problem: Still can't see buttons**

**Solution 1: Refresh Claims**
```
1. Go to /policydebug
2. Click "Refresh Claims from DB"
3. Go back to /users
```

**Solution 2: Check Role Permissions in Database**
```sql
-- Check if System Administrator role has the permissions
SELECT 
    r.RoleName,
    p.PermissionName
FROM Roles r
INNER JOIN RolePermissions rp ON r.RoleId = rp.RoleId
INNER JOIN Permissions p ON rp.PermissionId = p.PermissionId
WHERE r.RoleName = 'System Administrator';
```

**Solution 3: Re-seed Permissions**
The system automatically seeds permissions on startup. Restart the app to trigger seeding.

---

## 📝 **Quick Reference Table**

| Action | Required Permission | Fallback |
|--------|-------------------|----------|
| View Users | `Users.View` | Always allowed |
| Add User | `Users.Create` | None |
| Edit User | `Users.Edit` | Can edit own profile |
| Delete User | `Users.Delete` | Cannot delete self |
| Restore User | `Users.Delete` | None |
| Show Deleted | `Users.Delete` | None |

---

## ✅ **Summary**

**What was wrong:**
- Checking for `"ManageUsers"` permission (doesn't exist)

**What was fixed:**
- Changed to `"Users.Edit"`, `"Users.Create"`, `"Users.Delete"`
- These permissions exist in `PermissionConstants.cs`
- System Administrator role has all these permissions

**Now it works:**
- ✅ System Administrator can manage users
- ✅ Buttons appear correctly
- ✅ Self-deletion still blocked
- ✅ Permission checks working

**Your System Administrator account should now work perfectly!** 🎉
