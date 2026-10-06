# Create the Digital Notice Board database

Execute the following with a SQL Server login that can create databases and
schemas.

```sql
IF DB_ID(N'DigitalNoticeBoard') IS NULL
BEGIN
    CREATE DATABASE DigitalNoticeBoard;
END;
GO

USE DigitalNoticeBoard;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_Users PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL,
        DisplayName NVARCHAR(160) NOT NULL,
        PasswordHash NVARCHAR(512) NOT NULL,
        Role NVARCHAR(32) NOT NULL,
        IsActive BIT NOT NULL
            CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CONSTRAINT UQ_Users_Username UNIQUE (Username),
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'Viewer'))
    );
END;
GO

IF OBJECT_ID(N'dbo.Notices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notices
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_Notices PRIMARY KEY,
        Title NVARCHAR(160) NOT NULL,
        Summary NVARCHAR(500) NOT NULL
            CONSTRAINT DF_Notices_Summary DEFAULT (N''),
        Content NVARCHAR(4000) NOT NULL,
        Category NVARCHAR(100) NOT NULL
            CONSTRAINT DF_Notices_Category DEFAULT (N'General'),
        Priority INT NOT NULL
            CONSTRAINT DF_Notices_Priority DEFAULT (1),
        PublishFromUtc DATETIME2 NOT NULL,
        PublishUntilUtc DATETIME2 NULL,
        Status NVARCHAR(24) NOT NULL,
        CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL,
        UpdatedAtUtc DATETIME2 NOT NULL,
        CONSTRAINT FK_Notices_Users_CreatedByUserId
            FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(Id),
        CONSTRAINT CK_Notices_Priority CHECK (Priority BETWEEN 1 AND 5),
        CONSTRAINT CK_Notices_Status
            CHECK (Status IN (N'Draft', N'Published', N'Archived')),
        CONSTRAINT CK_Notices_PublishWindow
            CHECK (PublishUntilUtc IS NULL OR PublishUntilUtc > PublishFromUtc)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Notices_Status_PublishFromUtc_PublishUntilUtc'
      AND object_id = OBJECT_ID(N'dbo.Notices')
)
BEGIN
    CREATE INDEX IX_Notices_Status_PublishFromUtc_PublishUntilUtc
        ON dbo.Notices (Status, PublishFromUtc, PublishUntilUtc)
        INCLUDE (Priority, Category, Title);
END;
GO
```

The application stores ASP.NET password hashes rather than plaintext
passwords. Let the API create the initial local accounts instead of inserting
passwords manually.
