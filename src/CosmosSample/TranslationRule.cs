namespace CosmosSample;

public sealed class TranslationRule
{
    public Guid id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public string SourceSystem { get; set; } = string.Empty;

    public string SourceAttributeName { get; set; } = string.Empty;

    public string SourceAttributeValue { get; set; } = string.Empty;

    public string TargetSystem { get; set; } = string.Empty;

    public string TargetAttributeName { get; set; } = string.Empty;

    public string TargetAttributeValue { get; set; } = string.Empty;

    public int Priority { get; set; }

    public bool Sum { get; set; }

    public bool Concat { get; set; }

    public bool IsMapped { get; set; }

    public string? PartitionKey { get; set; }
}