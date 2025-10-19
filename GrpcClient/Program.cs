using Grpc.Net.Client;
using Demo;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcClient;

class Program
{
    private static GrpcDemoService.GrpcDemoServiceClient? _client;
    private static readonly string ServerUrl = "https://localhost:5001";

    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 gRPC Demo Client - Understanding gRPC flows");
        Console.WriteLine("==============================================");

        try
        {
            // Configure gRPC client
            using var channel = GrpcChannel.ForAddress(ServerUrl, new GrpcChannelOptions
            {
                // For local development with self-signed certificates
                HttpHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = 
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }
            });

            _client = new GrpcDemoService.GrpcDemoServiceClient(channel);

            Console.WriteLine($"✅ Connected to server: {ServerUrl}");
            Console.WriteLine();

            await ShowMainMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Connection error: {ex.Message}");
            Console.WriteLine("Make sure the gRPC server is started on https://localhost:5001");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static async Task ShowMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("🎯 MAIN MENU - gRPC Flow Types");
            Console.WriteLine("==============================");
            Console.WriteLine("1. 📞 Unary Call (One call, one response)");
            Console.WriteLine("2. 📡 Server Streaming (One call, multiple responses)");
            Console.WriteLine("3. 📤 Client Streaming (Multiple calls, one response)");
            Console.WriteLine("4. 🔄 Bidirectional Streaming (Multiple calls, multiple responses)");
            Console.WriteLine("5. ℹ️  Server Information");
            Console.WriteLine("6. 🎨 Complete Demonstration");
            Console.WriteLine("0. ❌ Exit");
            Console.WriteLine();

