using Grpc.Net.Client;
using Demo;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Diagnostics;

namespace GrpcClientStressTest;

class Program
{
    private static GrpcDemoService.GrpcDemoServiceClient? _client;
    private static readonly string ServerUrl = "https://localhost:5001";
    private static readonly string StressTestId = $"StressTest-{Random.Shared.Next(100, 999)}";

    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"⚡ Client Stress Test gRPC - {StressTestId}");
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
            Console.WriteLine($"✅ {StressTestId} connected to server: {ServerUrl}");
            Console.ResetColor();

            // Stress test menu
            await ShowStressTestMenu();
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

    static async Task ShowStressTestMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("⚡ MENU STRESS TEST gRPC");
            Console.WriteLine("========================");
            Console.ResetColor();
            Console.WriteLine("1. 🚀 Stress Test Unary (100 rapid calls)");
            Console.WriteLine("2. 📤 Stress Test Client Streaming (Massive upload)");
            Console.WriteLine("3. 🔄 Stress Test Bidirectional (Intensive chat)");
            Console.WriteLine("4. 🎯 Complete Stress Test (All patterns)");
            Console.WriteLine("0. ❌ Exit");
            Console.WriteLine();

            Console.Write("Your choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await StressTestUnary();
                    break;
                case "2":
                    await StressTestClientStreaming();
                    break;
                case "3":
                    await StressTestBidirectional();
                    break;
                case "4":
                    await StressTestComplete();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("❌ Choix invalide. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static async Task StressTestUnary()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🚀 STRESS TEST UNARY - 100 rapid calls");
        Console.WriteLine("==========================================");
        Console.ResetColor();

        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task<UnaryResponse>>();

        Console.WriteLine("📤 Sending 100 simultaneous unary calls...");

        for (int i = 1; i <= 100; i++)
        {
            var request = new UnaryRequest
            {
                Message = $"Stress test message {i}",
                ClientId = Environment.ProcessId,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            };

            tasks.Add(_client!.UnaryCallAsync(request).ResponseAsync);
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Stress test completed!");
        Console.WriteLine($"   📊 {responses.Length} responses received");
        Console.WriteLine($"   ⏱️  Total time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"   📈 Average: {stopwatch.ElapsedMilliseconds / (double)responses.Length:F2}ms per call");
        Console.ResetColor();

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static async Task StressTestClientStreaming()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("📤 STRESS TEST CLIENT STREAMING - Massive upload");
        Console.WriteLine("===============================================");
        Console.ResetColor();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var call = _client!.ClientStreaming();
            
            Console.WriteLine("📤 Sending 50 streaming messages...");

            for (int i = 1; i <= 50; i++)
            {
                var request = new StreamingRequest
                {
                    Message = $"Stress test message {i} - Massive data: {new string('X', 100)}",
                    SequenceNumber = i,
                    ClientId = StressTestId
                };

                await call.RequestStream.WriteAsync(request);
                
                if (i % 10 == 0)
                {
                    Console.WriteLine($"   📤 {i}/50 messages sent...");
                }
            }

            await call.RequestStream.CompleteAsync();
            
            Console.WriteLine("🔄 Waiting for final response...");
            var response = await call;
            
            stopwatch.Stop();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Massive upload completed!");
            Console.WriteLine($"   📊 Response: {response.Response}");
            Console.WriteLine($"   ⏱️  Total time: {stopwatch.ElapsedMilliseconds}ms");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Erreur: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static async Task StressTestBidirectional()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🔄 STRESS TEST BIDIRECTIONAL - Intensive chat");
        Console.WriteLine("============================================");
        Console.ResetColor();

        try
        {
            using var call = _client!.BidirectionalStreaming();
            
            // Démarrer la réception
            var receiveTask = Task.Run(async () =>
            {
                var messageCount = 0;
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    messageCount++;
                    if (messageCount % 10 == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"📨 {messageCount} responses received...");
                        Console.ResetColor();
                    }
                }
            });

            // Send many messages
            Console.WriteLine("📤 Sending 30 bidirectional messages...");
            
            for (int i = 1; i <= 30; i++)
            {
                var request = new StreamingRequest
                {
                    Message = $"Message intensif {i} - {DateTime.Now:HH:mm:ss.fff}",
                    SequenceNumber = i,
                    ClientId = StressTestId
                };

                await call.RequestStream.WriteAsync(request);
                await Task.Delay(50); // Petit délai pour éviter de surcharger
            }

            await call.RequestStream.CompleteAsync();
            await receiveTask;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Bidirectional stress test completed!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Erreur: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static async Task StressTestComplete()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("🎯 COMPLETE STRESS TEST - All patterns");
        Console.WriteLine("==========================================");
        Console.ResetColor();

        Console.WriteLine("⏳ Starting complete stress test in 3 seconds...");
        await Task.Delay(3000);

        var stopwatch = Stopwatch.StartNew();

        // 1. Stress Test Unary
        Console.WriteLine("\n1️⃣  Stress Test Unary...");
        await StressTestUnary();

        // 2. Stress Test Client Streaming
        Console.WriteLine("\n2️⃣  Stress Test Client Streaming...");
        await StressTestClientStreaming();

        // 3. Stress Test Bidirectional
        Console.WriteLine("\n3️⃣  Stress Test Bidirectional...");
        await StressTestBidirectional();

        stopwatch.Stop();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n🎉 COMPLETE STRESS TEST FINISHED!");
        Console.WriteLine($"   ⏱️  Total time: {stopwatch.ElapsedMilliseconds}ms");
        Console.ResetColor();

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
    }
}
