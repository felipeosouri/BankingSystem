IF DB_ID('TransactionDb') IS NULL
BEGIN
    CREATE DATABASE [TransactionDb];
END;
GO

USE [TransactionDb];
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE [dbo].[Transactions] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [ExternalId] UNIQUEIDENTIFIER NOT NULL,
        [SourceAccountId] UNIQUEIDENTIFIER NOT NULL,
        [TargetAccountId] UNIQUEIDENTIFIER NOT NULL,
        [TransferTypeId] INT NOT NULL,
        [Value] DECIMAL(18, 2) NOT NULL,
        [Status] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL
    );
END;
GO
