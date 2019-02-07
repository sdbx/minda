CREATE TABLE user_inventories (
    id uuid NOT NULL,
    user_id integer PRIMARY KEY REFERENCES users ON DELETE CASCADE,
    one_color_skin integer NOT NULL DEFAULT 0,
    two_color_skin integer NOT NULL DEFAULT 0
);

CREATE TABLE skins (
    id serial PRIMARY KEY,
    name character varying(255) NOT NULL,
    black_picture character varying(255),
    white_picture character varying(255)
);

CREATE TABLE user_skins (
    id serial PRIMARY KEY,
    user_id integer REFERENCES users ON DELETE CASCADE,
    skin_id integer REFERENCES skins ON DELETE CASCADE
    PRIMARY KEY (user_id, map_id)
);

INSERT INTO user_inventories (user_id)
  SELECT id
  FROM users;