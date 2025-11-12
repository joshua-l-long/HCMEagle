using System.Collections.Generic;

namespace SchemaProcessor;

public class JsonRoot
{
    public List<JsonTable> Tables { get; set; } = new();
}

public class JsonTable
{
    public string Schema { get; set; } = string.Empty;
    public string Table { get; set; } = string.Empty;
    public List<JsonColumn> Columns { get; set; } = new();
}

public class JsonColumn
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Precision { get; set; } = string.Empty;
    public string Nullable { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = string.Empty;
    public string ForeignKey { get; set; } = string.Empty;
    public string References { get; set; } = string.Empty;
    public string OrdinalPosition { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}