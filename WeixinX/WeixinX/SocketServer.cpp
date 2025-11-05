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
        util::logging::print("Stopping client connection");
        
        // 先关闭 socket，触发 recv() 返回
        if (m_socket != INVALID_SOCKET) {
            shutdown(m_socket, SD_BOTH);  // 优雅关闭
            closesocket(m_socket);
            m_socket = INVALID_SOCKET;
        }
        
        // 等待接收线程结束
        if (m_receiveThread.joinable()) {
            try {
                // 尝试 join，最多等待 3 秒
                util::logging::print("Waiting for receive thread to finish");
                m_receiveThread.join();
                util::logging::print("Receive thread joined successfully");
            }
            catch (const std::exception& e) {
                util::logging::print("Exception while joining receive thread: {}", e.what());
            }
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
            util::logging::print("Failed to send message length, error: {}", WSAGetLastError());
            // 只标记断开，不调用 Stop()，避免潜在的死锁
            m_connected = false;
            return false;
        }

        // 发送消息体
        sent = send(m_socket, message.c_str(), static_cast<int>(message.length()), 0);
        if (sent != static_cast<int>(message.length())) {
            util::logging::print("Failed to send message body, error: {}", WSAGetLastError());
            m_connected = false;
            return false;
        }

        return true;
    }
    catch (const std::exception& e) {
        util::logging::print("Exception in Send: {}", e.what());
        m_connected = false;
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
    
    // 保存 socket 用于清理（避免在 closesocket 后使用）
    SOCKET socketForCleanup = m_socket;

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

    util::logging::print("Receive thread exiting for socket {}", socketForCleanup);
    
    // 不在接收线程中调用 Stop()，避免自我 join 死锁
    // 只设置断开标志并关闭 socket
    if (m_connected.exchange(false)) {
        closesocket(m_socket);
        m_socket = INVALID_SOCKET;
        util::logging::print("Client disconnected, notifying server to remove");
    }
    
    // 通知服务器移除此客户端（使用保存的 SOCKET 作为标识）
    m_server->RemoveClientBySocket(socketForCleanup);
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

        util::logging::print("Processing command: {} (id={})", method, id);

        // 处理命令
        Json::Value result = m_server->HandleCommand(method, params);
        
        util::logging::print("Command {} executed, preparing response", method);

        // 构建响应
        Json::Value response;
        response["id"] = id;
        
        // 只有当 result 是对象且包含 "error" 字段时，才认为是错误响应
        if (result.isObject() && result.isMember("error")) {
            response["error"] = result["error"];
            response["result"] = Json::Value::null;
        } else {
            // 正常响应（可能是对象、数组或其他类型）
            response["result"] = result;
            response["error"] = Json::Value::null;
        }

        // 发送响应
        Json::StreamWriterBuilder writerBuilder;
        writerBuilder["indentation"] = "";
        writerBuilder["emitUTF8"] = true;
        std::string responseStr = Json::writeString(writerBuilder, response);
        
        util::logging::wPrint(L"Sending response: {}", util::utf8ToUtf16(responseStr.c_str()));
        
        bool sendResult = Send(responseStr);
        util::logging::print("Response sent: {}", sendResult ? "success" : "failed");
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
        ClientConnection* clientPtr = client.get(); // 保存裸指针
        client->Start();

        {
            std::lock_guard<std::mutex> lock(m_clientsMutex);
            m_clients.push_back(std::move(client));
        }

        util::logging::print("New client connected, total clients: {}", m_clients.size());
        
        // 推送 GetUserInfo 数据给新连接的客户端
        PushUserInfoToClient(clientPtr);
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
    
    util::logging::print("HandleCommand called for method: {}", method);
    util::logging::print("Registered handlers count: {}", m_handlers.size());
    
    auto it = m_handlers.find(method);
    if (it == m_handlers.end()) {
        Json::Value error;
        error["error"] = std::format("Unknown method: {}", method);
        util::logging::print("Unknown method: {}", method);
        
        // 打印所有已注册的方法
        util::logging::print("Available methods:");
        for (const auto& pair : m_handlers) {
            util::logging::print("  - {}", pair.first);
        }
        
        return error;
    }

    util::logging::print("Found handler for method: {}", method);
    
    try {
        Json::Value result = it->second(params);
        util::logging::print("Handler executed successfully for: {}", method);
        return result;
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
    if (!client) return;
    
    SOCKET socket = client->GetSocket();
    RemoveClientBySocket(socket);
}

void SocketServer::RemoveClientBySocket(SOCKET socket)
{
    util::logging::print("Removing client by socket {} asynchronously", socket);
    
    // 异步删除客户端，避免在接收线程中析构导致死锁
    // 使用 SOCKET 而不是指针，避免 use-after-free
    std::thread([this, socket]() {
        // 给接收线程一点时间完全退出
        std::this_thread::sleep_for(std::chrono::milliseconds(200));
        
        std::lock_guard<std::mutex> lock(m_clientsMutex);
        auto it = std::remove_if(m_clients.begin(), m_clients.end(),
            [socket](const std::unique_ptr<ClientConnection>& c) {
                return c->GetSocket() == socket;
            });
        
        if (it != m_clients.end()) {
            util::logging::print("Removing client with socket {}", socket);
            m_clients.erase(it, m_clients.end());
            util::logging::print("Client removed, remaining clients: {}", m_clients.size());
        } else {
            util::logging::print("Client with socket {} already removed", socket);
        }
    }).detach();
}

void SocketServer::PushUserInfoToClient(ClientConnection* client)
{
    try {
        util::logging::print("Pushing UserInfo to new client...");
        
        // 调用 GetUserInfo 处理器获取用户信息
        Json::Value emptyParams;
        Json::Value result = HandleCommand("GetUserInfo", emptyParams);
        
        // 检查 wxid 是否为空
        if (result.isMember("wxid") && result["wxid"].isString())
        {
            std::string wxid = result["wxid"].asString();
            if (wxid.empty())
            {
                util::logging::print("UserInfo wxid is empty, skip pushing");
                return;
            }
            
            util::logging::print("Pushing UserInfo with wxid: {}", wxid);
            
            // 构建推送消息
            Json::Value message;
            message["method"] = "OnLogin"; // 使用 OnLogin 事件通知客户端
            message["params"] = result;
            
            Json::StreamWriterBuilder writerBuilder;
            writerBuilder["indentation"] = "";
            writerBuilder["emitUTF8"] = true;
            std::string messageStr = Json::writeString(writerBuilder, message);
            
            // 发送给客户端
            if (client->IsConnected()) {
                bool sendResult = client->Send(messageStr);
                util::logging::print("UserInfo pushed: {}", sendResult ? "success" : "failed");
            }
        }
        else
        {
            util::logging::print("UserInfo result is invalid or wxid is missing");
        }
    }
    catch (const std::exception& e) {
        util::logging::print("Exception in PushUserInfoToClient: {}", e.what());
    }
}

} // namespace Socket
} // namespace WeixinX

