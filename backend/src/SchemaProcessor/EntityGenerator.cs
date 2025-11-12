using System.Text;

namespace SchemaProcessor;

public static class EntityGenerator
{
    public static string Generate(string tableName, string uniqueEntityName, IEnumerable<ColumnDefinition> columnDefinitions)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.ComponentModel.DataAnnotations;");
        sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
        sb.AppendLine();
        sb.AppendLine("namespace SchemaProcessor.Entities;");
        sb.AppendLine();

        // Get the schema from the first column definition.
        var schema = columnDefinitions.FirstOrDefault()?.Schema;

        // Generate the Table attribute, including the schema if it exists.
        sb.AppendLine(string.IsNullOrEmpty(schema)
            ? $"[Table(\"{tableName}\")]"
            : $"[Table(\"{tableName}\", Schema = \"{schema}\")]");
        sb.AppendLine($"public class {uniqueEntityName}");
        sb.AppendLine("{");

        foreach (var column in columnDefinitions.OrderBy(c => c.OrdinalPosition))
        {
            if (column.PrimaryKey)
            {
                sb.AppendLine("    [Key]");
            }
            var (type, isRefType) = GetClrType(column.DataType, column.Nullable);
            sb.Append($"    public {type} {column.ColumnName} {{ get; set; }}");
            if (isRefType && !column.Nullable)
            {
                sb.Append(" = string.Empty;");
            }
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static (string type, bool isRefType) GetClrType(string dbType, bool nullable)
    {
        (string type, bool isRefType) = dbType.ToLower() switch
        {
            "int" => ("int", false),
            "string" => ("string", true),
            "varchar" => ("string", true),
            "timestamp" => ("DateTime", false),
            "datetime" => ("DateTime", false),
            "timestamptz" => ("DateTime", false),
            _ => ("object", true)
        };

        if (nullable)
        {
            // For both value types (like int) and reference types (like string),
            // append '?' to mark them as nullable in C#.
            return ($"{type}?", isRefType);
        }

        return (type, isRefType);
    }
}