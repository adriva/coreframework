IF OBJECT_ID('{SCHEMA}.{TABLE}', 'U') IS NULL
BEGIN
    SET ANSI_NULLS ON
    
    SET QUOTED_IDENTIFIER ON
    
    CREATE TABLE {SCHEMA}.{TABLE}(
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [Application] [nvarchar](50) NULL,
        [Environment] [nvarchar](50) NULL,
        [Content] [varchar](max) NULL,
        [Command] [nvarchar](100) NULL,
        [Flags] [bigint] NOT NULL,
        [VisibilityTimeout] [int] NOT NULL,
        [TimeToLive] [int] NOT NULL,
        [TimestampUtc] [datetime2](7) NOT NULL,
        [RetrievedOnUtc] [datetime2](7) NULL
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
    
    ALTER TABLE {SCHEMA}.{TABLE} ADD PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    
    SET ANSI_PADDING ON
    
    CREATE NONCLUSTERED INDEX [IX_{TABLE}_Retrieve] ON {SCHEMA}.{TABLE}
    (
        [Application] ASC,
        [Environment] DESC,
        [Flags] ASC
    )
    INCLUDE([VisibilityTimeout],[TimestampUtc],[RetrievedOnUtc],[TimeToLive]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    
    ALTER TABLE {SCHEMA}.{TABLE} ADD  DEFAULT ((0)) FOR [Flags]
    
    ALTER TABLE {SCHEMA}.{TABLE} ADD  DEFAULT ((60)) FOR [VisibilityTimeout]
    
    ALTER TABLE {SCHEMA}.{TABLE} ADD  DEFAULT ((86400)) FOR [TimeToLive]
    
END
