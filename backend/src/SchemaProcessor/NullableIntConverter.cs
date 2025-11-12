using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace SchemaProcessor;

public class NullableIntConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text))
        {
            return default(int?); // Explicitly return null for a nullable int
        }

        if (int.TryParse(text, out var i))
        {
            return i;
        }
        
        // Fallback to the base converter if parsing fails, which might throw an exception
        return base.ConvertFromString(text, row, memberMapData); 
    }
}
