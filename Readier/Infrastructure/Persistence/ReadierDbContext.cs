using Microsoft.EntityFrameworkCore;

namespace Readier.Infrastructure.Persistence;

public class ReadierDbContext : DbContext
{
    public DbSet<UserStorageRecord> UserStorageRecords => Set<UserStorageRecord>();
    public DbSet<PlanRecord> Plans => Set<PlanRecord>();

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

        modelBuilder.Entity<PlanRecord>(entity =>
        {
            entity.ToTable("Plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).IsRequired().HasMaxLength(256);
            entity.Property(x => x.PlanId).IsRequired();
            entity.Property(x => x.Title).IsRequired().HasMaxLength(120);
            entity.Property(x => x.OriginJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.DestinationJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.StartTime).IsRequired();
            entity.Property(x => x.EstimatedTravelMinutes).IsRequired();
            entity.Property(x => x.EstimatedPrepMinutes).IsRequired();
            entity.Property(x => x.Transportation).HasConversion<int>().IsRequired();
            entity.Property(x => x.UpdatedUtc).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.StartTime });
            entity.HasIndex(x => new { x.UserId, x.PlanId }).IsUnique();
        });
    }
}
