using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace SchemaProcessor;

public static class CsvParser
{
    public static IEnumerable<ColumnDefinition> Parse(string filePath)
    {
        using var reader = new StreamReader(filePath);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.Trim(),
            HeaderValidated = null,
            MissingFieldFound = null
        };
        using var csv = new CsvReader(reader, config);
        
        csv.Context.RegisterClassMap<ColumnDefinitionMap>();

        return csv.GetRecords<ColumnDefinition>().ToList();
    }
}

public class ColumnDefinitionMap : ClassMap<ColumnDefinition>
{
    public ColumnDefinitionMap()
    {
        Map(m => m.Schema).Name("schema");
        Map(m => m.Table).Name("table");
        Map(m => m.ColumnName).Name("columnname");
        Map(m => m.DataType).Name("datatype");
        Map(m => m.Precision).Name("precision").TypeConverter<NullableIntConverter>();
        Map(m => m.Nullable).Name("nullable").TypeConverter<BooleanConverter>();
        Map(m => m.PrimaryKey).Name("primarykey").TypeConverter<BooleanConverter>();
        Map(m => m.ForeignKey).Name("foreignkey").TypeConverter<BooleanConverter>();
        Map(m => m.References).Name("references");
        Map(m => m.OrdinalPosition).Name("ordinalposition").TypeConverter<NullableIntConverter>();
        Map(m => m.DefaultValue).Name("defaultvalue");
        Map(m => m.Notes).Name("notes");
    }
}
