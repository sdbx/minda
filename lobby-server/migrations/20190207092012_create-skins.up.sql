CREATE TABLE skins (
    id serial PRIMARY KEY,
    name character varying(255) NOT NULL,
    black_picture character varying(255) NOT NULL,
    white_picture character varying(255) NOT NULL
);

CREATE TABLE user_inventories (
    id uuid NOT NULL,
    user_id integer PRIMARY KEY REFERENCES users ON DELETE CASCADE,
    one_color_skin integer NOT NULL DEFAULT 0,
    two_color_skin integer NOT NULL DEFAULT 0,
    current_skin integer DEFAULT NULL
);

CREATE TABLE user_skins (
    id uuid NOT NULL,
    user_id integer REFERENCES users ON DELETE CASCADE,
    skin_id integer REFERENCES skins ON DELETE CASCADE,
    PRIMARY KEY (user_id, skin_id)
);