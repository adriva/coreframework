IF OBJECT_ID('[dbo].[BlobItems]', 'U') IS NULL
BEGIN
    SET ANSI_NULLS ON

    SET QUOTED_IDENTIFIER ON

    CREATE TABLE [dbo].[BlobItems](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [ContainerName] [nvarchar](1024) NOT NULL,
        [Name] [nvarchar](1024) NOT NULL,
        [Content] [varbinary](max) NULL,
        [Length] [bigint] NOT NULL,
        [ETag] [varchar](100) NOT NULL,
        [LastModifiedUtc] [datetime2](7) NOT NULL
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    ALTER TABLE [dbo].[BlobItems] ADD PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

    SET ANSI_PADDING ON

    CREATE UNIQUE NONCLUSTERED INDEX [IUX_BlobItems_Name] ON [dbo].[BlobItems]
    (
        [ContainerName] ASC,
	    [Name] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

    ALTER TABLE [dbo].[BlobItems] ADD  DEFAULT ((0)) FOR [Length]

    ALTER TABLE [dbo].[BlobItems] ADD  DEFAULT (getutcdate()) FOR [LastModifiedUtc]
END