using System.Text.Json;
using System.Text.Json.Serialization;
using Inventory.Api.Features.Stock.IssueStock;
using Inventory.Api.Features.Stock.ReceiveStock;

namespace Inventory.Api.Infrastructure;

/// <summary>Source-generated JSON for every request and response type. Add one line per new type.</summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(ReceiveStockRequest))]
[JsonSerializable(typeof(ReceiveStockResponse))]
[JsonSerializable(typeof(IssueStockRequest))]
[JsonSerializable(typeof(IssueStockResponse))]
internal sealed partial class AppJsonContext : JsonSerializerContext;
