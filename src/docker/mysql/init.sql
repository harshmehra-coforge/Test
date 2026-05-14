-- Create databases for each service
CREATE DATABASE IF NOT EXISTS fnol_user_db;
CREATE DATABASE IF NOT EXISTS fnol_policy_db;
CREATE DATABASE IF NOT EXISTS fnol_claim_db;
CREATE DATABASE IF NOT EXISTS fnol_adjuster_db;
CREATE DATABASE IF NOT EXISTS fnol_coverage_db;
CREATE DATABASE IF NOT EXISTS fnol_report_db;
CREATE DATABASE IF NOT EXISTS fnol_notification_db;

-- Create users for each service
CREATE USER IF NOT EXISTS 'user_service'@'%' IDENTIFIED BY 'user_password';
CREATE USER IF NOT EXISTS 'policy_service'@'%' IDENTIFIED BY 'policy_password';
CREATE USER IF NOT EXISTS 'claim_service'@'%' IDENTIFIED BY 'claim_password';
CREATE USER IF NOT EXISTS 'adjuster_service'@'%' IDENTIFIED BY 'adjuster_password';
CREATE USER IF NOT EXISTS 'coverage_service'@'%' IDENTIFIED BY 'coverage_password';
CREATE USER IF NOT EXISTS 'report_service'@'%' IDENTIFIED BY 'report_password';
CREATE USER IF NOT EXISTS 'notification_service'@'%' IDENTIFIED BY 'notification_password';

-- Grant permissions
GRANT ALL PRIVILEGES ON fnol_user_db.* TO 'user_service'@'%';
GRANT ALL PRIVILEGES ON fnol_policy_db.* TO 'policy_service'@'%';
GRANT ALL PRIVILEGES ON fnol_claim_db.* TO 'claim_service'@'%';
GRANT ALL PRIVILEGES ON fnol_adjuster_db.* TO 'adjuster_service'@'%';
GRANT ALL PRIVILEGES ON fnol_coverage_db.* TO 'coverage_service'@'%';
GRANT ALL PRIVILEGES ON fnol_report_db.* TO 'report_service'@'%';
GRANT ALL PRIVILEGES ON fnol_notification_db.* TO 'notification_service'@'%';

-- Grant read access to report service for analytics
GRANT SELECT ON fnol_policy_db.* TO 'report_service'@'%';
GRANT SELECT ON fnol_claim_db.* TO 'report_service'@'%';
GRANT SELECT ON fnol_adjuster_db.* TO 'report_service'@'%';
GRANT SELECT ON fnol_user_db.* TO 'report_service'@'%';

FLUSH PRIVILEGES;