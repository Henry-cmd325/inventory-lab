using Inventory.Api.Features.Stock.IssueStock;
using Inventory.Api.Features.Stock.ReceiveStock;
using Inventory.Api.Infrastructure;
using Npgsql;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

var connectionString = builder.Configuration.GetConnectionString("Inventory")
    ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:Inventory'.");
builder.Services.AddSingleton(_ => new NpgsqlSlimDataSourceBuilder(connectionString).Build());

// One line per slice: stores.
builder.Services.AddSingleton<IReceiveStockStore, ReceiveStockStore>();
builder.Services.AddSingleton<IIssueStockStore, IssueStockStore>();

var app = builder.Build();

app.UseExceptionHandler();

// One route group per area, one line per slice.
var stock = app.MapGroup("/stock").WithTags("Stock");
ReceiveStock.Map(stock);
IssueStock.Map(stock);

app.MapGet("/health", () => TypedResults.Ok("ok"));

app.Run();

public partial class Program;
