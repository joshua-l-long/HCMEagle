-- Create schemas
CREATE SCHEMA IF NOT EXISTS eaglehr;
CREATE SCHEMA IF NOT EXISTS hrreporting;

-- Grant full permissions to testuser
GRANT ALL PRIVILEGES ON SCHEMA eaglehr TO testuser;
GRANT ALL PRIVILEGES ON SCHEMA hrreporting TO testuser;

-- Grant usage and create on schemas
GRANT USAGE ON SCHEMA eaglehr TO testuser;
GRANT USAGE ON SCHEMA hrreporting TO testuser;
GRANT CREATE ON SCHEMA eaglehr TO testuser;
GRANT CREATE ON SCHEMA hrreporting TO testuser;

-- Grant default privileges for future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA eaglehr GRANT ALL ON TABLES TO testuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA hrreporting GRANT ALL ON TABLES TO testuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA eaglehr GRANT ALL ON SEQUENCES TO testuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA hrreporting GRANT ALL ON SEQUENCES TO testuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA eaglehr GRANT ALL ON FUNCTIONS TO testuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA hrreporting GRANT ALL ON FUNCTIONS TO testuser;
