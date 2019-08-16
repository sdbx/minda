DROP TABLE moves;
DROP TABLE histories;

ALTER TABLE maps
DROP COLUMN updated_at;

ALTER TABLE maps
DROP COLUMN created_at;