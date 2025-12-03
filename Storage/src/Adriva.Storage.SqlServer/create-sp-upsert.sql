
IF OBJECT_ID('[$SchemaName$].[$UpsertName$]', 'P') IS NULL
BEGIN
    EXEC sp_executesql N'CREATE PROCEDURE [$SchemaName$].[$UpsertName$]
        @id [bigint],
        @container [varchar](255),
        @name [nvarchar](255),
        @etag [varchar](100),
        @mimeType [varchar](100),
        @properties [bigint],
        @contentData [varbinary](MAX),
        @insertedId [bigint] OUTPUT
    AS
    BEGIN
        DECLARE @rowId UNIQUEIDENTIFIER
        SET @container = ISNULL(@container, ''/'')
        SET @mimeType = ISNULL(@mimeType, ''application/octet-stream'')
        SET @properties = ISNULL(@properties, 0)
        SET @rowId = NEWID()

        MERGE [$SchemaName$].[$TableName$] AS target
        USING (
            SELECT
                @id Id,
                @container Container,
                @name [Name],
                @etag [ETag],
                @mimeType [MimeType],
                @properties Properties,
                @contentData ContentData,
                GETUTCDATE() ModifiedOn
        ) AS source
        ON target.Id = source.Id
        WHEN NOT MATCHED BY target AND @id IS NULL OR 0 = @id
        THEN
            INSERT (RowId, Container, [Name], [ETag], [MimeType], [ContentData], Properties, ModifiedOn)
            VALUES (@rowId, @container, @name, @etag, @mimeType, @contentData, @properties, GETUTCDATE())
        WHEN MATCHED
        THEN
            UPDATE SET 
                Container = @container,
                [Name] = @name,
                ETag = @etag,
                MimeType = @mimeType,
                Properties = @properties,
                ModifiedOn = GETUTCDATE(),
                ContentData = COALESCE(source.ContentData, target.ContentData)        
        ;

        SELECT @insertedId = COALESCE(SCOPE_IDENTITY(), @id)
    END'
END