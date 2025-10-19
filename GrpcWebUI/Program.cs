using Demo;
using GrpcDemo;

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

// Configure static files for web interface
app.UseStaticFiles();

// Home page with visualization interface
app.MapGet("/", () => Results.File("index.html", "text/html"));

// API to get server information
app.MapGet("/api/server-info", async () =>
{
    var serverInfo = new
    {
        ServerName = "GrpcDemo Server",
        Version = "1.0.0",
        StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        Status = "Running",
        SupportedFeatures = new[] { "Unary", "Server Streaming", "Client Streaming", "Bidirectional Streaming" }
    };
    
    return Results.Ok(serverInfo);
});

app.Run("https://localhost:5002");
