ALTER TABLE users
DROP COLUMN picture;

ALTER TABLE users
ADD COLUMN picture integer;

CREATE TABLE pictures (
    id serial PRIMARY KEY,
    payload bytea NOT NULL
);