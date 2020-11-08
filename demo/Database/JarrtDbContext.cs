using Microsoft.EntityFrameworkCore;

namespace demo.Database
{
    public class Promotion
    {
        public long Id { get; set; }

        public string Supplier { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class JarrtDbContext : DbContext
    {
        public DbSet<Promotion> Promotions { get; set; }

        public JarrtDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("Promotion");
                entity.HasKey(x => x.Id);
            });
        }
    }
}