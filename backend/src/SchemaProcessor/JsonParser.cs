using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SchemaProcessor;

public static class JsonParser
{
    public static IEnumerable<ColumnDefinition> Parse(string filePath)
    {
        var jsonString = File.ReadAllText(filePath);
        var jsonRoot = JsonSerializer.Deserialize<JsonRoot>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (jsonRoot == null)
        {
            return Enumerable.Empty<ColumnDefinition>();
        }

        var columnDefinitions = new List<ColumnDefinition>();

        foreach (var table in jsonRoot.Tables)
        {
            foreach (var column in table.Columns)
            {
                columnDefinitions.Add(new ColumnDefinition
                {
                    Schema = table.Schema,
                    Table = table.Table,
                    ColumnName = column.ColumnName,
                    DataType = column.DataType,
                    Precision = int.TryParse(column.Precision, out var p) ? p : null,
                    Nullable = column.Nullable.Equals("yes", System.StringComparison.OrdinalIgnoreCase),
                    PrimaryKey = column.PrimaryKey.Equals("yes", System.StringComparison.OrdinalIgnoreCase),
                    ForeignKey = column.ForeignKey.Equals("yes", System.StringComparison.OrdinalIgnoreCase),
                    References = column.References,
                    OrdinalPosition = int.TryParse(column.OrdinalPosition, out var o) ? o : 0,
                    DefaultValue = column.DefaultValue,
                    Notes = column.Notes
                });
            }
        }

        return columnDefinitions;
    }
}