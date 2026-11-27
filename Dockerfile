# Build stage: SDK with the Native AOT toolchain (clang, linker).
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble-aot AS build
WORKDIR /src

# Restore first, so package downloads are cached while code changes.
COPY global.json Directory.Build.props ./
COPY src/Inventory.Domain/Inventory.Domain.csproj src/Inventory.Domain/
COPY src/Inventory.Api/Inventory.Api.csproj src/Inventory.Api/
RUN dotnet restore src/Inventory.Api/Inventory.Api.csproj -r linux-x64

COPY src/ src/
RUN dotnet publish src/Inventory.Api/Inventory.Api.csproj -c Release -r linux-x64 --no-restore -o /app

# Final stage: no SDK, no shell, non-root.
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app/Inventory.Api .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["./Inventory.Api"]
