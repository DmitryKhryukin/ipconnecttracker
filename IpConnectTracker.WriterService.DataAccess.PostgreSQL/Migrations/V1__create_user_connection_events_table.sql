CREATE TABLE user_connection_events (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    ip TEXT NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL
);