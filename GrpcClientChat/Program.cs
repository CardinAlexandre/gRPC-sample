using Grpc.Net.Client;
using Demo;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcClientChat;

class Program
{
    private static GrpcDemoService.GrpcDemoServiceClient? _client;
    private static readonly string ServerUrl = "https://localhost:5003";
    private static readonly string ClientName = $"ChatUser-{Random.Shared.Next(1000, 9999)}";
    private static readonly ConsoleColor[] Colors = { ConsoleColor.Cyan, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Magenta, ConsoleColor.Red, ConsoleColor.Blue };

    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"💬 gRPC Chat Client - {ClientName}");
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
            Console.WriteLine($"✅ {ClientName} connected to server: {ServerUrl}");
            Console.ResetColor();

            // Start bidirectional chat session
            await StartChatSession();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Connection error: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\nAppuyez sur une touche pour quitter...");
        Console.ReadKey();
    }

    static async Task StartChatSession()
    {
        Console.WriteLine($"\n🔄 Starting bidirectional chat session for {ClientName}...");
        Console.WriteLine("Type your messages (or 'exit' to quit):\n");

        try
        {
            using var call = _client!.BidirectionalStreaming();
            
            // Start receiving messages in background
            var receiveTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    var color = Colors[response.SequenceNumber % Colors.Length];
                    Console.ForegroundColor = color;
                    Console.WriteLine($"📨 [{response.Timestamp}] Server: {response.Response}");
                    Console.ResetColor();
                }
            });

            // Send messages
            int messageCount = 1;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{ClientName}> ");
                Console.ResetColor();
                
                var message = Console.ReadLine();
                
                if (string.IsNullOrEmpty(message) || message.ToLower() == "exit")
                    break;

                var request = new StreamingRequest
                {
                    Message = $"[{ClientName}] {message}",
                    SequenceNumber = messageCount++,
                    ClientId = ClientName
                };

                await call.RequestStream.WriteAsync(request);
                
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"✅ Message #{request.SequenceNumber} sent");
                Console.ResetColor();
            }

            await call.RequestStream.CompleteAsync();
            await receiveTask;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error in chat session: {ex.Message}");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n👋 {ClientName} left the chat");
        Console.ResetColor();
    }
}
