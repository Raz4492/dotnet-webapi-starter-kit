-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ShineCoder')
BEGIN
    CREATE DATABASE ShineCoder;
END
GO

USE ShineCoder;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) UNIQUE NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        Role NVARCHAR(20) DEFAULT 'User'
    );
END
GO

-- Create RefreshTokens table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Token NVARCHAR(255) UNIQUE NOT NULL,
        UserId INT NOT NULL,
        ExpiryDate DATETIME2 NOT NULL,
        IsRevoked BIT DEFAULT 0,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedByIp NVARCHAR(45) NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username')
BEGIN
    CREATE INDEX IX_Users_Username ON Users(Username);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
BEGIN
    CREATE INDEX IX_Users_Email ON Users(Email);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_Token')
BEGIN
    CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_UserId')
BEGIN
    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
END
GO

-- Insert a default admin user (password: Admin123!)
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role, CreatedAt)
    VALUES (
        'admin', 
        'admin@smartapi.com', 
        '$2a$11$rQQbYZ8QhSxQAhP5UGLk1eYH5L5pQrF5sVmHcG8QNvR6xKQ4Qa4Xe', -- Admin123!
        'System',
        'Administrator',
        'Admin',
        GETUTCDATE()
    );
END
GO

PRINT 'Database setup completed successfully!';