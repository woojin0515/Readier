namespace Readier.Infrastructure.Persistence;

public class UserStorageRecord
{
    public long Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string JsonValue { get; set; } = string.Empty;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
