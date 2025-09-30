# 🔓 Password Recovery Guide

## Problem: Account Locked After BCrypt Migration

Your admin account was created with SHA256 password hashing, but the system now uses BCrypt. This causes login failures.

---

## ✅ **SOLUTION 1: Auto-Migration (Recommended)**

The system now has **automatic password migration**! Just login normally:

### **How It Works:**
1. You try to login with `admin` / `admin123`
2. System checks BCrypt first (fails)
3. System checks SHA256 (succeeds)
4. **System automatically converts** your password to BCrypt
5. Login succeeds!
6. Next login will use BCrypt

### **Just Login:**
```
Username: admin
Password: admin123
```

**The system will automatically upgrade your password to BCrypt on first login!**

---

## 🔧 **SOLUTION 2: Manual SQL Update**

If auto-migration doesn't work, run the SQL script:

### **Option A: Using SQL Script**
```bash
# Run the provided SQL script
sqlcmd -S your-server -d ModularSys -i FIX_ADMIN_PASSWORD.sql
```

### **Option B: Manual SQL Command**
```sql
USE ModularSys;

UPDATE Users
SET PasswordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIr.5gCKOK'
WHERE Username = 'admin';
```

This sets the admin password to: `admin123` (BCrypt hashed)

---

## 🔍 **Check Current Password Hash**

To see what format your password is in:

```sql
SELECT Username, 
       PasswordHash,
       CASE 
           WHEN PasswordHash LIKE '$2a$%' OR PasswordHash LIKE '$2b$%' THEN 'BCrypt'
           WHEN LEN(PasswordHash) = 44 THEN 'SHA256'
           ELSE 'Unknown'
       END AS HashType
FROM Users
WHERE Username = 'admin';
```

**Hash Types:**
- **BCrypt**: Starts with `$2a$` or `$2b$` (SECURE ✅)
- **SHA256**: 44 characters, base64 encoded (OLD ⚠️)

---

## 🆕 **Create New Admin User**

If all else fails, create a new admin user:

```sql
USE ModularSys;

-- BCrypt hash for password: "NewAdmin123"
INSERT INTO Users (Username, FirstName, LastName, PasswordHash, Email, ContactNumber, RoleId, DepartmentId, CreatedAt)
VALUES (
    'newadmin',
    'New',
    'Administrator',
    '$2a$12$8K1p/a0dL2LkO3Z4xeUJ4.WdRubIv9M7.Cd6/VVgWZCTYbR5Oy6Oy',
    'newadmin@techvault.com',
    '+1-555-0200',
    1,  -- Admin role
    1,  -- Administration department
    GETUTCDATE()
);

-- Verify
SELECT * FROM Users WHERE Username = 'newadmin';
```

**New Credentials:**
```
Username: newadmin
Password: NewAdmin123
```

---

## 🔐 **How Auto-Migration Works**

The `AuthService.cs` now has this logic:

```csharp
private bool VerifyPasswordWithMigration(string password, User user)
{
    // 1. Try BCrypt first (new format)
    if (BCrypt.Verify(password, user.PasswordHash))
        return true;

    // 2. Try SHA256 (old format)
    var sha256Hash = HashPasswordSHA256(password);
    if (user.PasswordHash == sha256Hash)
    {
        // 3. Auto-upgrade to BCrypt
        user.PasswordHash = BCrypt.HashPassword(password);
        _db.SaveChanges();
        return true;
    }

    return false;
}
```

**Benefits:**
- ✅ No manual intervention needed
- ✅ Seamless migration
- ✅ Happens on first login
- ✅ All users automatically upgraded

---

## 📋 **Verification Steps**

After recovery, verify everything works:

### **1. Login Test**
```
1. Go to /login
2. Enter: admin / admin123
3. Should login successfully
4. Check dashboard loads
```

### **2. Check Password Hash**
```sql
SELECT Username, 
       LEFT(PasswordHash, 10) AS HashPrefix,
       CASE 
           WHEN PasswordHash LIKE '$2a$%' THEN 'BCrypt ✅'
           ELSE 'SHA256 ⚠️'
       END AS Status
FROM Users
WHERE Username = 'admin';
```

Should show: `$2a$12$... | BCrypt ✅`

### **3. Test Change Password**
```
1. Go to /settings
2. Click "Change Password"
3. Enter current: admin123
4. Enter new password
5. Should work!
```

---

## 🚨 **Emergency Access**

If you're completely locked out:

### **Option 1: Reset Database**
```bash
# Drop and recreate database
dotnet ef database drop --project ModernSys.Data
dotnet ef database update --project ModernSys.Data
```

### **Option 2: Direct Database Access**
```sql
-- Temporarily set a known BCrypt hash
UPDATE Users
SET PasswordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIr.5gCKOK'
WHERE Id = 1;
```

---

## 📞 **Still Having Issues?**

### **Common Problems:**

**Problem:** "Invalid username or password"
- **Solution:** Try auto-migration (just login with admin/admin123)
- **Or:** Run the SQL update script

**Problem:** "Database connection failed"
- **Solution:** Check `appsettings.json` connection string
- **Verify:** SQL Server is running

**Problem:** "User not found"
- **Solution:** Check if admin user exists:
  ```sql
  SELECT * FROM Users WHERE Username = 'admin';
  ```

---

## ✅ **Summary**

**Easiest Solution:** Just login with `admin` / `admin123`
- System will auto-migrate your password to BCrypt
- No manual steps needed!

**If that fails:** Run `FIX_ADMIN_PASSWORD.sql`

**Your account is NOT bricked!** The auto-migration feature will fix it automatically. 🎉
