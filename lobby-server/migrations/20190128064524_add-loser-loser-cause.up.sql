ALTER TABLE histories
ADD COLUMN loser integer NOT NULL DEFAULT -1;

ALTER TABLE histories
ADD COLUMN cause character varying(100) NOT NULL DEFAULT '';