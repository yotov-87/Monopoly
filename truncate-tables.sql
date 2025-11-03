-- Script to truncate all tables while preserving the database structure
-- This removes all data but keeps the schema intact

-- Disable foreign key checks temporarily
SET session_replication_role = 'replica';

-- Truncate tables in reverse dependency order
TRUNCATE TABLE "BoardCells" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "GameBoards" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "GamePlayers" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Games" RESTART IDENTITY CASCADE;
TRUNCATE TABLE "Users" RESTART IDENTITY CASCADE;

-- Re-enable foreign key checks
SET session_replication_role = 'origin';

-- Verify all tables are empty
SELECT 'Users' as table_name, COUNT(*) as row_count FROM "Users"
UNION ALL
SELECT 'Games', COUNT(*) FROM "Games"
UNION ALL
SELECT 'GamePlayers', COUNT(*) FROM "GamePlayers"
UNION ALL
SELECT 'GameBoards', COUNT(*) FROM "GameBoards"
UNION ALL
SELECT 'BoardCells', COUNT(*) FROM "BoardCells";
