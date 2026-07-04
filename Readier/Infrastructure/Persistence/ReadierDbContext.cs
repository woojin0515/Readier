using Microsoft.EntityFrameworkCore;

namespace Readier.Infrastructure.Persistence;

public class ReadierDbContext : DbContext
{
    public DbSet<UserStorageRecord> UserStorageRecords => Set<UserStorageRecord>();

    public ReadierDbContext(DbContextOptions<ReadierDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserStorageRecord>(entity =>
        {
            entity.ToTable("UserStorageRecords");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).IsRequired().HasMaxLength(256);
            entity.Property(x => x.StorageKey).IsRequired().HasMaxLength(200);
            entity.Property(x => x.JsonValue).IsRequired();
            entity.Property(x => x.UpdatedUtc).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.StorageKey }).IsUnique();
        });
    }
}
