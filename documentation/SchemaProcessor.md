# Schema Processor Documentation

This document explains how to use the schema processor to automatically generate database migrations from schema definition files.

## File Location

To have your schema files processed, place them in the following directory:

```
/workspaces/Eagle/schemas/incoming
```

The processor will automatically pick up any new files in this directory the next time it runs.

## File Nomenclature

While not strictly enforced, we recommend the following naming convention for your schema files:

`<schema>.<tablename>.<format>`

For example:

*   `hrreporting.employees.csv`
*   `operations.users.json`

This convention helps to keep the schema files organized and easily identifiable.

## Supported File Formats

The schema processor currently supports the following file formats:

*   CSV (`.csv`)
*   Markdown (`.md`)
*   JSON (`.json`)
*   XLSX (`.xlsx`)
*   XML (`.xml`)


## Running the Processor

There are two ways to run the schema processor:

### 1. Manual Execution

You can run the processor manually from the command line. This is useful for development and testing.  

#### Command-Line Arguments

The schema processor accepts the following command-line arguments:

*   `--process-schemas`: This is the default behavior. It processes schema definition files from the `schemas/incoming` directory.
*   `--seed-data <file_path>`: This seeds data from a specific file.

##### Examples

To process schema files:

```bash
# From the project root directory
dotnet run --project SchemaProcessor -- --process-schemas
```

To seed data from a file:

```bash
# From the project root directory
dotnet run --project SchemaProcessor -- --seed-data /workspaces/Eagle/schemas/definitions/hrreporting.users.csv
```

### 2. Docker Build

The schema processor is designed to run automatically when you build the Docker image for this project. This is the recommended approach for production environments.

To build the Docker image, run the following command from the root of the project (`/workspaces/Eagle`):

```bash
# From the project root directory
docker build -t schema-processor .
```

During the Docker build process, the schema processor will execute and process any files found in the `schemas/incoming` directory. The generated migrations will be included in the final Docker image.
