# 🚀 gRPC Demo - Understanding gRPC Flows

This .NET project demonstrates all types of gRPC communication flows with practical examples and real-time visualization interface. It includes two main demonstrations: **Basic gRPC Flows** and **Multi-Client Chat**.

## 📋 Table of Contents

- [🎯 Objective](#-objective)
- [🏗️ Architecture](#️-architecture)
- [🔧 Installation](#-installation)
- [🚀 Quick Start](#-quick-start)
- [📚 gRPC Flow Types](#-grpc-flow-types)
- [💻 Usage](#-usage)
- [🌐 Web Interface](#-web-interface)
- [📊 Logs and Monitoring](#-logs-and-monitoring)
- [🔍 Troubleshooting](#-troubleshooting)

## 🎯 Objective

This project aims to:
- **Visually explain** different types of gRPC flows
- **Demonstrate** each pattern with concrete examples
- **Enable interactive learning** of gRPC concepts
- **Visualize** real-time communications

## 🏗️ Architecture

The project consists of 9 applications organized into two main demonstrations:

### 📚 Project 1: Basic gRPC Flows
**Learn the 4 fundamental gRPC communication patterns**

```
GrpcDemo/
├── GrpcServer/              # Main gRPC server with all patterns
├── GrpcClient/              # Interactive console client
├── GrpcWebUI/               # Web visualization interface
├── GrpcClientMonitor/       # Real-time monitoring client
├── GrpcClientStressTest/    # Performance testing client
└── README.md                # This documentation
```

### 💬 Project 2: Multi-Client Chat
**Real-world example with bidirectional streaming**

```
GrpcDemo/
├── GrpcChatServer/          # Dedicated chat hub server
├── GrpcClientChatHub/       # Chat hub client (multiple instances)
└── GrpcClientChat/          # Alternative bidirectional chat client
```

### Architecture Diagram

```mermaid
graph TD
    subgraph "Basic gRPC Flows"
        A[Console Client] --> B[gRPC Server]
        C[Web Interface] --> B
        D[Monitor Client] --> B
        E[Stress Test] --> B
        B --> F[Unary Call]
        B --> G[Server Streaming]
        B --> H[Client Streaming]
        B --> I[Bidirectional Streaming]
    end
    
    subgraph "Multi-Client Chat"
        J[Chat Client 1] --> K[Chat Hub Server]
        L[Chat Client 2] --> K
        M[Chat Client 3] --> K
        K --> N[Broadcast Messages]
    end
```

## 🔧 Installation

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Modern web browser

### Installation Steps

1. **Clone the project**
```bash
git clone <repository-url>
cd GrpcDemo
```

2. **Restore NuGet packages**
```bash
dotnet restore
```

3. **Build the project**
```bash
dotnet build
```

## 🚀 Quick Start

### 🎯 Interactive Menu (Recommended)

The easiest way to start is using the interactive menu:

```bash
.\start-demo.ps1
```

This will present you with options to choose between:
1. **📚 Basic gRPC Flows** - Learn the 4 fundamental patterns
2. **💬 Multi-Client Chat** - Test real-world bidirectional streaming
3. **🔧 Development Tools** - Build and clean projects

### 📚 Project 1: Basic gRPC Flows

**Objective:** Learn and understand the 4 fundamental gRPC communication patterns

#### Start Basic Flow Services
```bash
.\start-basic-flows.ps1
```

This starts:
- **GrpcServer** (Port 5001) - Main gRPC server
- **GrpcWebUI** (Port 5002) - Web visualization interface

#### Start Test Clients
```bash
.\start-basic-clients.ps1
```

This starts:
- **GrpcClient** - Interactive console client
- **GrpcClientMonitor** - Real-time monitoring client
- **GrpcClientStressTest** - Performance testing client

#### URLs Available:
- 🌐 **Server:** `https://localhost:5001`
- 🌐 **Web Interface:** `https://localhost:5002`

### 💬 Project 2: Multi-Client Chat

**Objective:** Experience real-world bidirectional streaming with multiple clients

#### Start Chat Services
```bash
.\start-chat-services.ps1
```

This starts:
- **GrpcChatServer** (Port 5003) - Dedicated chat hub server

#### Start Chat Clients
```bash
.\start-chat-clients.ps1
```

This starts 3 chat clients that can communicate with each other:
- **ChatUser-XXXX** - Multiple instances for testing

#### URLs Available:
- 🌐 **Chat Server:** `https://localhost:5003`

## 🎯 Which Project Should You Choose?

### 📚 Choose "Basic gRPC Flows" if you want to:
- **Learn the fundamentals** of gRPC communication
- **Understand the 4 patterns** (Unary, Server Streaming, Client Streaming, Bidirectional)
- **See visual demonstrations** with the web interface
- **Test each pattern individually** with detailed logs
- **Monitor real-time communications** between client and server

### 💬 Choose "Multi-Client Chat" if you want to:
- **Experience real-world applications** of bidirectional streaming
- **Test multi-client scenarios** with multiple users
- **See how gRPC handles** concurrent connections
- **Understand chat applications** and real-time messaging
- **Test scalability** with multiple simultaneous clients

### 🔄 Both Projects Complement Each Other:
- **Basic Flows** teach you the theory and individual patterns
- **Multi-Client Chat** shows you practical implementation
- **Together** they provide a complete understanding of gRPC

## 📚 gRPC Flow Types

### 1. 📞 Unary Call
**Pattern:** One call, one response

```csharp
// Client
var response = await client.UnaryCallAsync(request);

// Server
public override async Task<UnaryResponse> UnaryCall(UnaryRequest request, ServerCallContext context)
```

**Use cases:** REST-like APIs, authentication, simple queries

### 2. 📡 Server Streaming
**Pattern:** One call, multiple responses

```csharp
// Client
using var call = client.ServerStreaming(request);
await foreach (var response in call.ResponseStream.ReadAllAsync())

// Server
public override async Task ServerStreaming(StreamingRequest request, IServerStreamWriter<StreamingResponse> responseStream, ServerCallContext context)
```

**Use cases:** Real-time notifications, file downloads, data streaming

### 3. 📤 Client Streaming
**Pattern:** Multiple calls, one response

```csharp
// Client
using var call = client.ClientStreaming();
await call.RequestStream.WriteAsync(request);
await call.RequestStream.CompleteAsync();
var response = await call;

// Server
public override async Task<StreamingResponse> ClientStreaming(IAsyncStreamReader<StreamingRequest> requestStream, ServerCallContext context)
```

**Use cases:** File uploads, data collection, batch processing

### 4. 🔄 Bidirectional Streaming
**Pattern:** Multiple calls, multiple responses

```csharp
// Client
using var call = client.BidirectionalStreaming();
await call.RequestStream.WriteAsync(request);
await foreach (var response in call.ResponseStream.ReadAllAsync())

// Server
public override async Task BidirectionalStreaming(IAsyncStreamReader<StreamingRequest> requestStream, IServerStreamWriter<StreamingResponse> responseStream, ServerCallContext context)
```

**Use cases:** Real-time chat, multiplayer games, collaboration

## 💬 Multi-Client Chat Implementation

### Chat Hub Architecture

The chat system demonstrates advanced bidirectional streaming with multiple clients:

```mermaid
graph TD
    A[Chat Client 1] --> B[Chat Hub Server]
    C[Chat Client 2] --> B
    D[Chat Client 3] --> B
    B --> E[Message Broadcasting]
    E --> A
    E --> C
    E --> D
```

### Key Features

- **Real-time messaging** between multiple clients
- **Automatic client management** (connect/disconnect detection)
- **Message broadcasting** to all connected clients
- **System notifications** (join/leave messages)
- **Concurrent connection handling**

### Chat Client Features

Each chat client (`GrpcClientChatHub`) provides:
- **Unique client ID** (ChatUser-XXXX)
- **Real-time message reception** from other clients
- **Interactive message sending**
- **Connection status monitoring**
- **Automatic reconnection** handling

### Server Features

The chat server (`GrpcChatServer`) provides:
- **Client connection management**
- **Message broadcasting** to all connected clients
- **Automatic cleanup** of disconnected clients
- **System message generation**
- **Concurrent stream handling**

## 💻 Usage

### Console Client

The console client offers an interactive menu:

```
🎯 MAIN MENU - gRPC Flow Types
=============================
1. 📞 Unary Call (One call, one response)
2. 📡 Server Streaming (One call, multiple responses)
3. 📤 Client Streaming (Multiple calls, one response)
4. 🔄 Bidirectional Streaming (Multiple calls, multiple responses)
5. ℹ️  Server Information
6. 🎨 Complete Demonstration
0. ❌ Exit
```

### Usage Examples

#### Basic gRPC Flows - Test Unary Call
```bash
# In the console client
Your choice: 1
Enter your message: Hello gRPC!
```

#### Basic gRPC Flows - Test Server Streaming
```bash
# In the console client
Your choice: 2
Enter your message: Streaming test
# The server will send 5 messages with a 1-second delay
```

#### Multi-Client Chat - Start Chat Session
```bash
# Start chat server
.\start-chat-services.ps1

# Start multiple chat clients
.\start-chat-clients.ps1

# In any chat client window:
ChatUser-1234> Hello everyone!
# This message will appear in all other chat clients
```

#### Multi-Client Chat - Test Message Broadcasting
```bash
# Client 1 types:
ChatUser-1234> Testing broadcast

# Client 2 receives:
🔔 [14:30:15] ChatUser-1234 joined the chat!
💬 [14:30:20] ChatUser-1234: Testing broadcast

# Client 3 receives:
💬 [14:30:20] ChatUser-1234: Testing broadcast
```

## 🌐 Web Interface

The web interface (`https://localhost:5002`) offers:

- **Real-time visualization** of gRPC flows
- **Intuitive graphical interface**
- **Colored logs** for each type of communication
- **Explanatory diagrams** of flows
- **Interactive tests** for each pattern

### Features

- ✅ Tests for all flow types
- ✅ Real-time colored logs
- ✅ Visual flow diagrams
- ✅ Responsive interface
- ✅ Error handling

## 📊 Logs and Monitoring

### Server Logs

The server displays detailed logs:

```
🔵 UNARY CALL received: Hello gRPC! (Client ID: 1234)
🟢 UNARY RESPONSE sent: Hello Hello gRPC! ! Processed by server. (Time: 105ms)
```

### Log Types

- 🔵 **Incoming requests** (Blue)
- 🟢 **Outgoing responses** (Green)
- ❌ **Errors** (Red)
- ℹ️ **Information** (Yellow)

### Connection Monitoring

```bash
# Server information
curl https://localhost:5001/info
```

## 🔍 Troubleshooting

### Common Issues

#### 1. SSL Certificate Error
```bash
# Solution: Accept self-signed certificates
# The code already handles this automatically
```

#### 2. Port already in use
```bash
# Change ports in Program.cs
app.Run("https://localhost:5003");  # Instead of 5001
```

#### 3. Connection error
```bash
# Check that the server is started
curl https://localhost:5001
# Should return: "gRPC Demo Server - Ready to receive calls!"
```

### Debug Logs

To enable detailed logs:

```csharp
// In Program.cs
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

## 🎓 Concepts Learned

After using this project, you will understand:

### 📚 From Basic gRPC Flows:
1. **The 4 types of gRPC flows** and their use cases
2. **The difference** between each pattern
3. **Implementation** on server and client sides
4. **Stream management** and connections
5. **Monitoring** and debugging of gRPC flows
6. **Best practices** for each type of flow

### 💬 From Multi-Client Chat:
1. **Real-world bidirectional streaming** implementation
2. **Multi-client connection management**
3. **Message broadcasting** patterns
4. **Concurrent stream handling**
5. **Client lifecycle management** (connect/disconnect)
6. **Scalable gRPC architecture** for chat applications

## 🚀 Next Steps

- Add gRPC authentication
- Implement compression
- Add custom metadata
- Test with clients in other languages
- Implement gRPC interception

## 📝 License

This project is provided for educational purposes. Free to use and modify.

---

**🎉 Congratulations!** You now master gRPC flows! 

For any questions, consult the detailed logs or test with the interactive web interface.
