DO $$

BEGIN

-- Schemas
CREATE SCHEMA IF NOT EXISTS fts;

-- Sequences
CREATE SEQUENCE IF NOT EXISTS fts.user_seq
    start 38187
    increment 1
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE IF NOT EXISTS fts.outbox_event_seq
    start 38187
    increment 1
    NO MAXVALUE
    CACHE 1;
    
CREATE SEQUENCE IF NOT EXISTS fts.document_seq
    start 38187
    increment 1
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE IF NOT EXISTS fts.document_keyword_seq
    start 38187
    increment 1
    NO MAXVALUE
    CACHE 1;
    
CREATE SEQUENCE IF NOT EXISTS fts.document_suggestion_seq
    start 38187
    increment 1
    NO MAXVALUE
    CACHE 1;
    
CREATE SEQUENCE IF NOT EXISTS fts.job_seq
    start 38187
    increment 1
    NO MAXVALUE
    CACHE 1;
    
CREATE SEQUENCE IF NOT EXISTS fts.keyword_seq
    start 38187
    increment 1
    NO MAXVALUE
    CACHE 1;
    
CREATE SEQUENCE IF NOT EXISTS fts.suggestion_seq
    start 38187
    increment 1
    NO MAXVALUE
    CACHE 1;    

-- Tables
CREATE TABLE IF NOT EXISTS fts.user (
    user_id integer default nextval('fts.user_seq'),
    email varchar(2000) not null,
    preferred_name varchar(2000) not null,
    last_edited_by integer not null,
    sys_period tstzrange not null default tstzrange(current_timestamp, null),
    CONSTRAINT user_pkey
        PRIMARY KEY (user_id),
    CONSTRAINT user_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES fts.user(user_id)
);

CREATE TABLE IF NOT EXISTS fts.document (
    document_id integer default nextval('fts.document_seq'),
    title varchar(2000) not null,
    filename varchar(2000) not null,
    data bytea not null,
    uploaded_at timestamptz not null,
    indexed_at timestamptz null,
    last_edited_by integer not null,
    sys_period tstzrange not null default tstzrange(current_timestamp, null),
    CONSTRAINT document_pkey
        PRIMARY KEY (document_id),
    CONSTRAINT document_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES fts.user(user_id)
);

CREATE TABLE IF NOT EXISTS fts.keyword (
    keyword_id integer default nextval('fts.keyword_seq'),
    name varchar(255) not null,
    last_edited_by integer not null,
    sys_period tstzrange not null default tstzrange(current_timestamp, null),
    CONSTRAINT keyword_pkey
        PRIMARY KEY (keyword_id),
    CONSTRAINT keyword_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES fts.user(user_id)
);

CREATE TABLE IF NOT EXISTS fts.suggestion (
    suggestion_id integer default nextval('fts.suggestion_seq'),
    name varchar(255) not null,
    last_edited_by integer not null,
    sys_period tstzrange not null default tstzrange(current_timestamp, null),
    CONSTRAINT suggestion_pkey
        PRIMARY KEY (suggestion_id),
    CONSTRAINT suggestion_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES fts.user(user_id)
);

CREATE TABLE IF NOT EXISTS fts.document_keyword (
    document_keyword_id integer default nextval('fts.document_keyword_seq'),
    document_id int not null,
    keyword_id int not null,
    last_edited_by integer not null,
    sys_period tstzrange not null default tstzrange(current_timestamp, null),
    CONSTRAINT document_keyword_pkey
        PRIMARY KEY (document_keyword_id),
    CONSTRAINT document_keyword_document_id_fkey 
        FOREIGN KEY (document_id)
        REFERENCES fts.document(document_id),
    CONSTRAINT document_keyword_keyword_id_fkey 
        FOREIGN KEY (keyword_id)
        REFERENCES fts.keyword(keyword_id),
    CONSTRAINT document_keyword_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES fts.user(user_id)
);

CREATE TABLE IF NOT EXISTS fts.document_suggestion (
    document_suggestion_id integer default nextval('fts.document_suggestion_seq'),
    document_id int not null,
    suggestion_id int not null,
    last_edited_by integer not null,
    sys_period tstzrange not null default tstzrange(current_timestamp, null),
    CONSTRAINT document_suggestion_pkey
        PRIMARY KEY (document_suggestion_id),
    CONSTRAINT document_suggestion_document_id_fkey 
        FOREIGN KEY (document_id)
        REFERENCES fts.document(document_id),
    CONSTRAINT document_suggestion_suggestion_id_fkey 
        FOREIGN KEY (suggestion_id)
        REFERENCES fts.suggestion(suggestion_id),
    CONSTRAINT document_suggestion_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES fts.user(user_id)
);

CREATE TABLE IF NOT EXISTS fts.outbox_event (
    outbox_event_id integer default nextval('fts.outbox_event_seq'),
    correlation_id_1 varchar(2000) null,
    correlation_id_2 varchar(2000) null,
    correlation_id_3 varchar(2000) null,
    correlation_id_4 varchar(2000) null,
    event_time timestamptz not null,
    event_source varchar(2000) not null,
    event_type varchar(255) not null,
    payload JSONB not null,
    last_edited_by integer not null,
    sys_period tstzrange not null default tstzrange(current_timestamp, null),
    CONSTRAINT outbox_event_pkey
        PRIMARY KEY (outbox_event_id),
    CONSTRAINT outbox_event_last_edited_by_fkey 
        FOREIGN KEY (last_edited_by)
        REFERENCES fts.user(user_id)
);

-- Indexes
CREATE UNIQUE INDEX IF NOT EXISTS user_email_key 
    ON fts.user(email);

CREATE UNIQUE INDEX IF NOT EXISTS suggestion_name_key 
    ON fts.suggestion(name);

CREATE UNIQUE INDEX IF NOT EXISTS keyword_name_key 
    ON fts.keyword(name);

CREATE UNIQUE INDEX IF NOT EXISTS document_suggestion_document_id_suggestion_id_key 
    ON fts.document_suggestion(document_id, suggestion_id);

CREATE UNIQUE INDEX IF NOT EXISTS document_keyword_document_id_keyword_id_key 
    ON fts.document_keyword(document_id, keyword_id);

END;
$$ LANGUAGE plpgsql;
