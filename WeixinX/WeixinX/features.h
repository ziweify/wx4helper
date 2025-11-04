#pragma once

#include <concurrent_unordered_map.h>
#include <concurrent_queue.h>
#include "util.h"
#include "weixin_dll.h"
#include "SocketServer.h"


namespace WeixinX {

	namespace Features {
		static concurrency::concurrent_unordered_map<std::string, uintptr_t>  DBHandles = concurrency::concurrent_unordered_map<std::string, uintptr_t>();


	}


	namespace Detour
	{
		inline util::DetourHook OpenDatabase;
		inline util::DetourHook AddMsgListToDb;

		int _fastcall hkOpenDatabase(const char* dbName, uintptr_t** ppHandle, unsigned int flags, const char* zVfs);
		__int64 _fastcall hkAddMsgListToDb(__int64 rcx, __int64 rdx, uintptr_t r8);
	}



	class Core;
	struct CurrentUserInfo
	{

		std::atomic_bool online;
		std::string wxid;
		std::string alias;
		std::string nick;
		
		// Socket 通信需要的额外字段
		std::string nickname;    // 昵称
		std::string account;     // 账号
		std::string mobile;      // 手机号
		std::string avatar;      // 头像
		std::string dataPath;    // 数据路径
		std::string currentDataPath;  // 当前数据路径
		std::string dbKey;       // 数据库密钥

		static constexpr uintptr_t offset_wxid = 0x48;
		static constexpr uintptr_t offset_alias = offset_wxid + 0x20;
		static constexpr uintptr_t offset_nick = offset_wxid + 0x40;

		void read(WeixinX::Core* core);
		void clear();

		std::mutex currentUserInfoMutex;

	};

	class MsgReceived {
	public:
		std::string receiver1;
		std::string receiver2;
		std::string sender;
		std::string content;
		std::wstring wcontent;
		std::string refermsg;
		std::wstring wrefermsg;
		uint64_t ts;
		bool fromChatroom;

		static void Received(weixin_dll::v41021::weixin_struct::MsgReceived* msg);
		static concurrency::concurrent_queue<MsgReceived> msgReceived_queue;

	};


	class Core {
	public:
		void Run();

		bool Hook();

		void OnLogin();
		void OnLogout();


		void SendText(string who, string what);
		void SendImage(string who, string which);

		string GetNameByWxid(string wxid);
		void Notify(string notification);
		void SendNotification(auto j);

		// Socket 服务器
		Socket::SocketServer* GetSocketServer() { return m_socketServer.get(); }
		void InitializeSocketServer();

		// 全局静态成员
		static CurrentUserInfo currentUserInfo;

	private:
		std::unique_ptr<Socket::SocketServer> m_socketServer;
	};
}
