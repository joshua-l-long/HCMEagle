using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SchemaProcessor;

public static class XmlParser
{
    public static IEnumerable<ColumnDefinition> Parse(string filePath)
    {
        var doc = XDocument.Load(filePath);
        var columnDefinitions = new List<ColumnDefinition>();

        foreach (var tableElement in doc.Descendants("table"))
        {
            var schema = tableElement.Element("schema")?.Value ?? string.Empty;
            var tableName = tableElement.Element("name")?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(tableName))
            {
                continue;
            }

            foreach (var columnElement in tableElement.Descendants("column"))
            {
                columnDefinitions.Add(new ColumnDefinition
                {
                    Schema = schema,
                    Table = tableName,
                    ColumnName = columnElement.Element("columnname")?.Value ?? string.Empty,
                    DataType = columnElement.Element("datatype")?.Value ?? string.Empty,
                    Precision = int.TryParse(columnElement.Element("precision")?.Value, out var p) ? p : null,
                    Nullable = (columnElement.Element("nullable")?.Value ?? "no").Equals("yes", System.StringComparison.OrdinalIgnoreCase),
                    PrimaryKey = (columnElement.Element("primarykey")?.Value ?? "no").Equals("yes", System.StringComparison.OrdinalIgnoreCase),
                    ForeignKey = (columnElement.Element("foreignkey")?.Value ?? "no").Equals("yes", System.StringComparison.OrdinalIgnoreCase),
                    References = columnElement.Element("references")?.Value,
                    OrdinalPosition = int.TryParse(columnElement.Element("ordinalposition")?.Value, out var o) ? o : 0,
                    DefaultValue = columnElement.Element("defaultvalue")?.Value,
                    Notes = columnElement.Element("notes")?.Value
                });
            }
        }

        return columnDefinitions;
    }
}