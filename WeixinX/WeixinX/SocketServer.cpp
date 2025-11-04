#include "SocketServer.h"
#include "util.h"
#include <format>

namespace WeixinX {
namespace Socket {

// ============================================================================
// ClientConnection 实现
// ============================================================================

ClientConnection::ClientConnection(SOCKET socket, SocketServer* server)
    : m_socket(socket)
    , m_server(server)
    , m_connected(true)
{
}

ClientConnection::~ClientConnection()
{
    Stop();
}

void ClientConnection::Start()
{
    m_receiveThread = std::thread(&ClientConnection::ReceiveThread, this);
}

void ClientConnection::Stop()
{
    if (m_connected.exchange(false)) {
        closesocket(m_socket);
        if (m_receiveThread.joinable()) {
            m_receiveThread.join();
        }
    }
}

bool ClientConnection::Send(const std::string& message)
{
    if (!m_connected) {
        return false;
    }

    try {
        // 发送消息长度（4字节）
        uint32_t length = static_cast<uint32_t>(message.length());
        uint32_t networkLength = htonl(length);
        
        int sent = send(m_socket, reinterpret_cast<const char*>(&networkLength), sizeof(networkLength), 0);
        if (sent != sizeof(networkLength)) {
            util::logging::print("Failed to send message length");
            Stop();
            return false;
        }

        // 发送消息体
        sent = send(m_socket, message.c_str(), static_cast<int>(message.length()), 0);
        if (sent != static_cast<int>(message.length())) {
            util::logging::print("Failed to send message body");
            Stop();
            return false;
        }

        return true;
    }
    catch (...) {
        Stop();
        return false;
    }
}

bool ClientConnection::ReceiveExact(char* buffer, int length)
{
    int totalReceived = 0;
    while (totalReceived < length && m_connected) {
        int received = recv(m_socket, buffer + totalReceived, length - totalReceived, 0);
        if (received <= 0) {
            return false;
        }
        totalReceived += received;
    }
    return totalReceived == length;
}

void ClientConnection::ReceiveThread()
{
    util::logging::print("Client connected from socket {}", m_socket);

    while (m_connected) {
        try {
            // 接收消息长度（4字节）
            uint32_t networkLength = 0;
            if (!ReceiveExact(reinterpret_cast<char*>(&networkLength), sizeof(networkLength))) {
                util::logging::print("Client disconnected or failed to receive length");
                break;
            }

            uint32_t length = ntohl(networkLength);
            if (length == 0 || length > 10 * 1024 * 1024) { // 限制最大10MB
                util::logging::print("Invalid message length: {}", length);
                break;
            }

            // 接收消息体
            std::vector<char> buffer(length + 1);
            if (!ReceiveExact(buffer.data(), length)) {
                util::logging::print("Failed to receive message body");
                break;
            }
            buffer[length] = '\0';

            std::string message(buffer.data(), length);
            ProcessMessage(message);
        }
        catch (const std::exception& e) {
            util::logging::print("Exception in receive thread: {}", e.what());
            break;
        }
    }

    Stop();
    m_server->RemoveClient(this);
}

void ClientConnection::ProcessMessage(const std::string& message)
{
    try {
        util::logging::wPrint(L"Received: {}", util::utf8ToUtf16(message.c_str()));

        // 解析 JSON
        Json::Value request;
        JSONCPP_STRING err;
        Json::CharReaderBuilder builder;
        const std::unique_ptr<Json::CharReader> reader(builder.newCharReader());
        
        if (!reader->parse(message.c_str(), message.c_str() + message.length(), &request, &err)) {
            util::logging::print("Failed to parse JSON: {}", err);
            return;
        }

        // 提取请求信息
        int id = request.get("id", 0).asInt();
        std::string method = request.get("method", "").asString();
        Json::Value params = request.get("params", Json::Value(Json::arrayValue));

        // 处理命令
        Json::Value result = m_server->HandleCommand(method, params);

        // 构建响应
        Json::Value response;
        response["id"] = id;
        if (result.isMember("error")) {
            response["error"] = result["error"];
            response["result"] = Json::Value::null;
        } else {
            response["result"] = result;
            response["error"] = Json::Value::null;
        }

        // 发送响应
        Json::StreamWriterBuilder writerBuilder;
        writerBuilder["indentation"] = "";
        writerBuilder["emitUTF8"] = true;
        std::string responseStr = Json::writeString(writerBuilder, response);
        
        Send(responseStr);
    }
    catch (const std::exception& e) {
        util::logging::print("Exception in ProcessMessage: {}", e.what());
    }
}

// ============================================================================
// SocketServer 实现
// ============================================================================

SocketServer::SocketServer(int port)
    : m_port(port)
    , m_listenSocket(INVALID_SOCKET)
    , m_running(false)
{
}

SocketServer::~SocketServer()
{
    Stop();
}

bool SocketServer::Start()
{
    if (m_running) {
        return true;
    }

    // 初始化 Winsock
    WSADATA wsaData;
    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (result != 0) {
        util::logging::print("WSAStartup failed: {}", result);
        return false;
    }

    // 创建监听 socket
    m_listenSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (m_listenSocket == INVALID_SOCKET) {
        util::logging::print("socket() failed: {}", WSAGetLastError());
        WSACleanup();
        return false;
    }

    // 设置地址重用
    int reuseAddr = 1;
    setsockopt(m_listenSocket, SOL_SOCKET, SO_REUSEADDR, (const char*)&reuseAddr, sizeof(reuseAddr));

    // 绑定端口
    sockaddr_in serverAddr{};
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_addr.s_addr = INADDR_ANY;
    serverAddr.sin_port = htons(m_port);

    // 使用 ::bind 明确指定全局命名空间（WinSock 的 bind，而不是 std::bind）
    if (::bind(m_listenSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        util::logging::print("bind() failed: {}", WSAGetLastError());
        closesocket(m_listenSocket);
        WSACleanup();
        return false;
    }

    // 开始监听
    if (listen(m_listenSocket, SOMAXCONN) == SOCKET_ERROR) {
        util::logging::print("listen() failed: {}", WSAGetLastError());
        closesocket(m_listenSocket);
        WSACleanup();
        return false;
    }

    m_running = true;
    m_acceptThread = std::thread(&SocketServer::AcceptThread, this);

    util::logging::print("SocketServer started on port {}", m_port);
    return true;
}

void SocketServer::Stop()
{
    if (!m_running.exchange(false)) {
        return;
    }

    util::logging::print("Stopping SocketServer...");

    // 关闭监听 socket
    if (m_listenSocket != INVALID_SOCKET) {
        closesocket(m_listenSocket);
        m_listenSocket = INVALID_SOCKET;
    }

    // 等待接受线程结束
    if (m_acceptThread.joinable()) {
        m_acceptThread.join();
    }

    // 断开所有客户端
    {
        std::lock_guard<std::mutex> lock(m_clientsMutex);
        for (auto& client : m_clients) {
            client->Stop();
        }
        m_clients.clear();
    }

    WSACleanup();
    util::logging::print("SocketServer stopped");
}

void SocketServer::AcceptThread()
{
    while (m_running) {
        sockaddr_in clientAddr{};
        int clientAddrLen = sizeof(clientAddr);
        
        SOCKET clientSocket = accept(m_listenSocket, (sockaddr*)&clientAddr, &clientAddrLen);
        if (clientSocket == INVALID_SOCKET) {
            if (m_running) {
                util::logging::print("accept() failed: {}", WSAGetLastError());
            }
            continue;
        }

        // 创建客户端连接
        auto client = std::make_unique<ClientConnection>(clientSocket, this);
        client->Start();

        {
            std::lock_guard<std::mutex> lock(m_clientsMutex);
            m_clients.push_back(std::move(client));
        }

        util::logging::print("New client connected, total clients: {}", m_clients.size());
    }
}

void SocketServer::RegisterHandler(const std::string& method, CommandHandler handler)
{
    std::lock_guard<std::mutex> lock(m_handlersMutex);
    m_handlers[method] = handler;
    util::logging::print("Registered handler for method: {}", method);
}

Json::Value SocketServer::HandleCommand(const std::string& method, const Json::Value& params)
{
    std::lock_guard<std::mutex> lock(m_handlersMutex);
    
    auto it = m_handlers.find(method);
    if (it == m_handlers.end()) {
        Json::Value error;
        error["error"] = std::format("Unknown method: {}", method);
        util::logging::print("Unknown method: {}", method);
        return error;
    }

    try {
        return it->second(params);
    }
    catch (const std::exception& e) {
        Json::Value error;
        error["error"] = std::format("Handler exception: {}", e.what());
        util::logging::print("Handler exception for {}: {}", method, e.what());
        return error;
    }
}

void SocketServer::Broadcast(const std::string& method, const Json::Value& params)
{
    Json::Value notification;
    notification["method"] = method;
    notification["params"] = params;

    Json::StreamWriterBuilder builder;
    builder["indentation"] = "";
    builder["emitUTF8"] = true;
    std::string message = Json::writeString(builder, notification);

    std::lock_guard<std::mutex> lock(m_clientsMutex);
    for (auto& client : m_clients) {
        if (client->IsConnected()) {
            client->Send(message);
        }
    }

    util::logging::print("Broadcasted {} to {} clients", method, m_clients.size());
}

void SocketServer::RemoveClient(ClientConnection* client)
{
    std::lock_guard<std::mutex> lock(m_clientsMutex);
    m_clients.erase(
        std::remove_if(m_clients.begin(), m_clients.end(),
            [client](const std::unique_ptr<ClientConnection>& c) {
                return c.get() == client;
            }),
        m_clients.end()
    );
    util::logging::print("Client removed, remaining clients: {}", m_clients.size());
}

} // namespace Socket
} // namespace WeixinX

