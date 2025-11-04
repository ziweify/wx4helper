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
    server->RegisterHandler("GetUserInfo", HandleGetUserInfo);
    
    util::logging::print("All socket commands registered");
}

Json::Value SocketCommands::HandleGetContacts(const Json::Value& params)
{
    util::logging::print("Handling GetContacts");
    
    Json::Value result(Json::arrayValue);
    
    // TODO: 从微信获取联系人列表
    // 这里返回示例数据
    Json::Value contact;
    contact["wxid"] = "wxid_example123";
    contact["nickname"] = "示例联系人";
    contact["remark"] = "备注名";
    contact["avatar"] = "http://example.com/avatar.jpg";
    
    result.append(contact);
    
    return result;
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
    
    // TODO: 实现发送消息功能
    
    Json::Value result;
    result["success"] = true;
    result["messageId"] = "msg_" + std::to_string(util::Timestamp());
    
    return result;
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

