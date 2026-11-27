-- Schema for inventory-lab. The domain classes are the source of truth;
-- constraints here repeat critical rules as defense in depth.
CREATE TABLE IF NOT EXISTS stock_items (
    sku      varchar(32) PRIMARY KEY CHECK (sku ~ '^[A-Z0-9-]{1,32}$'),
    on_hand  integer     NOT NULL CHECK (on_hand >= 0),  -- stock on hand can never go negative
    version  integer     NOT NULL CHECK (version >= 1)
);
