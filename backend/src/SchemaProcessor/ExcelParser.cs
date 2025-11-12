using ExcelDataReader;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SchemaProcessor;

public static class ExcelParser
{
    public static IEnumerable<ColumnDefinition> Parse(string filePath)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = true
            }
        });

        var dataTable = dataSet.Tables[0];
        var columnDefinitions = new List<ColumnDefinition>();

        foreach (System.Data.DataRow row in dataTable.Rows)
        {
            columnDefinitions.Add(new ColumnDefinition
            {
                Schema = row["schema"]?.ToString() ?? string.Empty,
                Table = row["table"]?.ToString() ?? string.Empty,
                ColumnName = row["columnname"]?.ToString() ?? string.Empty,
                DataType = row["datatype"]?.ToString() ?? string.Empty,
                Precision = int.TryParse(row["precision"]?.ToString(), out var p) ? p : null,
                Nullable = (row["nullable"]?.ToString() ?? "no").Equals("yes", System.StringComparison.OrdinalIgnoreCase),
                PrimaryKey = (row["primarykey"]?.ToString() ?? "no").Equals("yes", System.StringComparison.OrdinalIgnoreCase),
                ForeignKey = (row["foreignkey"]?.ToString() ?? "no").Equals("yes", System.StringComparison.OrdinalIgnoreCase),
                References = row["references"]?.ToString(),
                OrdinalPosition = int.TryParse(row["ordinalposition"]?.ToString(), out var o) ? o : 0,
                DefaultValue = row["defaultvalue"]?.ToString(),
                Notes = row["notes"]?.ToString()
            });
        }

        return columnDefinitions;
    }
}