CREATE TABLE user_connection_events (
    user_id BIGINT NOT NULL,
    ip_address INET NOT NULL,
    last_connected TIMESTAMPTZ NOT NULL,
    PRIMARY KEY (user_id, ip_address)
);