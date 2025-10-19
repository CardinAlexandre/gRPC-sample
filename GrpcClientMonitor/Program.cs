using Grpc.Net.Client;
using Demo;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcClientMonitor;

class Program
{
    private static GrpcDemoService.GrpcDemoServiceClient? _client;
    private static readonly string ServerUrl = "https://localhost:5001";
    private static readonly string MonitorId = $"Monitor-{Random.Shared.Next(100, 999)}";

    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"📊 Client Monitor gRPC - {MonitorId}");
        Console.WriteLine("==============================================");
        Console.ResetColor();

        try
        {
            // Configure gRPC client
            using var channel = GrpcChannel.ForAddress(ServerUrl, new GrpcChannelOptions
            {
                HttpHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = 
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }
            });

            _client = new GrpcDemoService.GrpcDemoServiceClient(channel);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ {MonitorId} connected to server: {ServerUrl}");
            Console.ResetColor();

            // Start monitoring
            await StartMonitoring();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Connection error: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static async Task StartMonitoring()
    {
        Console.WriteLine($"\n📡 Starting real-time monitoring for {MonitorId}...");
        Console.WriteLine("The monitor is listening for server notifications...\n");

        try
        {
            // Get server information first
            var serverInfo = await _client!.GetServerInfoAsync(new Empty());
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🖥️  SERVER INFORMATION:");
            Console.WriteLine($"   Name: {serverInfo.ServerName}");
            Console.WriteLine($"   Version: {serverInfo.Version}");
            Console.WriteLine($"   Startup: {serverInfo.StartTime}");
            Console.WriteLine($"   Active connections: {serverInfo.ActiveConnections}");
            Console.WriteLine($"   Features: {string.Join(", ", serverInfo.SupportedFeatures)}");
            Console.ResetColor();
            Console.WriteLine();

            // Start monitoring streaming
            var request = new StreamingRequest
            {
                Message = $"Monitoring started by {MonitorId}",
                SequenceNumber = 1,
                ClientId = MonitorId
            };

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"🔄 Starting Server Streaming for monitoring...");
            Console.ResetColor();

            using var call = _client.ServerStreaming(request);
            var messageCount = 0;
            
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                messageCount++;
                
                // Colorer les logs selon le type de message
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                
                switch (response.StreamType)
                {
                    case "server_stream":
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"📡 [{timestamp}] NOTIFICATION #{messageCount}: {response.Response}");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"📨 [{timestamp}] MESSAGE #{messageCount}: {response.Response}");
                        break;
                }
                
                Console.ResetColor();
                
                // Display periodic statistics
                if (messageCount % 3 == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"   📊 Statistics: {messageCount} messages received, Sequence: {response.SequenceNumber}");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n📊 Monitoring completed. Total: {messageCount} messages received");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Erreur dans le monitoring: {ex.Message}");
            Console.ResetColor();
        }
    }
}
