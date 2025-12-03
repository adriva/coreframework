using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.SqlServer;

public class SqlServerBlobStorage(DbContextOptions<SqlServerBlobStorage> options, IOptions<SqlServerBlobManagerConfiguration> optionsAccessor) : DbContext(options)
{
    private readonly SqlServerBlobManagerConfiguration Options = optionsAccessor.Value;

    public DbSet<SqlServerBlob> Blobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SqlServerBlob>(e =>
        {
            e.ToTable(this.Options.TableName, this.Options.SchemaName).HasKey(x => x.Id);

            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Container).HasConversion<string>();
        });
    }
}