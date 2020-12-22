IF NOT EXISTS (
SELECT * FROM INFORMATION_SCHEMA.ROUTINES
    WHERE SPECIFIC_SCHEMA = N'dbo'
    AND SPECIFIC_NAME = N'GetNextQueueMessage'
    AND ROUTINE_TYPE = N'PROCEDURE'
)
BEGIN
    EXEC sp_executesql N'CREATE PROCEDURE GetNextQueueMessage
         @environment NVARCHAR(50)
    AS
    DECLARE @now DATETIME2 = GETUTCDATE() 

    SELECT TOP 1 *
    FROM QueueMessages WITH (UPDLOCK, READPAST)
    WHERE Id IN (
        SELECT Id
            FROM QueueMessages WITH (INDEX(IX_QueueMessages_Retrieve))
            WHERE Environment = @environment
            AND TimestampUtc <= @now
            AND
                (
                    RetrievedOnUtc IS NULL
                    OR DATEADD(SECOND, VisibilityTimeout, RetrievedOnUtc) <= @now
                )    
        )
    ORDER BY Flags & 3 DESC, Id ASC'
END
