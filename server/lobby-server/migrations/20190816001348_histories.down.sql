DROP TABLE history_game_rules;

ALTER TABLE moves
DROP COLUMN turn_time;

ALTER TABLE moves
DROP COLUMN game_time;