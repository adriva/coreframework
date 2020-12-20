using Microsoft.EntityFrameworkCore;

namespace Adriva.Storage.SqlServer
{
    internal class QueueDbContext : DbContext
    {
        public DbSet<QueueMessageEntity> Messages { get; set; }

        public QueueDbContext(DbContextOptions<QueueDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QueueMessageEntity>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.VisibilityTimeout).HasDefaultValue(60);
                entity.Property(x => x.Content).HasColumnType("varbinary(max)");
            });
        }
    }
}