            Console.Write("Your choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await DemoUnaryCall();
                    break;
                case "2":
                    await DemoServerStreaming();
                    break;
                case "3":
                    await DemoClientStreaming();
                    break;
                case "4":
                    await DemoBidirectionalStreaming();
                    break;
                case "5":
                    await GetServerInfo();
                    break;
                case "6":
                    await FullDemo();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("❌ Invalid choice. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static async Task DemoUnaryCall()
    {
        Console.Clear();
        Console.WriteLine("📞 DEMONSTRATION: Unary Call");
        Console.WriteLine("============================");
        Console.WriteLine("A Unary call = 1 request → 1 response");
        Console.WriteLine();

        Console.Write("Enter your message: ");
        var message = Console.ReadLine() ?? "Default message";

        Console.WriteLine($"\n🔄 Sending request...");
        
        var request = new UnaryRequest
        {
            Message = message,
            ClientId = Environment.ProcessId,
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        };

        try
        {
            var response = await _client!.UnaryCallAsync(request);
            
            Console.WriteLine("✅ Response received:");
            Console.WriteLine($"   📨 Message: {response.Response}");
            Console.WriteLine($"   🆔 Server ID: {response.ServerId}");
            Console.WriteLine($"   ⏰ Timestamp: {response.Timestamp}");
            Console.WriteLine($"   ⚡ Processing time: {response.ProcessingTimeMs}ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static async Task DemoServerStreaming()
    {
        Console.Clear();
        Console.WriteLine("📡 DEMONSTRATION: Server Streaming");
        Console.WriteLine("===================================");
        Console.WriteLine("Server Streaming = 1 request → Multiple responses");
        Console.WriteLine();

        Console.Write("Enter your message: ");
        var message = Console.ReadLine() ?? "Streaming message";

        Console.WriteLine($"\n🔄 Starting server streaming...");
        Console.WriteLine("📥 Receiving messages...\n");

        var request = new StreamingRequest
        {
            Message = message,
            SequenceNumber = 1,
            ClientId = $"Client-{Environment.ProcessId}"
        };

        try
        {
            using var call = _client!.ServerStreaming(request);
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"📨 Response #{response.SequenceNumber}:");
                Console.WriteLine($"   💬 Message: {response.Response}");
                Console.WriteLine($"   🆔 Server ID: {response.ServerId}");
                Console.WriteLine($"   ⏰ Timestamp: {response.Timestamp}");
                Console.WriteLine($"   🔄 Type: {response.StreamType}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur: {ex.Message}");
        }

        Console.WriteLine("✅ Streaming completed. Press any key to continue...");
        Console.ReadKey();
    }

    static async Task DemoClientStreaming()
    {
        Console.Clear();
        Console.WriteLine("📤 DEMONSTRATION: Client Streaming");
        Console.WriteLine("===================================");
        Console.WriteLine("Client Streaming = Multiple requests → 1 response");
        Console.WriteLine();

        Console.WriteLine("📤 Sending multiple messages...");
        Console.WriteLine("(Type 'end' to finish sending)\n");

        try
        {
            using var call = _client!.ClientStreaming();
            
            int sequenceNumber = 1;
            while (true)
            {
                Console.Write($"Message #{sequenceNumber} (or 'end'): ");
                var message = Console.ReadLine();
                
                if (string.IsNullOrEmpty(message) || message.ToLower() == "end")
                    break;

                var request = new StreamingRequest
                {
                    Message = message,
                    SequenceNumber = sequenceNumber++,
                    ClientId = $"Client-{Environment.ProcessId}"
                };

                await call.RequestStream.WriteAsync(request);
                Console.WriteLine($"   ✅ Message #{request.SequenceNumber} sent");
            }

            await call.RequestStream.CompleteAsync();
            
            Console.WriteLine("\n🔄 Waiting for final response...");
            var response = await call;
            
            Console.WriteLine("✅ Final response received:");
            Console.WriteLine($"   📨 Message: {response.Response}");
            Console.WriteLine($"   🔢 Sequence: {response.SequenceNumber}");
            Console.WriteLine($"   🆔 Server ID: {response.ServerId}");
            Console.WriteLine($"   ⏰ Timestamp: {response.Timestamp}");
            Console.WriteLine($"   🔄 Type: {response.StreamType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static async Task DemoBidirectionalStreaming()
    {
        Console.Clear();
        Console.WriteLine("🔄 DEMONSTRATION: Bidirectional Streaming");
        Console.WriteLine("==========================================");
        Console.WriteLine("Bidirectional Streaming = Multiple requests ↔ Multiple responses");
        Console.WriteLine();

        Console.WriteLine("🔄 Starting bidirectional streaming...");
        Console.WriteLine("(Type 'end' to finish)\n");

        try
        {
            using var call = _client!.BidirectionalStreaming();
            
            // Start receiving responses in background
            var receiveTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine($"📨 Response #{response.SequenceNumber}:");
                    Console.WriteLine($"   💬 Message: {response.Response}");
                    Console.WriteLine($"   ⏰ Timestamp: {response.Timestamp}");
                    Console.WriteLine($"   🔄 Type: {response.StreamType}");
                    Console.WriteLine();
                }
            });

            // Send messages
            int sequenceNumber = 1;
            while (true)
            {
                Console.Write($"Message #{sequenceNumber} (or 'end'): ");
                var message = Console.ReadLine();
                
                if (string.IsNullOrEmpty(message) || message.ToLower() == "end")
                    break;

                var request = new StreamingRequest
                {
                    Message = message,
                    SequenceNumber = sequenceNumber++,
                    ClientId = $"Client-{Environment.ProcessId}"
                };

                await call.RequestStream.WriteAsync(request);
                Console.WriteLine($"   ✅ Message #{request.SequenceNumber} sent");
            }

            await call.RequestStream.CompleteAsync();
            await receiveTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur: {ex.Message}");
        }

        Console.WriteLine("✅ Bidirectional streaming completed. Press any key to continue...");
        Console.ReadKey();
    }

    static async Task GetServerInfo()
    {
        Console.Clear();
        Console.WriteLine("ℹ️  SERVER INFORMATION");
        Console.WriteLine("======================");

        try
        {
            var response = await _client!.GetServerInfoAsync(new Empty());
            
            Console.WriteLine("✅ Server information:");
            Console.WriteLine($"   🖥️  Name: {response.ServerName}");
            Console.WriteLine($"   📦 Version: {response.Version}");
            Console.WriteLine($"   🚀 Start time: {response.StartTime}");
            Console.WriteLine($"   🔗 Active connections: {response.ActiveConnections}");
            Console.WriteLine($"   🛠️  Supported features:");
            
            foreach (var feature in response.SupportedFeatures)
            {
                Console.WriteLine($"      • {feature}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static async Task FullDemo()
    {
        Console.Clear();
        Console.WriteLine("🎨 COMPLETE DEMONSTRATION");
        Console.WriteLine("=========================");
        Console.WriteLine("This demonstration will execute all types of gRPC flows");
        Console.WriteLine();

        Console.WriteLine("⏳ Starting in 3 seconds...");
        await Task.Delay(3000);

        // 1. Unary Call
        Console.WriteLine("\n1️⃣  TEST: Unary Call");
        await DemoUnaryCall();

        // 2. Server Streaming
        Console.WriteLine("\n2️⃣  TEST: Server Streaming");
        await DemoServerStreaming();

        // 3. Client Streaming
        Console.WriteLine("\n3️⃣  TEST: Client Streaming");
        await DemoClientStreaming();

        // 4. Bidirectional Streaming
        Console.WriteLine("\n4️⃣  TEST: Bidirectional Streaming");
        await DemoBidirectionalStreaming();

        Console.WriteLine("\n🎉 Complete demonstration finished!");
        Console.WriteLine("Press any key to return to menu...");
        Console.ReadKey();
    }
}
