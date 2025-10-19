using Chat;

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
app.MapGrpcService<ChatHubService>();

// Simple home page to test connectivity
app.MapGet("/", () => "Serveur Chat gRPC - Hub de chat partagé !");

// Endpoint to get server information
app.MapGet("/info", () => new
{
    ServerName = "GrpcChat Server",
    Version = "1.0.0",
    StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    Status = "Running",
    ConnectedClients = ChatHubService.GetConnectedClientsCount(),
    Features = new[] { "Chat Hub", "Multi-Client Support", "Real-time Messaging" }
});

app.Run("https://localhost:5003");
