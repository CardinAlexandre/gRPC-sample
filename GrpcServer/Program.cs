using Demo;
using GrpcDemo;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Configuration du serveur gRPC
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});

// Configuration du logging pour voir les flux
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Configure pipeline
app.MapGrpcService<GrpcDemo.GrpcDemoServiceImpl>();

// Simple home page to test connectivity
app.MapGet("/", () => "Serveur gRPC Demo - Prêt à recevoir des appels !");

// Endpoint to get server information
app.MapGet("/info", () => new
{
    ServerName = "GrpcDemo Server",
    Version = "1.0.0",
    StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    Status = "Running",
    SupportedFeatures = new[] { "Unary", "Server Streaming", "Client Streaming", "Bidirectional Streaming" }
});

app.Run("https://localhost:5001");
