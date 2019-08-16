CREATE TABLE user_permissions (
    id uuid NOT NULL,
    user_id integer PRIMARY KEY REFERENCES users ON DELETE CASCADE,
    admin boolean NOT NULL
);