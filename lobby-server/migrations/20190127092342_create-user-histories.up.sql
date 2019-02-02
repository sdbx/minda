CREATE TABLE histories (
    id serial PRIMARY KEY,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    black integer NOT NULL,
    white integer NOT NULL,
    map character varying(1024) NOT NULL
);

CREATE TABLE moves (
    id serial PRIMARY KEY NOT NULL,
    history_id integer REFERENCES histories ON DELETE CASCADE,
    player integer NOT NULL,
    start_cord character varying(100) NOT NULL,
    end_cord character varying(100) NOT NULL,
    dir_cord character varying(100) NOT NULL
);

ALTER TABLE maps
ADD COLUMN created_at timestamp without time zone NOT NULL DEFAULT NOW();

ALTER TABLE maps
ADD COLUMN updated_at timestamp without time zone NOT NULL DEFAULT NOW();