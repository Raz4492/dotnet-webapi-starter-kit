/*
  # Create SmartAPI Database Tables

  1. New Tables
    - `Users`
      - `Id` (int, primary key, identity)
      - `Username` (nvarchar(50), unique)
      - `Email` (nvarchar(100))
      - `PasswordHash` (nvarchar(255))
      - `FirstName` (nvarchar(50))
      - `LastName` (nvarchar(50))
      - `IsActive` (bit, default true)
      - `CreatedAt` (datetime2, default current time)
      - `UpdatedAt` (datetime2, nullable)
      - `Role` (nvarchar(20), default 'User')
    
    - `RefreshTokens`
      - `Id` (int, primary key, identity)
      - `Token` (nvarchar(255), unique)
      - `UserId` (int, foreign key to Users)
      - `ExpiryDate` (datetime2)
      - `IsRevoked` (bit, default false)
      - `CreatedAt` (datetime2, default current time)
      - `CreatedByIp` (nvarchar(45))

  2. Indexes
    - Performance indexes on Username, Email, Token, and UserId
    
  3. Default Data
    - Admin user with credentials (admin/Admin123!)
*/

-- Create Users table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND type = 'U')
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
    PRINT 'Users table created successfully';
END
ELSE
BEGIN
    PRINT 'Users table already exists';
END

-- Create RefreshTokens table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens' AND type = 'U')
BEGIN
    CREATE TABLE RefreshTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Token NVARCHAR(255) UNIQUE NOT NULL,
        UserId INT NOT NULL,
        ExpiryDate DATETIME2 NOT NULL,
        IsRevoked BIT DEFAULT 0,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedByIp NVARCHAR(45) NULL,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    PRINT 'RefreshTokens table created successfully';
END
ELSE
BEGIN
    PRINT 'RefreshTokens table already exists';
END

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX IX_Users_Username ON Users(Username);
    PRINT 'Index IX_Users_Username created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX IX_Users_Email ON Users(Email);
    PRINT 'Index IX_Users_Email created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_Token' AND object_id = OBJECT_ID('RefreshTokens'))
BEGIN
    CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
    PRINT 'Index IX_RefreshTokens_Token created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_UserId' AND object_id = OBJECT_ID('RefreshTokens'))
BEGIN
    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
    PRINT 'Index IX_RefreshTokens_UserId created';
END

-- Insert default admin user if it doesn't exist
-- Password hash for 'Admin123!' using BCrypt
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
    PRINT 'Default admin user created successfully';
END
ELSE
BEGIN
    PRINT 'Admin user already exists';
END

PRINT 'SmartAPI database setup completed successfully!';