IF NOT EXISTS (
SELECT * FROM INFORMATION_SCHEMA.ROUTINES
    WHERE SPECIFIC_SCHEMA = N'{SCHEMA}'
    AND SPECIFIC_NAME = N'{PROC_RETRIEVE}'
    AND ROUTINE_TYPE = N'PROCEDURE'
)
BEGIN
    EXEC sp_executesql N'CREATE PROCEDURE {SCHEMA}.{PROC_RETRIEVE}
         @environment NVARCHAR(50),
         @application NVARCHAR(50)
        AS
        DECLARE @now DATETIME2 = GETUTCDATE()
        DECLARE @nextId BIGINT

        DELETE {TABLE} WHERE DATEADD(SECOND, TimeToLive, TimestampUtc) < @now

        SET @nextId = (SELECT TOP 1 Id
        FROM {TABLE} WITH (UPDLOCK, READPAST)
        WHERE Id IN (
            SELECT Id
                FROM {TABLE} WITH (INDEX(IX_{TABLE}_Retrieve))
                WHERE Environment = @environment
                AND Application = @application
                AND TimestampUtc <= @now
                AND DATEADD (SECOND, TimeToLive, TimestampUtc) >= @now 
                AND
                    (
                        RetrievedOnUtc IS NULL
                        OR DATEADD(SECOND, VisibilityTimeout, RetrievedOnUtc) <= @now
                    )    
            )
        ORDER BY Flags & 3 DESC, Id ASC)
        
        UPDATE {TABLE} SET RetrievedOnUtc = GETUTCDATE() WHERE Id = @nextId
        
        SELECT * FROM {TABLE} WITH (NOLOCK) WHERE [Id] = @nextId'
END
