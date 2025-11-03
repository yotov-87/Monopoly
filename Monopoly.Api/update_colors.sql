-- Update color groups for existing board cells
UPDATE "BoardCells" SET "ColorGroup" = 'Brown' WHERE "Position" IN (1, 3) AND "CellType" = 1;
UPDATE "BoardCells" SET "ColorGroup" = 'LightBlue' WHERE "Position" IN (6, 8, 9) AND "CellType" = 1;
UPDATE "BoardCells" SET "ColorGroup" = 'Pink' WHERE "Position" IN (11, 13, 14) AND "CellType" = 1;
UPDATE "BoardCells" SET "ColorGroup" = 'Orange' WHERE "Position" IN (16, 18, 19) AND "CellType" = 1;
UPDATE "BoardCells" SET "ColorGroup" = 'Red' WHERE "Position" IN (21, 23, 24) AND "CellType" = 1;
UPDATE "BoardCells" SET "ColorGroup" = 'Yellow' WHERE "Position" IN (26, 27, 29) AND "CellType" = 1;
UPDATE "BoardCells" SET "ColorGroup" = 'Green' WHERE "Position" IN (31, 32, 34) AND "CellType" = 1;
UPDATE "BoardCells" SET "ColorGroup" = 'DarkBlue' WHERE "Position" IN (37, 39) AND "CellType" = 1;
