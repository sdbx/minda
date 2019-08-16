ALTER TABLE moves
ADD COLUMN game_time double precision NOT NULL DEFAULT 0;

ALTER TABLE moves
ADD COLUMN turn_time double precision NOT NULL DEFAULT 0;

ALTER TABLE histories
ADD COLUMN ranked boolean NOT NULL DEFAULT FALSE;;

CREATE TABLE history_game_rules (
    id uuid NOT NULL,
    history_id integer PRIMARY KEY REFERENCES histories ON DELETE CASCADE,
    defeat_lost_stones integer NOT NULL,
    turn_timeout integer NOT NULL,
    game_timeout integer NOT NULL
);

INSERT INTO history_game_rules (id, history_id, defeat_lost_stones, turn_timeout, game_timeout) SELECT uuid_generate_v4(), id, 6, 0, 0 FROM histories;
