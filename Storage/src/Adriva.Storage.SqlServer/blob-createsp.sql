IF NOT EXISTS (
SELECT * FROM INFORMATION_SCHEMA.ROUTINES
    WHERE SPECIFIC_SCHEMA = N'{SCHEMA}'
    AND SPECIFIC_NAME = N'{PROC_UPSERT}'
    AND ROUTINE_TYPE = N'PROCEDURE'
)
BEGIN
    EXEC sp_executesql N'CREATE PROCEDURE {SCHEMA}.{PROC_UPSERT}
         @id BIGINT,
         @containerName NVARCHAR(1024),
         @name NVARCHAR(1024),
         @data VARBINARY(MAX),
         @length BIGINT,
         @etag VARCHAR(100)
    AS
    DECLARE @now DATETIME2 = GETUTCDATE() 

    IF EXISTS (SELECT * FROM {SCHEMA}.{TABLE} WHERE Id = @id)
    BEGIN
        UPDATE {SCHEMA}.{TABLE} SET [Content] = @data, [Length] = @length, [ETag] = @etag, [LastModifiedUtc] = @now WHERE Id = @id
    END
    ELSE
    BEGIN
        INSERT INTO {SCHEMA}.{TABLE} (ContainerName, [Name], [Content], [Length], [ETag], [LastModifiedUtc])
        VALUES (@containerName, @name, @data, @length, @etag, @now)
    END';
END


IF NOT EXISTS (
SELECT * FROM INFORMATION_SCHEMA.ROUTINES
    WHERE SPECIFIC_SCHEMA = N'{SCHEMA}'
    AND SPECIFIC_NAME = N'{PROC_UPDATE}'
    AND ROUTINE_TYPE = N'PROCEDURE'
)
BEGIN
EXEC sp_executesql N'CREATE PROCEDURE {SCHEMA}.{PROC_UPDATE}
         @id BIGINT,
         @data VARBINARY(MAX),
         @length BIGINT,
         @etag VARCHAR(100),
         @matchEtag VARCHAR(100)
    AS
    DECLARE @now DATETIME2 = GETUTCDATE() 

    UPDATE {SCHEMA}.{TABLE} SET Content = @data, [Length] = @length, ETag = @etag, LastModifiedUtc = @now
    WHERE Id = @id AND (@matchEtag = ''*'' OR ETag = @matchEtag)';
END