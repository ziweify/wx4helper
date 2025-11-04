#pragma once
#include "SocketServer.h"
// json.h 已经在 SocketServer.h 中包含了

namespace WeixinX {
namespace Socket {

/**
 * Socket 命令处理器
 */
class SocketCommands {
public:
    static void RegisterAll(SocketServer* server);

private:
    // 命令处理函数
    static Json::Value HandleGetContacts(const Json::Value& params);
    static Json::Value HandleGetGroupContacts(const Json::Value& params);
    static Json::Value HandleSendMessage(const Json::Value& params);
    static Json::Value HandleGetUserInfo(const Json::Value& params);
};

} // namespace Socket
} // namespace WeixinX

