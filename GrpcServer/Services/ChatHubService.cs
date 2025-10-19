using Grpc.Core;
using Chat;
using System.Collections.Concurrent;

namespace GrpcDemo;

public class ChatHubService : ChatService.ChatServiceBase
{
    private readonly ILogger<ChatHubService> _logger;
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<ChatMessage>> _connectedClients = new();
    private static int _messageCounter = 0;

    public ChatHubService(ILogger<ChatHubService> logger)
    {
        _logger = logger;
    }

    // Streaming bidirectionnel pour le chat
    public override async Task Chat(IAsyncStreamReader<ChatMessage> requestStream, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
    {
        var clientId = $"ChatUser-{Random.Shared.Next(1000, 9999)}";
        var clientInfo = $"Client {clientId} depuis {context.Peer}";
        
        _logger.LogInformation("🔵 CHAT HUB - Nouveau client connecté: {ClientId}", clientId);
        
        try
        {
            // Ajouter le client à la liste des clients connectés
            _connectedClients.TryAdd(clientId, responseStream);
            
            // Notifier tous les autres clients de la nouvelle connexion
            await BroadcastMessage($"👋 {clientId} a rejoint le chat!", "system", clientId);
            
            // Écouter les messages de ce client
            await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
            {
                _logger.LogInformation("🔵 CHAT HUB - Message reçu de {ClientId}: {Message}", clientId, request.Message);
                
                // Relayer le message à tous les clients connectés (y compris l'expéditeur)
                await BroadcastMessage(request.Message, "chat", request.ClientId);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("❌ CHAT HUB - Client {ClientId} déconnecté", clientId);
        }
        finally
        {
            // Retirer le client de la liste des clients connectés
            _connectedClients.TryRemove(clientId, out _);
            
            // Notifier tous les autres clients de la déconnexion
            await BroadcastMessage($"👋 {clientId} left the chat!", "system", clientId);
            
            _logger.LogInformation("✅ CHAT HUB - Client {ClientId} déconnecté proprement", clientId);
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
                // Ajouter l'identifiant de l'expéditeur au message
                if (messageType == "chat")
                {
                    response.Message = $"[{senderId}] {message}";
                }
                
                tasks.Add(client.Value.WriteAsync(response));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("⚠️ CHAT HUB - Erreur envoi à {ClientId}: {Error}", client.Key, ex.Message);
                clientsToRemove.Add(client.Key);
            }
        }

        // Nettoyer les clients déconnectés
        foreach (var clientId in clientsToRemove)
        {
            _connectedClients.TryRemove(clientId, out _);
        }

        // Attendre que tous les messages soient envoyés
        await Task.WhenAll(tasks);
        
        _logger.LogInformation("🟢 CHAT HUB - Message diffusé à {ClientCount} clients: {Message}", 
            _connectedClients.Count, response.Message);
    }

    // Method to get the number of connected clients
    public static int GetConnectedClientsCount()
    {
        return _connectedClients.Count;
    }
}
