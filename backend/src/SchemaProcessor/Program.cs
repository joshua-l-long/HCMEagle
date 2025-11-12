using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SchemaProcessor;

ProcessIncomingDirectory();

static void ProcessIncomingDirectory()
{
    var incomingDir = Path.Combine(Directory.GetCurrentDirectory(), "schemas", "incoming");
    if (!Directory.Exists(incomingDir))
    {
        Directory.CreateDirectory(incomingDir);
        Console.WriteLine("Incoming directory created. No files to process.");
        return;
    }

    var files = Directory.GetFiles(incomingDir, "*.*")
        .Where(f => f.EndsWith(".csv") || f.EndsWith(".md") || f.EndsWith(".json") || f.EndsWith(".xml") || f.EndsWith(".xlsx") || f.EndsWith(".sql")).ToArray();

    if (files.Length == 0)
    {
        Console.WriteLine("No new files to process.");
        return;
    }

    var schemaFiles = files.Where(f => Path.GetFileName(f).StartsWith("schema_")).ToList();
    var seedFiles = files.Where(f => Path.GetFileName(f).StartsWith("seed_")).ToList();

    if (schemaFiles.Any())
    {
        ProcessSchemaFiles(schemaFiles);
    }

    if (seedFiles.Any())
    {
        foreach (var file in seedFiles)
        {
            SeedDataFile(file);
        }
    }
}

static void ProcessSchemaFiles(List<string> schemaFiles)
{
    var entitiesDir = Path.Combine(Directory.GetCurrentDirectory(), "SchemaProcessor", "Entities");
    var processedDir = Path.Combine(Directory.GetCurrentDirectory(), "schemas", "processed");
    var projectPath = Path.Combine(Directory.GetCurrentDirectory(), "SchemaProcessor", "SchemaProcessor.csproj");

    if (Directory.Exists(entitiesDir))
    {
        Directory.Delete(entitiesDir, true);
        Console.WriteLine("Cleaned up old entity files.");
    }
    Directory.CreateDirectory(entitiesDir);
    Directory.CreateDirectory(processedDir);

    foreach (var file in schemaFiles)
    {
        Console.WriteLine($"Processing schema file {file}...");

        IEnumerable<ColumnDefinition> columnDefinitions = Enumerable.Empty<ColumnDefinition>();

        try
        {
            switch (Path.GetExtension(file).ToLower())
            {
                case ".csv":
                    columnDefinitions = CsvParser.Parse(file);
                    break;
                case ".md":
                    columnDefinitions = MarkdownParser.Parse(file);
                    break;
                case ".json":
                    columnDefinitions = JsonParser.Parse(file);
                    break;
                case ".xml":
                    columnDefinitions = XmlParser.Parse(file);
                    break;
                case ".xlsx":
                    columnDefinitions = ExcelParser.Parse(file);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file {file}: {ex.Message}");
            continue;
        }

        if (!columnDefinitions.Any())
        {
            Console.WriteLine($"  -> Skipping file with unsupported extension or empty content: {file}");
        }
        else
        {
            var tables = columnDefinitions.GroupBy(c => c.Table);

            foreach (var table in tables)
            {
                var tableName = table.Key;
                var schemaName = table.FirstOrDefault()?.Schema ?? string.Empty;
                var uniqueFileName = string.IsNullOrEmpty(schemaName) ? tableName : $"{schemaName}_{tableName}";
                var uniqueClassName = (string.IsNullOrEmpty(schemaName) ? "" : char.ToUpper(schemaName[0]) + schemaName.Substring(1) + "_") + char.ToUpper(tableName[0]) + tableName.Substring(1);

                var entitySource = EntityGenerator.Generate(tableName, uniqueClassName, table.ToList());
                var entityFilePath = Path.Combine(entitiesDir, $"{uniqueFileName}.cs");

                File.WriteAllText(entityFilePath, entitySource);
                Console.WriteLine($"  -> Generated {entityFilePath}");
            }
        }

        var newFileName = $"{Path.GetFileNameWithoutExtension(file)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file)}";
        var newFilePath = Path.Combine(processedDir, newFileName);
        File.Move(file, newFilePath);

        Console.WriteLine($"  -> Moved to {newFilePath}");
    }

    Console.WriteLine("Building the project...");
    var buildProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build {projectPath}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        }
    };

    buildProcess.Start();
    string buildOutput = buildProcess.StandardOutput.ReadToEnd();
    string buildError = buildProcess.StandardError.ReadToEnd();
    buildProcess.WaitForExit();

    Console.WriteLine(buildOutput);

    if (buildProcess.ExitCode != 0)
    {
        Console.WriteLine("Build failed:");
        Console.WriteLine(buildError);
        return;
    }

    Console.WriteLine("Build successful.");

    var migrationName = $"Migration_{DateTime.Now:yyyyMMddHHmmss}";
    Console.WriteLine($"Generating migration: {migrationName}...");

    var migrationProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"ef migrations add {migrationName} --project {projectPath} --startup-project {projectPath}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        }
    };

    migrationProcess.Start();
    string migrationOutput = migrationProcess.StandardOutput.ReadToEnd();
    string migrationError = migrationProcess.StandardError.ReadToEnd();
    migrationProcess.WaitForExit();

    Console.WriteLine(migrationOutput);

    if (migrationProcess.ExitCode != 0)
    {
        Console.WriteLine("Migration generation failed:");
        Console.WriteLine(migrationError);
        return;
    }

    Console.WriteLine("Migration generated successfully.");

    Console.WriteLine("Applying migration...");

    var updateProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"ef database update --project {projectPath} --startup-project {projectPath}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        }
    };

    updateProcess.Start();
    string updateOutput = updateProcess.StandardOutput.ReadToEnd();
    string updateError = updateProcess.StandardError.ReadToEnd();
    updateProcess.WaitForExit();

    Console.WriteLine(updateOutput);

    if (updateProcess.ExitCode != 0)
    {
        Console.WriteLine("Applying migration failed:");
        Console.WriteLine(updateError);
        return;
    }

    Console.WriteLine("Migration applied successfully.");
}

