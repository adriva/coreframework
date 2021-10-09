using Microsoft.EntityFrameworkCore;

namespace Adriva.Storage.SqlServer
{
    internal sealed class QueueDbContext : DbContext
    {
        private readonly SqlServerQueueOptions Options;

        public DbSet<QueueMessageEntity> Messages { get; set; }

        public QueueDbContext(SqlServerQueueOptions queueOptions, DbContextOptions<QueueDbContext> contextOptions) : base(contextOptions)
        {
            this.Options = queueOptions;
            this.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(this.Options.SchemaName);

            modelBuilder.Entity<QueueMessageEntity>(entity =>
            {
                entity.ToTable(this.Options.TableName);
                entity.HasKey(x => x.Id);
                entity.Property(x => x.VisibilityTimeout).HasDefaultValue(60 * 60 * 24);
                entity.Property(x => x.TimeToLive).HasDefaultValue(30);
                entity.Property(x => x.Content);
            });
        }
    }
}
