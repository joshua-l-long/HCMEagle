using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;

namespace SchemaProcessor;

public static class SeedDataParser
{
    public static Dictionary<string, List<dynamic>> Parse(string filePath, string tableName)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".csv" => ParseCsv(filePath, tableName),
            ".json" => ParseJson(filePath),
            ".xml" => ParseXml(filePath),
            ".xlsx" => ParseExcel(filePath),
            ".md" => ParseMarkdown(filePath),
            _ => throw new NotSupportedException($"File type not supported: {filePath}")
        };
    }

    private static Dictionary<string, List<dynamic>> ParseCsv(string filePath, string tableName)
    {
        using var reader = new StreamReader(filePath);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
        };
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<dynamic>().ToList();
        var parts = tableName.Split('.');
        var schema = parts[0];
        var table = parts[1];

        foreach (var record in records)
        {
            var dict = (IDictionary<string, object>)record;
            dict["schema"] = schema;
            dict["table"] = table;
        }

        return GroupRecords(records);
    }

    private static Dictionary<string, List<dynamic>> ParseJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<List<JsonSeedData>>(json);
        var records = new List<dynamic>();

        if (data == null) return new Dictionary<string, List<dynamic>>();

        foreach (var entry in data)
        {
            foreach (var row in entry.Rows)
            {
                var record = new ExpandoObject() as IDictionary<string, object>;
                record["schema"] = entry.Schema;
                record["table"] = entry.Table;
                foreach (var prop in row.EnumerateObject())
                {
                    record[prop.Name] = prop.Value.ToString();
                }
                records.Add(record);
            }
        }
        return GroupRecords(records);
    }

    private static Dictionary<string, List<dynamic>> ParseXml(string filePath)
    {
        var doc = XDocument.Load(filePath);
        var records = new List<dynamic>();

        foreach (var dataElement in doc.Descendants("data"))
        {
            var schema = dataElement.Attribute("schema")?.Value;
            var table = dataElement.Attribute("table")?.Value;

            if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(table)) continue;

            foreach (var rowElement in dataElement.Descendants("row"))
            {
                var record = new ExpandoObject() as IDictionary<string, object>;
                record["schema"] = schema;
                record["table"] = table;
                foreach (var attribute in rowElement.Attributes())
                {
                    record[attribute.Name.ToString()] = attribute.Value;
                }
                records.Add(record);
            }
        }
        return GroupRecords(records);
    }

    private static Dictionary<string, List<dynamic>> ParseExcel(string filePath)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = true
            }
        });

        var records = new List<dynamic>();
        foreach (DataTable table in result.Tables)
        {
            foreach (DataRow row in table.Rows)
            {
                var record = new ExpandoObject() as IDictionary<string, object>;
                foreach (DataColumn col in table.Columns)
                {
                    record[col.ColumnName] = row[col]?.ToString() ?? string.Empty;
                }
                records.Add(record);
            }
        }
        return GroupRecords(records);
    }

    private static Dictionary<string, List<dynamic>> ParseMarkdown(string filePath)
    {
        // For Markdown, we'll treat it like CSV but need to parse the table format.
        // This is a simplified parser assuming a standard GitHub-flavored Markdown table.
        var lines = File.ReadAllLines(filePath).Select(l => l.Trim()).Where(l => l.StartsWith("|")).ToList();
        if (lines.Count < 2) return new Dictionary<string, List<dynamic>>();

        var header = lines[0].Split('|', StringSplitOptions.RemoveEmptyEntries).Select(h => h.Trim()).ToArray();
        var records = new List<dynamic>();

        for (int i = 2; i < lines.Count; i++)
        {
            var values = lines[i].Split('|', StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToArray();
            var record = new ExpandoObject() as IDictionary<string, object>;
            for (int j = 0; j < header.Length; j++)
            {
                record[header[j]] = values.Length > j ? values[j] : string.Empty;
            }
            records.Add(record);
        }
        return GroupRecords(records);
    }

    private static Dictionary<string, List<dynamic>> GroupRecords(List<dynamic> records)
    {
        var groupedData = new Dictionary<string, List<dynamic>>();
        foreach (var record in records)
        {
            var recordDict = (IDictionary<string, object>)record;
            if (recordDict.TryGetValue("schema", out var schemaObj) && recordDict.TryGetValue("table", out var tableObj))
            {
                var schema = schemaObj.ToString();
                var table = tableObj.ToString();
                var tableName = $"{schema}.{table}";

                if (!groupedData.ContainsKey(tableName))
                {
                    groupedData[tableName] = new List<dynamic>();
                }
                groupedData[tableName].Add(record);
            }
        }
        return groupedData;
    }
}

file class JsonSeedData
{
    public string Schema { get; set; } = string.Empty;
    public string Table { get; set; } = string.Empty;
    public List<JsonElement> Rows { get; set; } = new();
}
