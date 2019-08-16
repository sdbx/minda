CREATE TABLE maps (
    id serial PRIMARY KEY,
    name character varying(255) NOT NULL,
    payload character varying(1024) NOT NULL,
    public boolean NOT NULL
);

CREATE TABLE user_maps (
    id uuid NOT NULL,
    user_id integer NOT NULL REFERENCES users ON DELETE CASCADE,
    map_id integer NOT NULL REFERENCES maps ON DELETE CASCADE,
    PRIMARY KEY (user_id, map_id)
);