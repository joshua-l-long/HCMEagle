namespace SchemaProcessor;

public record ColumnDefinition
{
    public string Schema { get; set; } = string.Empty;
    public string Table { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? Precision { get; set; }
    public bool Nullable { get; set; }
    public bool PrimaryKey { get; set; }
    public bool ForeignKey { get; set; }
    public string? References { get; set; }
    public int? OrdinalPosition { get; set; }
    public string? DefaultValue { get; set; }
    public string? Notes { get; set; }
}