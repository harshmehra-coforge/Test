-- Create database if it doesn't exist
CREATE DATABASE IF NOT EXISTS wealth_management;

-- Create user if it doesn't exist
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles
      WHERE  rolname = 'wealth_user') THEN

      CREATE ROLE wealth_user LOGIN PASSWORD 'wealth_password';
   END IF;
END
$do$;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE wealth_management TO wealth_user;

-- Connect to the database
\c wealth_management;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Grant schema privileges
GRANT ALL ON SCHEMA public TO wealth_user;