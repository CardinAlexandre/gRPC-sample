using Grpc.Core;
using Demo;
using Google.Protobuf.WellKnownTypes;
using System.Diagnostics;

namespace GrpcDemo;

public class GrpcDemoServiceImpl : GrpcDemoService.GrpcDemoServiceBase
{
    private readonly ILogger<GrpcDemoServiceImpl> _logger;
    private static int _activeConnections = 0;
    private static readonly DateTime _startTime = DateTime.Now;

    public GrpcDemoServiceImpl(ILogger<GrpcDemoServiceImpl> logger)
    {
        _logger = logger;
    }

    // 1. UNARY CALL - One call, one response
    public override async Task<UnaryResponse> UnaryCall(UnaryRequest request, ServerCallContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("🔵 UNARY CALL received: {Message} (Client ID: {ClientId})", 
            request.Message, request.ClientId);

        // Simulate processing
        await Task.Delay(100);

        var response = new UnaryResponse
        {
            Response = $"Hello {request.Message} ! Processed by server.",
            ServerId = 1001,
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds.ToString()
        };

        _logger.LogInformation("🟢 UNARY RESPONSE sent: {Response} (Time: {Time}ms)", 
            response.Response, response.ProcessingTimeMs);

        return response;
    }

    // 2. SERVER STREAMING - One call, multiple responses
    public override async Task ServerStreaming(StreamingRequest request, IServerStreamWriter<StreamingResponse> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("🔵 SERVER STREAMING started: {Message} (Client: {ClientId})", 
            request.Message, request.ClientId);

        for (int i = 1; i <= 5; i++)
        {
            // Check if client is still connected
            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("❌ SERVER STREAMING cancelled by client");
                break;
            }

            var response = new StreamingResponse
            {
                Response = $"Streaming message {i}/5 for: {request.Message}",
                SequenceNumber = i,
                ServerId = "1001",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                StreamType = "server_stream"
            };

            _logger.LogInformation("🟢 SERVER STREAMING sent: {Response} (Séquence: {Seq})", 
                response.Response, i);

            await responseStream.WriteAsync(response);
            
            // Delay between messages to visualize streaming
            await Task.Delay(1000);
        }

        _logger.LogInformation("✅ SERVER STREAMING completed pour client: {ClientId}", request.ClientId);
    }

    // 3. CLIENT STREAMING - Multiple calls, one response
    public override async Task<StreamingResponse> ClientStreaming(IAsyncStreamReader<StreamingRequest> requestStream, ServerCallContext context)
    {
        _logger.LogInformation("🔵 CLIENT STREAMING started");

        var messages = new List<string>();
        var sequenceNumbers = new List<int>();

        try
        {
            await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
            {
                _logger.LogInformation("🔵 CLIENT STREAMING received: {Message} (Séquence: {Seq}, Client: {ClientId})", 
                    request.Message, request.SequenceNumber, request.ClientId);

                messages.Add(request.Message);
                sequenceNumbers.Add(request.SequenceNumber);

                // Simulate processing
                await Task.Delay(200);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("❌ CLIENT STREAMING cancelled by client");
        }

        var response = new StreamingResponse
        {
            Response = $"Processed {messages.Count} client messages: {string.Join(", ", messages)}",
            SequenceNumber = sequenceNumbers.Count,
            ServerId = "1001",
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            StreamType = "client_stream"
        };

        _logger.LogInformation("🟢 CLIENT STREAMING réponse: {Response} ({Count} messages traités)", 
            response.Response, messages.Count);

        return response;
    }

    // 4. BIDIRECTIONAL STREAMING - Multiple calls, multiple responses
    public override async Task BidirectionalStreaming(IAsyncStreamReader<StreamingRequest> requestStream, IServerStreamWriter<StreamingResponse> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("🔵 BIDIRECTIONAL STREAMING started");

        try
        {
            await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
            {
                _logger.LogInformation("🔵 BIDIRECTIONAL STREAMING received: {Message} (Séquence: {Seq}, Client: {ClientId})", 
                    request.Message, request.SequenceNumber, request.ClientId);

                // Immediate processing and response
                var response = new StreamingResponse
                {
                    Response = $"Bidirectional echo: {request.Message}",
                    SequenceNumber = request.SequenceNumber,
                    ServerId = "1001",
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    StreamType = "bidirectional"
                };

                _logger.LogInformation("🟢 BIDIRECTIONAL STREAMING sent: {Response} (Séquence: {Seq})", 
                    response.Response, request.SequenceNumber);

                await responseStream.WriteAsync(response);

                // Delay to simulate processing
                await Task.Delay(500);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("❌ BIDIRECTIONAL STREAMING cancelled by client");
        }

        _logger.LogInformation("✅ BIDIRECTIONAL STREAMING completed");
    }

    // 5. Server information
    public override Task<ServerInfo> GetServerInfo(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("🔵 GET SERVER INFO requested");

        var response = new ServerInfo
        {
            ServerName = "GrpcDemo Server",
            Version = "1.0.0",
            StartTime = _startTime.ToString("yyyy-MM-dd HH:mm:ss"),
            ActiveConnections = Interlocked.Increment(ref _activeConnections),
            SupportedFeatures = { "Unary Calls", "Server Streaming", "Client Streaming", "Bidirectional Streaming", "Metadata Support" }
        };

        _logger.LogInformation("🟢 SERVER INFO sent: {ServerName} v{Version}", 
            response.ServerName, response.Version);

        return Task.FromResult(response);
    }
}
