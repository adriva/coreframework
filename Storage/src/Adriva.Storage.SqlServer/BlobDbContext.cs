using Microsoft.EntityFrameworkCore;

namespace Adriva.Storage.SqlServer
{
    internal sealed class BlobDbContext : DbContext
    {
        private readonly SqlServerBlobOptions Options;
        public DbSet<BlobItemEntity> Blobs { get; set; }

        public BlobDbContext(SqlServerBlobOptions blobOptions, DbContextOptions<BlobDbContext> contextOptions) : base(contextOptions)
        {
            this.Options = blobOptions;
            this.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(this.Options.SchemaName);

            modelBuilder.Entity<BlobItemEntity>(entity =>
            {
                entity.ToTable(this.Options.TableName).HasKey(x => x.Id);
                entity.HasIndex(e => new { e.ContainerName, e.Name }).IsUnique().HasName($"IUX_{this.Options.TableName}_Name");
                entity.Property(e => e.LastModifiedUtc).HasDefaultValueSql("GETUTCDATE()");
            });

        }
    }
}
