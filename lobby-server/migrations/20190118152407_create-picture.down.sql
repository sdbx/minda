ALTER TABLE users
DROP COLUMN picture;

ALTER TABLE users
ADD COLUMN picture character varying(255);

DROP TABLE pictures;