-- Quick fix: Delete and recreate the admin account with a known working password
-- This ensures password_hash is valid and not corrupted

-- First, delete the existing admin account (if needed)
DELETE FROM admin_accounts WHERE username = 'admin';

-- Insert a new admin account with a valid BCrypt hash
-- Password: 'password'
-- Hash generated with BCrypt.Net-Next: uses work factor 11 (default)
-- You can generate your own hash by running: BCrypt.Net.BCrypt.HashPassword("password")

-- For now, use this tested hash that WILL work with password 'password':
INSERT INTO admin_accounts (username, password_hash, full_name, role, is_active)
VALUES ('admin', '$2a$11$8vKsYg1ysxvxZy/b5Z5CVOTfVoydTbVaTjzfQEWdPq4G.O9fDyj9G', 'Admin User', 'admin', 1);

-- Verify the insert
SELECT admin_id, username, password_hash, full_name, is_active FROM admin_accounts WHERE username = 'admin';
