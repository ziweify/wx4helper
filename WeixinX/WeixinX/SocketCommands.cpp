#include "SocketCommands.h"
#include "Features.h"
#include "util.h"

namespace WeixinX {
namespace Socket {

void SocketCommands::RegisterAll(SocketServer* server)
{
    server->RegisterHandler("GetContacts", HandleGetContacts);
    server->RegisterHandler("GetGroupContacts", HandleGetGroupContacts);
    server->RegisterHandler("SendMessage", HandleSendMessage);
    server->RegisterHandler("SendImage", HandleSendImage);
    server->RegisterHandler("GetUserInfo", HandleGetUserInfo);
    
    util::logging::print("All socket commands registered");
}

Json::Value SocketCommands::HandleGetContacts(const Json::Value& params)
{
    util::logging::print("Handling GetContacts - querying real database");
    
    try {
        // 获取 Core 单例实例
        auto& core = util::Singleton<Core>::Get();
        
        // 调用 GetContacts 查询数据库，返回 JSON 字符串
        std::string jsonString = core.GetContacts();
        
        // 解析 JSON 字符串为 Json::Value
        Json::Value result;
        JSONCPP_STRING err;
        Json::CharReaderBuilder builder;
        const std::unique_ptr<Json::CharReader> reader(builder.newCharReader());
        
        if (reader->parse(jsonString.c_str(), jsonString.c_str() + jsonString.length(), &result, &err))
        {
            util::logging::print("GetContacts: Successfully parsed {} contacts", 
                result.isArray() ? result.size() : 0);
            return result;
        }
        else
        {
            util::logging::print("GetContacts: Failed to parse JSON: {}", err);
            Json::Value error;
            error["error"] = std::format("Failed to parse contacts JSON: {}", err);
            return error;
        }
    }
    catch (const std::exception& e) {
        util::logging::print("GetContacts: Exception: {}", e.what());
        Json::Value error;
        error["error"] = std::format("Failed to get contacts: {}", e.what());
        return error;
    }
}

Json::Value SocketCommands::HandleGetGroupContacts(const Json::Value& params)
{
    if (params.empty() || !params[0].isString()) {
        Json::Value error;
        error["error"] = "Missing or invalid parameter: groupId (string)";
        return error;
    }
    
    std::string groupId = params[0].asString();
    util::logging::wPrint(L"Handling GetGroupContacts for group: {}", 
        util::utf8ToUtf16(groupId.c_str()));
    
    Json::Value result(Json::arrayValue);
    
    // TODO: 从微信获取群成员列表
    // 这里返回示例数据
    Json::Value member;
    member["wxid"] = "wxid_member123";
    member["nickname"] = "群成员";
    member["displayName"] = "群昵称";
    
    result.append(member);
    
    return result;
}

Json::Value SocketCommands::HandleSendMessage(const Json::Value& params)
{
    if (params.size() < 2 || !params[0].isString() || !params[1].isString()) {
        Json::Value error;
        error["error"] = "Invalid parameters. Expected: (wxid: string, message: string)";
        return error;
    }
    
    std::string wxid = params[0].asString();
    std::string message = params[1].asString();
    
    util::logging::wPrint(L"Handling SendMessage to {}: {}", 
        util::utf8ToUtf16(wxid.c_str()),
        util::utf8ToUtf16(message.c_str()));
    
    // 获取 Core 单例实例并发送消息
    try {
        auto& core = util::Singleton<Core>::Get();
        core.SendText(wxid, message);
        
        Json::Value result;
        result["success"] = true;
        result["messageId"] = "msg_" + std::to_string(util::Timestamp());
        
        util::logging::print("Message sent successfully");
        return result;
    }
    catch (const std::exception& e) {
        Json::Value error;
        error["error"] = std::format("Failed to send message: {}", e.what());
        util::logging::print("Failed to send message: {}", e.what());
        return error;
    }
}


Json::Value SocketCommands::HandleSendImage(const Json::Value& params)
{
    if (params.size() < 2 || !params[0].isString() || !params[1].isString()) {
        Json::Value error;
        error["error"] = "Invalid parameters. Expected: (wxid: string, imagepath: string)";
        return error;
    }

    std::string wxid = params[0].asString();
    std::string message = params[1].asString();

    util::logging::wPrint(L"Handling SendMessage to {}: {}",
        util::utf8ToUtf16(wxid.c_str()),
        util::utf8ToUtf16(message.c_str()));

    // 获取 Core 单例实例并发送消息
    try {
        auto& core = util::Singleton<Core>::Get();
        core.SendImage(wxid, message);

        Json::Value result;
        result["success"] = true;
        result["messageId"] = "msg_" + std::to_string(util::Timestamp());

        util::logging::print("Message sent successfully");
        return result;
    }
    catch (const std::exception& e) {
        Json::Value error;
        error["error"] = std::format("Failed to send message: {}", e.what());
        util::logging::print("Failed to send message: {}", e.what());
        return error;
    }
}

Json::Value SocketCommands::HandleGetUserInfo(const Json::Value& params)
{
    util::logging::print("Handling GetUserInfo");
    
    Json::Value result;
    
    // 返回当前登录用户信息
    result["wxid"] = Core::currentUserInfo.wxid;
    result["nickname"] = Core::currentUserInfo.nickname;
    result["account"] = Core::currentUserInfo.account;
    result["mobile"] = Core::currentUserInfo.mobile;
    result["avatar"] = Core::currentUserInfo.avatar;
    result["dataPath"] = Core::currentUserInfo.dataPath;
    result["currentDataPath"] = Core::currentUserInfo.currentDataPath;
    result["dbKey"] = Core::currentUserInfo.dbKey;
    
    return result;
}

} // namespace Socket
} // namespace WeixinX

