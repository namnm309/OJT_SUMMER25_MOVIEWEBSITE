-- Add layout columns to tbl_cinema_rooms
ALTER TABLE tbl_cinema_rooms 
ADD COLUMN IF NOT EXISTS "NumberOfRows" integer NOT NULL DEFAULT 0;

ALTER TABLE tbl_cinema_rooms 
ADD COLUMN IF NOT EXISTS "NumberOfColumns" integer NOT NULL DEFAULT 0;

ALTER TABLE tbl_cinema_rooms 
ADD COLUMN IF NOT EXISTS "DefaultSeatPrice" numeric NOT NULL DEFAULT 100000;

-- Update existing records with default values
UPDATE tbl_cinema_rooms 
SET "NumberOfRows" = 10, "NumberOfColumns" = 10, "DefaultSeatPrice" = 100000 
WHERE "NumberOfRows" = 0 OR "NumberOfColumns" = 0; 