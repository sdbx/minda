CREATE TABLE users (
    id serial primary key,
    username character varying(255) NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL
);

CREATE TABLE oauth_users {
    oauth_id character varying(255) NOT NULL,
    user_id integer NOT NULL,
    provider character varying(255) NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL
};