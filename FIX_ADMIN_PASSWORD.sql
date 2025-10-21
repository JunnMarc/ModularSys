-- Fix Admin Password - Update to SHA256 (auto-migrates to BCrypt on login)
-- This script sets the admin user's password hash to SHA256 for: administrator12
-- On next successful login, the application will automatically migrate it to BCrypt.

USE ModularSys;
GO

-- Update admin user password to SHA256 Base64 of "administrator12"
-- SHA256("administrator12") Base64: idH7w5EDU8HJlFmLjHS9xEhJ5wYCdEcEuhpQoCi1C3s=
UPDATE Users
SET PasswordHash = 'idH7w5EDU8HJlFmLjHS9xEhJ5wYCdEcEuhpQoCi1C3s='
WHERE Username = 'admin';

-- Verify the update
SELECT Id, Username, PasswordHash, Email
FROM Users
WHERE Username = 'admin';

PRINT 'Admin password updated to SHA256 hash (password: administrator12). App will auto-migrate to BCrypt on login.';
GO

/*
NOTES:
- This sets the admin password to: "administrator12" using SHA256 Base64.
- After running this script, you can login with:
  Username: admin
  Password: administrator12

- On successful login, the application detects SHA256 and automatically
  migrates the password to BCrypt (workFactor: 12).
*/
