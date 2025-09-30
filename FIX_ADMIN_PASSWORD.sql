-- Fix Admin Password - Update to BCrypt Hash
-- This script updates the admin user's password from SHA256 to BCrypt
-- Password: admin123
-- BCrypt Hash (workFactor: 12)

USE ModularSys;
GO

-- Update admin user password to BCrypt hash
UPDATE Users
SET PasswordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIr.5gCKOK'
WHERE Username = 'admin';

-- Verify the update
SELECT Id, Username, PasswordHash, Email
FROM Users
WHERE Username = 'admin';

PRINT 'Admin password updated to BCrypt hash (password: admin123)';
GO

/*
NOTES:
- This BCrypt hash is for password: "admin123"
- The hash was generated with workFactor: 12
- After running this script, you can login with:
  Username: admin
  Password: admin123
  
- The system will automatically migrate any remaining SHA256 passwords
  to BCrypt on first login (auto-migration feature)
*/
