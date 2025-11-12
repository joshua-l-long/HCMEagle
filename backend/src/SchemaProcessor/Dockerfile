FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

# Copy all source code
COPY . .

# Run the schema processor to generate entities and migrations
RUN dotnet run --project SchemaProcessor

# Build the final application
RUN dotnet build SchemaProcessor/SchemaProcessor.csproj -c Release

# The container will have the processed files and the generated migration.
# The default command can be set to run the application or a shell.
CMD ["/bin/bash"]
