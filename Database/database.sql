CREATE DATABASE DocumentManagement;
GO

USE DocumentManagement;
GO

CREATE TABLE [dbo].[Documents]
(
    DocumentId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    FileName NVARCHAR(255) NOT NULL,
    StoredFileName NVARCHAR(255) NOT NULL,
    ContentType NVARCHAR(150) NOT NULL,
    FileSize BIGINT NOT NULL,
    UploadDate DATETIME2 NOT NULL
);

CREATE INDEX IX_Documents_UploadDate
ON Documents (UploadDate);
GO