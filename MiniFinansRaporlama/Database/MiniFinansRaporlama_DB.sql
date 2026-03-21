USE master;
GO

IF DB_ID('MiniFinansDB') IS NOT NULL
BEGIN
    ALTER DATABASE MiniFinansDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MiniFinansDB;
END
GO

CREATE DATABASE MiniFinansDB;
GO

USE MiniFinansDB;
GO

/*
    =========================================================
    Mini Finans Raporlama Database Script
    =========================================================
    This script creates the database structure used in the
    Mini Finans Raporlama project.

    Tables:
    1. Transactions -> Stores income and expense records
    2. Logs         -> Stores system action history
*/

-- =========================================================
-- Table: Transactions
-- Stores all financial transaction records
-- =========================================================
CREATE TABLE Transactions
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Date DATETIME NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NULL,
    Category NVARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL
);
GO

-- =========================================================
-- Table: Logs
-- Stores create, update and delete operation history
-- =========================================================
CREATE TABLE Logs
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Action NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL,
    LogDate DATETIME NOT NULL
);
GO

-- Sample transaction data
INSERT INTO Transactions (Date, Type, Amount, Description, Category, CreatedAt, UpdatedAt)
VALUES 
('2026-03-01', 'Gelir', 15000.00, 'Maaş ödemesi', 'Maaş', GETDATE(), NULL),
('2026-03-02', 'Gider', 850.50, 'Market alışverişi', 'Market', GETDATE(), NULL),
('2026-03-03', 'Gider', 420.00, 'Elektrik faturası', 'Fatura', GETDATE(), NULL);
GO

-- Sample log data
INSERT INTO Logs (Action, Description, LogDate)
VALUES
('Create', 'Sample transaction records inserted.', GETDATE());
GO