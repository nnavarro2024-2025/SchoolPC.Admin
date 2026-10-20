-- ============================================================================
-- Migration 001: PC Groups feature
-- ============================================================================
-- This migration adds the pc_groups table and group/sort columns to pc_instances.
-- Do NOT run this on an empty database; use database_setup.sql first.
-- ============================================================================

CREATE TABLE pc_groups (
	group_id   INT AUTO_INCREMENT PRIMARY KEY,
	group_name VARCHAR(100) NOT NULL UNIQUE,
	sort_order INT NOT NULL DEFAULT 0,
	created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

ALTER TABLE pc_instances
	ADD COLUMN group_id   INT NULL,
	ADD COLUMN sort_order INT NOT NULL DEFAULT 0,
	ADD CONSTRAINT fk_pc_group FOREIGN KEY (group_id)
		REFERENCES pc_groups(group_id) ON DELETE SET NULL;
