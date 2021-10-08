using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.SqlServer
{
    internal class QueueDbContext : DbContext
    {
        private readonly SqlServerQueueOptions Options;

        public DbSet<QueueMessageEntity> Messages { get; set; }

        public QueueDbContext(IOptions<SqlServerQueueOptions> optionsAccessor, DbContextOptions<QueueDbContext> options) : base(options)
        {
            this.Options = optionsAccessor.Value;
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
