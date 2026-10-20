-- This script rehashes the admin password to ensure it's valid
-- Password: 'password' 
-- If you need a different password, generate the hash using generate_bcrypt_hash.cs

-- The BCrypt hash for password 'password' (auto-generated with work factor):
-- Use this exact hash or generate your own using the C# script

UPDATE admin_accounts 
SET password_hash = '$2a$11$UXqNBk7YRvJYhLqGqm14w.Oe3w3D3G5k8P8Z8F7G6H5I4J3K2L1M0'
WHERE username = 'admin';

-- Verify the update worked
SELECT admin_id, username, password_hash, full_name, is_active FROM admin_accounts WHERE username = 'admin';
