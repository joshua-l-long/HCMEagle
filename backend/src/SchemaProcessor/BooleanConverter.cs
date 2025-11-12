#pragma warning disable CS8603 // Possible null reference return.
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace SchemaProcessor;

public class BooleanConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        if (text.Equals("yes", StringComparison.OrdinalIgnoreCase) || text.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (text.Equals("no", StringComparison.OrdinalIgnoreCase) || text.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return base.ConvertFromString(text, row, memberMapData);
    }
}