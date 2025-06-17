CREATE TABLE CacheDependency (
	[Key] [nvarchar](900) NOT NULL,
	[CacheKey] [nvarchar](900) NOT NULL,

    INDEX IX_CacheDependency NONCLUSTERED ([Key])
) WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_ONLY)

CREATE TABLE [Cache](
	[Id] [nvarchar](900) NOT NULL PRIMARY KEY NONCLUSTERED,
	[Value] [varbinary](max) NOT NULL,
	[ExpiresAtTime] [datetime2](7) NOT NULL INDEX IX_Cache_Expiry NONCLUSTERED,
	[SlidingExpirationInSeconds] [bigint] NULL,
	[AbsoluteExpiration] [datetime2](7) NULL
) WITH (MEMORY_OPTIMIZED = ON, DURABILITY = SCHEMA_ONLY)