static void SeedDataFile(string filePath)
{
    DotNetEnv.Env.Load();
    Console.WriteLine($"Seeding data from {filePath}...");

    var fileName = Path.GetFileNameWithoutExtension(filePath);
    var nameParts = fileName.Split('_');
    var tableName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

    Dictionary<string, List<dynamic>> seedData;
    try
    {
        seedData = SeedDataParser.Parse(filePath, tableName);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing seed file: {ex.Message}");
        return;
    }

    var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    var connectionString = configuration["EAGLE_CONNECTION_STRING"];
    Console.WriteLine($"Connection String: {connectionString}");

    var optionsBuilder = new DbContextOptionsBuilder<SchemaDbContext>();
    optionsBuilder.UseNpgsql(connectionString);

    using (var context = new SchemaDbContext(optionsBuilder.Options))
    {
        foreach (var (fullTableName, data) in seedData)
        {
            var parts = fullTableName.Split('.');
            var schemaName = parts[0];
            var justTableName = parts[1];
            var uniqueClassName = (string.IsNullOrEmpty(schemaName) ? "" : char.ToUpper(schemaName[0]) + schemaName.Substring(1) + "_") + char.ToUpper(justTableName[0]) + justTableName.Substring(1);

            var entityTypeName = $"SchemaProcessor.Entities.{uniqueClassName}";
            var entityType = typeof(SchemaDbContext).Assembly.GetType(entityTypeName);

            if (entityType == null)
            {
                Console.WriteLine($"Could not find entity type for {uniqueClassName}");
                continue;
            }

            var primaryKeyName = context.Model.FindEntityType(entityType)?.FindPrimaryKey()?.Properties.Select(p => p.Name).FirstOrDefault();

            foreach (var row in data)
            {
                var entity = Activator.CreateInstance(entityType);
                if (entity == null) continue;

                foreach (var property in (IDictionary<string, object>)row)
                {
                    if (primaryKeyName != null && property.Key.Equals(primaryKeyName, StringComparison.OrdinalIgnoreCase) && (property.Value == null || property.Value.ToString() == ":nextid" || string.IsNullOrEmpty(property.Value.ToString())))
                    {
                        continue;
                    }

                    if (property.Key == "schema" || property.Key == "table") continue;

                    var propInfo = entityType.GetProperty(property.Key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (propInfo != null && propInfo.CanWrite)
                    {
                        try
                        {
                            var underlyingType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
                            object value;
                            if (property.Value == null || string.IsNullOrEmpty(property.Value.ToString()))
                            {
                                value = null;
                            }
                            else if (underlyingType == typeof(DateTime))
                            {
                                if (DateTime.TryParse(property.Value.ToString(), out DateTime dt))
                                {
                                    value = dt.ToUniversalTime();
                                }
                                else
                                {
                                    Console.WriteLine($"Could not parse '{property.Value}' as a DateTime for property {property.Key}.");
                                    continue;
                                }
                            }
                            else
                            {
                                value = Convert.ChangeType(property.Value, underlyingType);
                            }
                            propInfo.SetValue(entity, value, null);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Could not set property {property.Key} on {uniqueClassName}: {ex.Message}");
                        }
                    }
                }
                context.Add(entity);
            }
        }
        try
        {
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving changes to database: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }

    var processedDir = Path.Combine(Directory.GetCurrentDirectory(), "schemas", "processed");
    Directory.CreateDirectory(processedDir);
    var newFileName = $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(filePath)}";
    var newFilePath = Path.Combine(processedDir, newFileName);
    File.Move(filePath, newFilePath);
    Console.WriteLine($"  -> Moved to {newFilePath}");

    Console.WriteLine("Data seeding complete.");
}