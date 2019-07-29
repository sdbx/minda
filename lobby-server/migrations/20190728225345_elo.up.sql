CREATE TABLE user_ratings (
    id uuid NOT NULL UNIQUE,
    user_id integer NOT NULL REFERENCES users ON DELETE CASCADE,
    r double precision NOT NULL,
    rd double precision NOT NULL,
    v double precision NOT NULL,
    PRIMARY KEY (user_id)
);

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

INSERT INTO user_ratings (id, user_id, r, rd, v) SELECT (uuid_generate_v4(), id, 1500, 350, 0.06) FROM users;
