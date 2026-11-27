using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Inventory.Api.Tests;

/// <summary>
/// Runs the API in memory against the real PostgreSQL started by Docker Compose.
/// The connection string comes from ConnectionStrings__Inventory (set in compose.yaml).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:Inventory",
            Environment.GetEnvironmentVariable("ConnectionStrings__Inventory")
            ?? "Host=localhost;Database=inventory;Username=inventory;Password=inventory");

    /// <summary>A SKU no other test uses, so tests can share one database.</summary>
    public static string NewSku() => "T-" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
}
