#pragma once

// 必须在 windows.h 之前定义，防止包含 winsock.h
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

// 必须在任何 Windows 头文件之前包含 WinSock2
#include <WinSock2.h>
#include <WS2tcpip.h>
#include <Windows.h>

#include <string>
#include <vector>
#include <map>
#include <functional>
#include <thread>
#include <mutex>
#include <atomic>
#include <queue>
#include "3rd/include/json/json.h"

#pragma comment(lib, "ws2_32.lib")

namespace WeixinX {
namespace Socket {

// 前置声明
class SocketServer;
class ClientConnection;

// 命令处理器类型
using CommandHandler = std::function<Json::Value(const Json::Value& params)>;

/**
 * 客户端连接管理
 */
class ClientConnection {
public:
    ClientConnection(SOCKET socket, SocketServer* server);
    ~ClientConnection();

    void Start();
    void Stop();
    bool Send(const std::string& message);
    bool IsConnected() const { return m_connected; }
    SOCKET GetSocket() const { return m_socket; }

private:
    void ReceiveThread();
    bool ReceiveExact(char* buffer, int length);
    void ProcessMessage(const std::string& message);

    SOCKET m_socket;
    SocketServer* m_server;
    std::atomic<bool> m_connected;
    std::thread m_receiveThread;
};

/**
 * Socket 服务端
 */
class SocketServer {
public:
    SocketServer(int port = 6328);
    ~SocketServer();

    // 启动/停止服务器
    bool Start();
    void Stop();
    bool IsRunning() const { return m_running; }

    // 注册命令处理器
    void RegisterHandler(const std::string& method, CommandHandler handler);

    // 处理命令（由 ClientConnection 调用）
    Json::Value HandleCommand(const std::string& method, const Json::Value& params);

    // 广播消息到所有客户端
    void Broadcast(const std::string& method, const Json::Value& params);

    // 移除客户端连接
    void RemoveClient(ClientConnection* client);

private:
    void AcceptThread();

    int m_port;
    SOCKET m_listenSocket;
    std::atomic<bool> m_running;
    std::thread m_acceptThread;
    
    std::vector<std::unique_ptr<ClientConnection>> m_clients;
    std::mutex m_clientsMutex;
    
    std::map<std::string, CommandHandler> m_handlers;
    std::mutex m_handlersMutex;
};

} // namespace Socket
} // namespace WeixinX

