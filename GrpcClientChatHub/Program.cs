using Grpc.Net.Client;
using Chat;
using Grpc.Core;

namespace GrpcClientChatHub;

class Program
{
    private static ChatService.ChatServiceClient? _client;
    private static readonly string ServerUrl = "https://localhost:5003";
    private static readonly string ClientName = $"ChatUser-{Random.Shared.Next(1000, 9999)}";
    private static readonly ConsoleColor[] Colors = { ConsoleColor.Cyan, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Magenta, ConsoleColor.Red, ConsoleColor.Blue };

    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"💬 Client Chat Hub gRPC - {ClientName}");
        Console.WriteLine("==============================================");
        Console.WriteLine("🌟 This client connects to the shared chat hub");
        Console.WriteLine("💡 Multiple clients can chat together!");
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

            _client = new ChatService.ChatServiceClient(channel);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ {ClientName} connected to server: {ServerUrl}");
            Console.ResetColor();

            // Start chat hub session
            await StartChatHubSession();
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

    static async Task StartChatHubSession()
    {
        Console.WriteLine($"\n🔄 Connecting to chat hub for {ClientName}...");
        Console.WriteLine("Type your messages (or 'exit' to quit):\n");

        try
        {
            using var call = _client!.Chat();
            
            // Start receiving messages in background
            var receiveTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    
                    // Color messages according to type
                    switch (response.MessageType)
                    {
                        case "system":
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"🔔 [{timestamp}] {response.Message}");
                            break;
                        case "chat":
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"💬 [{timestamp}] {response.Message}");
                            break;
                        default:
                            var color = Colors[response.Timestamp.GetHashCode() % Colors.Length];
                            Console.ForegroundColor = color;
                            Console.WriteLine($"📨 [{timestamp}] {response.Message}");
                            break;
                    }
                    Console.ResetColor();
                }
            });

            // Send messages
            int messageCount = 1;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{ClientName}> ");
                Console.ResetColor();
                
                var message = Console.ReadLine();
                
                if (string.IsNullOrEmpty(message) || message.ToLower() == "exit")
                    break;

                var request = new ChatMessage
                {
                    Message = message,
                    ClientId = ClientName,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    MessageType = "chat"
                };

                await call.RequestStream.WriteAsync(request);
                
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"✅ Message sent to hub");
                Console.ResetColor();
            }

            await call.RequestStream.CompleteAsync();
            await receiveTask;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error in chat hub session: {ex.Message}");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n👋 {ClientName} left the chat hub");
        Console.ResetColor();
    }
}
