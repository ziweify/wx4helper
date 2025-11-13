#include "SocketCommands.h"
#include "Features.h"
#include "util.h"
#include <thread>
#include <chrono>

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
	// 🔥 参数可选：如果提供了 groupId，过滤指定群的成员
	std::string groupId = "";
	
	// 🔥 调试：输出接收到的参数信息（转换枚举为整数）
	util::logging::print("HandleGetGroupContacts: Received params type={}, size={}", 
		static_cast<int>(params.type()), static_cast<int>(params.size()));
	
	if (!params.empty() && params[0].isString()) {
		groupId = params[0].asString();
		util::logging::print("HandleGetGroupContacts: Filtering for group: [{}]", 
			groupId.c_str());
	} else {
		util::logging::print("HandleGetGroupContacts: No groupId parameter - querying ALL chatroom members");
		
		// 🔥 如果参数格式不对，输出详细信息
		if (!params.empty()) {
			util::logging::print("HandleGetGroupContacts: First param type={}, value={}", 
				static_cast<int>(params[0].type()), params[0].toStyledString().c_str());
		}
	}
    
    try {
        // 获取 Core 单例实例
        auto& core = util::Singleton<Core>::Get();
        
        // 🔥 调用 GetGroupContacts 查询数据库（传递 groupId，如果为空则查询所有）
        util::logging::print("HandleGetGroupContacts: Calling GetGroupContacts with wxid=[{}]", 
            groupId.c_str());
        std::string jsonString = core.GetGroupContacts(groupId);
        
        // 解析 JSON 字符串为 Json::Value
        Json::Value result;
        JSONCPP_STRING err;
        Json::CharReaderBuilder builder;
        const std::unique_ptr<Json::CharReader> reader(builder.newCharReader());
        
        if (reader->parse(jsonString.c_str(), jsonString.c_str() + jsonString.length(), &result, &err))
        {
            util::logging::print("GetGroupContacts: Successfully parsed {} members", 
                result.isArray() ? result.size() : 0);
            return result;
        }
        else
        {
            util::logging::print("GetGroupContacts: Failed to parse JSON: {}", err);
            Json::Value error;
            error["error"] = std::format("Failed to parse group contacts JSON: {}", err);
            return error;
        }
    }
    catch (const std::exception& e) {
        util::logging::print("GetGroupContacts: Exception: {}", e.what());
        Json::Value error;
        error["error"] = std::format("Failed to get group contacts: {}", e.what());
        return error;
    }
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
    
    // 检查用户是否在线
    bool online = *reinterpret_cast<bool*>(util::getWeixinDllBase() + weixin_dll::v41021::offset::is_online);
    
    // 如果用户在线但数据为空，重新读取
    if (online && Core::currentUserInfo.wxid.empty()) {
        util::logging::print("User is online but currentUserInfo is empty, re-reading user info...");
        
        // 获取 Core 单例并重新读取用户信息
        auto& core = util::Singleton<Core>::Get();
        Core::currentUserInfo.read(&core);
        
        // 等待一小段时间让数据读取完成
        std::this_thread::sleep_for(std::chrono::milliseconds(500));
        
        util::logging::print("User info re-read completed. wxid: {}, nickname: {}", 
                           Core::currentUserInfo.wxid.c_str(), 
                           Core::currentUserInfo.nickname.c_str());
    }
    
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

