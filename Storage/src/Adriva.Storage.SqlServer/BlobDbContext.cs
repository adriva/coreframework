using Microsoft.EntityFrameworkCore;

namespace Adriva.Storage.SqlServer
{
    internal class BlobDbContext : DbContext
    {
        public DbSet<BlobItemEntity> Blobs { get; set; }

        public BlobDbContext(DbContextOptions<BlobDbContext> options) : base(options)
        {
            this.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BlobItemEntity>(entity =>
            {
                entity.ToTable("BlobItems").HasKey(x => x.Id);
                entity.HasIndex(e => new { e.ContainerName, e.Name }).IsUnique().HasName("IUX_BlobItems_Name");
                entity.Property(e => e.LastModifiedUtc).HasDefaultValueSql("GETUTCDATE()");
            });

        }
    }
}
