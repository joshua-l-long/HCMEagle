using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SchemaProcessor;

public static class MarkdownParser
{
    public static IEnumerable<ColumnDefinition> Parse(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var columnDefinitions = new List<ColumnDefinition>();

        // Skip header and separator
        foreach (var line in lines.Skip(2))
        {
            var parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            if (parts.Length < 12) continue;

            columnDefinitions.Add(new ColumnDefinition
            {
                Schema = parts[0],
                Table = parts[1],
                ColumnName = parts[2],
                DataType = parts[3],
                Precision = int.TryParse(parts[4], out var p) ? p : null,
                Nullable = parts[5].Equals("yes", StringComparison.OrdinalIgnoreCase),
                PrimaryKey = parts[6].Equals("yes", StringComparison.OrdinalIgnoreCase),
                ForeignKey = parts[7].Equals("yes", StringComparison.OrdinalIgnoreCase),
                References = parts[8],
                OrdinalPosition = int.TryParse(parts[9], out var o) ? o : 0,
                DefaultValue = parts[10],
                Notes = parts[11]
            });
        }

        return columnDefinitions;
    }
}
