IF NOT EXISTS (
SELECT * FROM INFORMATION_SCHEMA.ROUTINES
    WHERE SPECIFIC_SCHEMA = N'dbo'
    AND SPECIFIC_NAME = N'UpsertBlobItem'
    AND ROUTINE_TYPE = N'PROCEDURE'
)
BEGIN
    EXEC sp_executesql N'CREATE PROCEDURE UpsertBlobItem
         @id BIGINT,
         @containerName NVARCHAR(1024),
         @name NVARCHAR(1024),
         @data VARBINARY(MAX),
         @length BIGINT,
         @etag VARCHAR(100)
    AS
    DECLARE @now DATETIME2 = GETUTCDATE() 

    IF EXISTS (SELECT * FROM BlobItems WHERE Id = @id)
    BEGIN
        UPDATE BlobItems SET [Content] = @data, [Length] = @length, [ETag] = @etag, [LastModifiedUtc] = @now WHERE Id = @id
    END
    ELSE
    BEGIN
        INSERT INTO BlobItems (ContainerName, [Name], [Content], [Length], [ETag], [LastModifiedUtc])
        VALUES (@containerName, @name, @data, @length, @etag, @now)
    END';
END


IF NOT EXISTS (
SELECT * FROM INFORMATION_SCHEMA.ROUTINES
    WHERE SPECIFIC_SCHEMA = N'dbo'
    AND SPECIFIC_NAME = N'UpdateBlobItem'
    AND ROUTINE_TYPE = N'PROCEDURE'
)
BEGIN
EXEC sp_executesql N'CREATE PROCEDURE [dbo].[UpdateBlobItem]
         @id BIGINT,
         @data VARBINARY(MAX),
         @length BIGINT,
         @etag VARCHAR(100),
         @matchEtag VARCHAR(100)
    AS
    DECLARE @now DATETIME2 = GETUTCDATE() 

    UPDATE BlobItems SET Content = @data, [Length] = @length, ETag = @etag, LastModifiedUtc = @now
    WHERE Id = @id AND (@matchEtag = ''*'' OR ETag = @matchEtag)';
END