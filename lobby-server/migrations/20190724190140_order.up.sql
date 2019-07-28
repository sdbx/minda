CREATE TABLE order_ids (
    id serial PRIMARY KEY,
    last_id integer NOT NULL
);

INSERT INTO order_ids (id, last_id) VALUES (1,10);

CREATE TABLE order_logs (
    id serial PRIMARY KEY,
    user_id integer NOT NULL,
    order_id integer NOT NULL,
    dif integer NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL
);

CREATE TABLE skin_logs (
    id serial PRIMARY KEY,
    user_id integer NOT NULL,
    dif integer NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL
);
