SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF OBJECT_ID('[$SchemaName$].[$TableName$]', 'U') IS NULL
BEGIN
	CREATE TABLE [$SchemaName$].[$TableName$](
		[Id] [bigint] IDENTITY(1,1) PRIMARY KEY,
		[RowId] [uniqueidentifier] ROWGUIDCOL NOT NULL,
		[Container] [varchar](255) NOT NULL DEFAULT('/'),
		[Name] [nvarchar](255) NOT NULL,
		[ETag] [varchar](100) NOT NULL,
		[MimeType] [varchar](100) NOT NULL DEFAULT('application/octet-stream'),
		[Size] [bigint] NOT NULL DEFAULT(-1),
		[ContentData] [varbinary](max) FILESTREAM  NULL,
		[Properties] [bigint] NOT NULL DEFAULT(0),
		[ModifiedOn] [datetimeoffset](3) NOT NULL DEFAULT(GETUTCDATE()),
	UNIQUE NONCLUSTERED 
	(
		[RowId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] FILESTREAM_ON [$FileGroupName$]

	CREATE UNIQUE NONCLUSTERED INDEX [IUX_$TableName$] ON [$SchemaName$].[$TableName$]([Container] ASC, [Name] ASC);
END