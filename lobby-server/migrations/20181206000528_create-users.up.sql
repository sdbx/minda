CREATE TABLE users (
    id serial PRIMARY KEY,
    username character varying(255) NOT NULL,
    picture character varying(255),
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL
);

CREATE TABLE oauth_users (
    provider character varying(255) NOT NULL,
    id character varying(255) NOT NULL,
    user_id integer REFERENCES users ON DELETE CASCADE,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    PRIMARY KEY (provider, id)
);