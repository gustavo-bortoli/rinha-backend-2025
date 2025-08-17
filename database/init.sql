CREATE UNLOGGED TABLE payments (
    correlation_id UUID PRIMARY KEY,
    amount NUMERIC(18,2) NOT NULL,
    requested_at TIMESTAMP NOT NULL,
    sent_to VARCHAR(20) NOT NULL
);

CREATE INDEX idx_payments_requested_at
ON payments (requested_at);