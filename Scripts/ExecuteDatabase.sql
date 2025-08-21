/*
    SmartAPI Database Setup Script
    - Creates Users & RefreshTokens tables
    - Adds indexes
    - Seeds default admin user (username: admin, password: Admin123!)
*/

-- Create Users table
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
    PRINT 'Users table already exists';

-- Create RefreshTokens table
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
    PRINT 'RefreshTokens table already exists';

-- Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('Users'))
    CREATE INDEX IX_Users_Username ON Users(Username);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
    CREATE INDEX IX_Users_Email ON Users(Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_Token' AND object_id = OBJECT_ID('RefreshTokens'))
    CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RefreshTokens_UserId' AND object_id = OBJECT_ID('RefreshTokens'))
    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);

-- Insert default admin user
-- BCrypt hash corresponds to password "Admin123!"
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
    PRINT 'Admin user already exists';

PRINT '✅ SmartAPI database setup completed successfully!';
