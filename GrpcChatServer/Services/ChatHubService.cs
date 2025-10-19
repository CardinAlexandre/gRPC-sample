using Grpc.Core;
using Chat;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Microsoft.AspNetCore.Connections;

namespace Chat;

public class ChatHubService : ChatService.ChatServiceBase
{
    private readonly ILogger<ChatHubService> _logger;
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<ChatMessage>> _connectedClients = new();

    public ChatHubService(ILogger<ChatHubService> logger)
    {
        _logger = logger;
    }

    // Bidirectional streaming for chat
    public override async Task Chat(IAsyncStreamReader<ChatMessage> requestStream, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
    {
        var clientId = $"ChatUser-{Random.Shared.Next(1000, 9999)}";
        
        _logger.LogInformation("🔵 CHAT HUB - New client connected: {ClientId}", clientId);
        
        try
        {
            // Add client to connected clients list
            _connectedClients.TryAdd(clientId, responseStream);
            
            // Notify all other clients of the new connection
            await BroadcastMessage($"👋 {clientId} joined the chat!", "system", clientId);
            
            // Listen for messages from this client
            await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
            {
                _logger.LogInformation("🔵 CHAT HUB - Message received from {ClientId}: {Message}", request.ClientId, request.Message);
                
                // Relay message to all connected clients (including sender)
                await BroadcastMessage(request.Message, "chat", request.ClientId);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("❌ CHAT HUB - Client {ClientId} disconnected (cancelled)", clientId);
        }
        catch (IOException ex) when (ex.InnerException is ConnectionResetException || ex.InnerException is SocketException)
        {
            _logger.LogInformation("❌ CHAT HUB - Client {ClientId} disconnected (connection closed)", clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ CHAT HUB - Unexpected error for client {ClientId}", clientId);
        }
        finally
        {
            // Remove client from connected clients list
            _connectedClients.TryRemove(clientId, out _);
            
            // Notify all other clients of disconnection (only if there are other clients)
            if (_connectedClients.Count > 0)
            {
                try
                {
                    await BroadcastMessage($"👋 {clientId} left the chat!", "system", clientId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ CHAT HUB - Error notifying disconnection for {ClientId}", clientId);
                }
            }
            
            _logger.LogInformation("✅ CHAT HUB - Client {ClientId} disconnected cleanly", clientId);
        }
    }

    private async Task BroadcastMessage(string message, string messageType, string senderId)
    {
        var response = new ChatMessage
        {
            ClientId = senderId,
            Message = message,
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            MessageType = messageType
        };

        var tasks = new List<Task>();
        var clientsToRemove = new List<string>();

        foreach (var client in _connectedClients)
        {
            try
            {
                // Add sender identifier to message
                if (messageType == "chat")
                {
                    response.Message = $"[{senderId}] {message}";
                }
                
                tasks.Add(client.Value.WriteAsync(response));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("⚠️ CHAT HUB - Error sending to {ClientId}: {Error}", client.Key, ex.Message);
                clientsToRemove.Add(client.Key);
            }
        }

        // Clean up disconnected clients before sending messages
        foreach (var clientId in clientsToRemove)
        {
            _connectedClients.TryRemove(clientId, out _);
        }

        // Wait for all messages to be sent
        await Task.WhenAll(tasks);
        
        _logger.LogInformation("🟢 CHAT HUB - Message broadcast to {ClientCount} clients: {Message}", 
            _connectedClients.Count, response.Message);
    }

    // Method to get the number of connected clients
    public static int GetConnectedClientsCount()
    {
        return _connectedClients.Count;
    }
}
