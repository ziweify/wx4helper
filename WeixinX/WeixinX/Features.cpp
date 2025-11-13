#include<thread>
#include <emmintrin.h>

#include "features.h"
#include "MQ.h"
#include "ii3image.h"
#include "base64.h"
#include "3rd/include/json/json.h"
#include "SocketServer.h"
#include "SocketCommands.h"

//static concurrency::concurrent_queue<string>  test_queue = concurrency::concurrent_queue<string>();

// 静态成员定义
WeixinX::CurrentUserInfo WeixinX::Core::currentUserInfo;

void WeixinX::CurrentUserInfo::read(WeixinX::Core* core) {


	std::thread t([&]() {


		std::lock_guard<std::mutex> l(currentUserInfoMutex);



		while (true) {

			uintptr_t base = *(uint64_t*)(util::getWeixinDllBase() + weixin_dll::v41021::offset::current_user_info);
			uintptr_t  currentUserInfo = base + 0x68;

			util::logging::print("currentUserInfo = {:#0X} ", *(__int64*)currentUserInfo);


			if (!online.load()) {
				break;
			}
			if (currentUserInfo != 0 && currentUserInfo < util::getWeixinDllBase()) {


				weixin_dll::v41021::weixin_struct::WeixinString str;
				memcpy(&str, (void*)(*(__int64*)currentUserInfo + CurrentUserInfo::offset_wxid), 32);
				wxid = str.str();
				util::logging::print("wxid: {}", wxid.c_str());

				memcpy(&str, (void*)(*(__int64*)currentUserInfo + CurrentUserInfo::offset_alias), 32);
				alias = str.str();
				util::logging::print("Alias: {}", alias.c_str());

			memcpy(&str, (void*)(*(__int64*)currentUserInfo + CurrentUserInfo::offset_nick), 32);
			nick = str.str();
			nickname = nick;  // ✅ 同时赋值给 nickname（用于 Socket 通信）
			util::logging::wPrint(L"Nick: {}", util::utf8ToUtf16(nick.c_str()).c_str());

			core->Notify("/online");
			break;
			}


			std::this_thread::sleep_for(std::chrono::milliseconds(1000));

		}


		});

	t.detach();


}


void WeixinX::CurrentUserInfo::clear() {

	std::lock_guard<std::mutex> l(currentUserInfoMutex);

	// 清空原始字段
	wxid.clear();
	alias.clear();
	nick.clear();
	
	// 清空 Socket 通信字段
	nickname.clear();
	account.clear();
	mobile.clear();
	avatar.clear();
	dataPath.clear();
	currentDataPath.clear();
	dbKey.clear();
}



void WeixinX::Core::Run() {

	Hook();

	static unsigned long long  ticks = 0;
	while (1)
	{


		this_thread::sleep_for(chrono::milliseconds(50));
		bool online = *reinterpret_cast<bool*>(util::getWeixinDllBase() + weixin_dll::v41021::offset::is_online);

		if (online != currentUserInfo.online.load()) {

			if (online) {

				OnLogin();

			}
			else {
				OnLogout();
			}

			currentUserInfo.online.store(online);
		}

		WeixinX::MsgReceived msg;
		while (WeixinX::MsgReceived::msgReceived_queue.try_pop(msg)) {

			if (currentUserInfo.wxid.length() < 1) {
				continue;
			}

			Json::Value j;
			j["source"] = "wechat";
			j["robot"] = currentUserInfo.wxid;

			string b64;
			string content = msg.content;
			if (content.starts_with("")) {

			}

			base64::encode(content.c_str(), content.length(), &b64);
			j["content"] = b64;

			b64.clear();
			string chatroom = GetNameByWxid(msg.receiver1);
			base64::encode(chatroom.c_str(), chatroom.length(), &b64);

			j["group"] = msg.receiver1;
			j["group_name"] = b64;

			b64.clear();
			string nick = GetNameByWxid(msg.sender);
			base64::encode(nick.c_str(), nick.length(), &b64);
			j["sender"] = msg.sender;
			j["sender_name"] = b64;


			Json::StreamWriterBuilder builder;
			const std::string payload = Json::writeString(builder, j);

			MQ::out_queue.push(payload);

			util::logging::wPrint(L"MsgReceived:  {:d}\nreceiver1 = {} \nreceiver2 = {} \nsender = {} \ncontent:\n{} \nrefermsg:\n{} \n----------------",
				msg.ts,
				util::utf8ToUtf16(msg.receiver1.c_str()),
				util::utf8ToUtf16(msg.receiver2.c_str()),
				util::utf8ToUtf16(msg.sender.c_str()),
				util::utf8ToUtf16(msg.content.c_str()),
				util::utf8ToUtf16(msg.refermsg.c_str())
			);
		}


		string notification;
		while (MQ::in_queue.try_pop(notification)) {

			Json::Value j;
			JSONCPP_STRING err;
			Json::CharReaderBuilder builder;
			const std::unique_ptr<Json::CharReader> reader(builder.newCharReader());
			if (!reader->parse(notification.c_str(), notification.c_str() + notification.length(), &j, &err)) {

				WeixinX::util::logging::print("json parse: [err {}]", err);

			}
			else {

				if (j["robot"] != NULL) {

					string robot = j["robot"].asString().c_str();
					string receiver = j["receiver"].asString().c_str();
					string content;
					base64::decode(j["text"].asString().c_str(), j["text"].asString().length(), &content);
					util::logging::wPrint(L"robot = {}\n receiver = {}\n content:\n{}", util::utf8ToUtf16(robot.c_str()), util::utf8ToUtf16(receiver.c_str()), util::utf8ToUtf16(content.c_str()));
					if (robot == currentUserInfo.wxid) {

						SendNotification(j);

					}

				}


			}


		}

		ticks++;

		if (ticks % 6000 == 0) {

			ii3images::remove_stale_files();
		}

		if (ticks % 1200 == 0) {

			if (currentUserInfo.online.load()) {

				Notify("/heartbeat");

			}
		}


	}

}

bool WeixinX::Core::Hook() {

	if (MH_Initialize() != MH_OK)
		throw std::runtime_error("failed initialize minhook");


	uintptr_t openDatabaseAddr = util::getWeixinDllBase() + weixin_dll::v41021::offset::db::open_database;
	util::logging::print("Hooking OpenDatabase at address: 0x{:X} (Base: 0x{:X} + Offset: 0x{:X})", 
		openDatabaseAddr, 
		util::getWeixinDllBase(), 
		weixin_dll::v41021::offset::db::open_database);
	if (!Detour::OpenDatabase.Create((void*)openDatabaseAddr, &Detour::hkOpenDatabase))
		return false;


	uintptr_t addMsgListToDbAddr = util::getWeixinDllBase() + weixin_dll::v41021::offset::db::add_msg_list_to_db;
	util::logging::print("Hooking AddMsgListToDb at address: 0x{:X} (Base: 0x{:X} + Offset: 0x{:X})", 
		addMsgListToDbAddr, 
		util::getWeixinDllBase(), 
		weixin_dll::v41021::offset::db::add_msg_list_to_db);
	if (!Detour::AddMsgListToDb.Create((void*)addMsgListToDbAddr, &Detour::hkAddMsgListToDb))
		return false;

	return true;

}

void WeixinX::Core::OnLogin() {

	util::logging::print("current user logged in");

	currentUserInfo.read(this);

	//std::thread t([&]() {


	//	while (true) {
	//		string filePath;
	//		if (test_queue.try_pop(filePath) && currentUserInfo.online.load()) {

	//			util::logging::print(filePath);
	//			SendText("filehelper", filePath);


	//		}
	//	}

	//	//SEH_START

	//	this_thread::sleep_for(chrono::milliseconds(1000));
	//	//SEH_END
	//	});
	//t.detach();

}


void WeixinX::Core::OnLogout() {

	Notify("/offline");

	util::logging::print("current user logged out");
	currentUserInfo.clear();
	WeixinX::Features::DBHandles.clear();


}

void WeixinX::Core::Notify(string notification) {

	Json::Value j;
	j["source"] = "wechat";
	j["robot"] = currentUserInfo.wxid;

	string b64;
	string nick = currentUserInfo.nick;
	base64::encode(nick.c_str(), nick.length(), &b64);
	j["sender_name"] = b64;

	b64.clear();
	string hb = notification;
	base64::encode(hb.c_str(), hb.length(), &b64);
	j["content"] = b64;

	Json::StreamWriterBuilder builder;
	const std::string payload = Json::writeString(builder, j);
	MQ::out_queue.push(payload);

}

void WeixinX::Core::SendNotification(auto j)
{

	string wxid = j["receiver"].asString();
	string plainText;
	base64::decode(j["text"].asString().c_str(), j["text"].asString().length(), &plainText);
	string msg = plainText;
	if (j["type"] == "text") {

		SendText(wxid, msg);

	}

	if (j["type"] == "image") {

		std::string extra = j["extra"].asString();
		if (extra.length() > 0) {

			string decoded;
			base64::decode(extra.c_str(), extra.length(), &decoded);

			ii3images::ensure_image_directory_exists();
			wstring ii3 = ii3images::save_ii3_image(decoded.c_str(), decoded.length());
			string img = util::utf16ToUtf8(ii3.c_str());
			SendText(wxid, msg);
			SendImage(wxid, img);
			 
		}

	}

}

typedef __int64(*WeixinCall)(...);

void buildTextMessage(uint64_t* ptr, const std::string& what, const std::string& who) {

	uint64_t base = WeixinX::util::getWeixinDllBase();

	ptr[1] = 0x100000004LL;
	*ptr = base + WeixinX::weixin_dll::v41021::offset::message::txt_message_vtable;

	WeixinCall Ctr = (WeixinCall)(base + WeixinX::weixin_dll::v41021::offset::message::txt_message_ctr);
	WeixinX::weixin_dll::v41021::weixin_struct::TextMessage* msgToSend = reinterpret_cast<WeixinX::weixin_dll::v41021::weixin_struct::TextMessage*>(ptr + 2);
	Ctr(reinterpret_cast<uint64_t>(msgToSend));

	msgToSend->receiver = who;
	msgToSend->content = what;
	msgToSend->msg_len = what.length();
	msgToSend->type = 1;

}


void buildSendMessageArg2(uint64_t* a1) {



	uint64_t base = WeixinX::util::getWeixinDllBase();

	WeixinCall createParam = (WeixinCall)(base + WeixinX::weixin_dll::v41021::offset::message::create_param2);
	uint64_t a2 = base + WeixinX::weixin_dll::v41021::offset::message::param2_1;
	uint64_t a3 = base + WeixinX::weixin_dll::v41021::offset::message::param2_2;
	uint64_t a4 = base + WeixinX::weixin_dll::v41021::offset::message::param2_3;
	uint64_t arg2 = base + WeixinX::weixin_dll::v41021::offset::message::param2;

	uint64_t* buf = WeixinX::util::heapAlloc<uint64_t>(16);
	WeixinX::weixin_dll::v41021::weixin_struct::UnknownBlock* p2 = (WeixinX::weixin_dll::v41021::weixin_struct::UnknownBlock*)WeixinX::util::heapAlloc<uint64_t>(64);
	WeixinX::weixin_dll::v41021::weixin_struct::UnknownBlock* p3 = (WeixinX::weixin_dll::v41021::weixin_struct::UnknownBlock*)WeixinX::util::heapAlloc<uint64_t>(64);
	WeixinX::weixin_dll::v41021::weixin_struct::UnknownBlock* p4 = (WeixinX::weixin_dll::v41021::weixin_struct::UnknownBlock*)WeixinX::util::heapAlloc<uint64_t>(64);

	memset(p2, 0, sizeof(WeixinX::weixin_dll::v41021::weixin_struct::UnknownBlock));
	memset(p3, 0, sizeof(WeixinX::weixin_dll::v41021::weixin_struct::UnknownBlock));
	memset(p4, 0, sizeof(WeixinX::weixin_dll::v41021::weixin_struct::UnknownBlock));

	p2->vtable = a2;
	p2->self = (uint64_t)p2;
	p3->vtable = a3;
	p3->self = (uint64_t)p3;
	p4->vtable = a4;
	p4->self = (uint64_t)p4;

	createParam(reinterpret_cast<uint64_t>(a1), reinterpret_cast<uint64_t>(p2), reinterpret_cast<uint64_t>(p3), reinterpret_cast<uint64_t>(p4), reinterpret_cast<uint64_t>(buf), *(uint64_t*)arg2);

}
//这是发送文本消息， 接收者同发送图片消息，what是内容，就是字符串而已
void WeixinX::Core::SendText(string who, string what) {


	uint64_t base = WeixinX::util::getWeixinDllBase();

	uint64_t* txtMessage = WeixinX::util::heapAlloc<uint64_t>(0x530);
	buildTextMessage(txtMessage, what, who);

	uint64_t* data = WeixinX::util::heapAlloc<uint64_t>(0x20);
	data[0] = reinterpret_cast<uint64_t>(txtMessage + 2);
	data[1] = reinterpret_cast<uint64_t>(txtMessage);
	data[2] = 0;
	uint64_t* arg1 = WeixinX::util::heapAlloc<uint64_t>(0x28);
	arg1[0] = base + WeixinX::weixin_dll::v41021::offset::message::param1_vtable;
	arg1[1] = reinterpret_cast<uint64_t>(data);
	arg1[2] = reinterpret_cast<uint64_t>(data) + 0x10;
	arg1[3] = reinterpret_cast<uint64_t>(data) + 0x10;
	arg1[4] = 1;

	uint64_t* arg2 = WeixinX::util::heapAlloc<uint64_t>(0xE8);
	buildSendMessageArg2(arg2);

	WeixinCall send = (WeixinCall)(base + WeixinX::weixin_dll::v41021::offset::message::send_message);
	send(reinterpret_cast<uint64_t>(arg1), reinterpret_cast<uint64_t>(arg2));

}

void buildImageMessage(uint64_t* ptr, const std::string& which, const std::string& who) {



	uint64_t base = WeixinX::util::getWeixinDllBase();

	ptr[1] = 0x100000005LL;
	*ptr = base + WeixinX::weixin_dll::v41021::offset::message::img_message_vtable;
	WeixinCall Ctr = (WeixinCall)(base + WeixinX::weixin_dll::v41021::offset::message::img_message_ctr);

	WeixinX::weixin_dll::v41021::weixin_struct::ImageMessage* msgToSend = reinterpret_cast<WeixinX::weixin_dll::v41021::weixin_struct::ImageMessage*>(ptr + 2);
	Ctr(reinterpret_cast<uint64_t>(msgToSend));


	__m128i path;
	path.m128i_i64[0] = (uint64_t)which.c_str();
	path.m128i_i64[1] = which.length();
	WeixinX::weixin_dll::v41021::weixin_struct::WeixinWideString wPath;
	WeixinCall Utf8ToWString = (WeixinCall)(base + WeixinX::weixin_dll::v41021::offset::message::utf8_to_wstring);
	Utf8ToWString(&wPath, path);

	msgToSend->receiver = who;
	memcpy((void*)&msgToSend->path, (void*)&wPath, 0x20);
	msgToSend->type = 3;
	msgToSend->unknown1 = 1;
	auto size = WeixinX::util::tool::FileSize(which);
	msgToSend->size = size;


}
//这是发送图片消息，who是 发送给谁，群用群id，个人用wxid， which是图片路径
void WeixinX::Core::SendImage(string who, string which) {

	// 检查图片文件是否存在
	DWORD fileAttr = GetFileAttributesA(which.c_str());
	if (fileAttr == INVALID_FILE_ATTRIBUTES)
	{
		util::logging::print("SendImage: File not found: {}", which);
		throw std::runtime_error(std::format("Image file not found: {}", which));
	}
	
	// 检查是否是目录
	if (fileAttr & FILE_ATTRIBUTE_DIRECTORY)
	{
		util::logging::print("SendImage: Path is a directory, not a file: {}", which);
		throw std::runtime_error(std::format("Path is a directory, not a file: {}", which));
	}
	
	// 检查文件扩展名（可选，确保是图片格式）
	std::string lowerPath = which;
	std::transform(lowerPath.begin(), lowerPath.end(), lowerPath.begin(), ::tolower);
	
	bool isValidImageExt = 
		lowerPath.ends_with(".jpg") || 
		lowerPath.ends_with(".jpeg") || 
		lowerPath.ends_with(".png") || 
		lowerPath.ends_with(".gif") || 
		lowerPath.ends_with(".bmp") ||
		lowerPath.ends_with(".webp");  // 添加 webp 支持
	
	if (!isValidImageExt)
	{
		util::logging::print("SendImage: Invalid image format: {}", which);
		throw std::runtime_error(std::format("Invalid image format (must be jpg/jpeg/png/gif/bmp/webp): {}", which));
	}
	
	util::logging::print("SendImage: File validated successfully: {}", which);

	uint64_t base = WeixinX::util::getWeixinDllBase();

	uint64_t* imageMessage = WeixinX::util::heapAlloc<uint64_t>(0x580);
	buildImageMessage(imageMessage, which, who);

	uint64_t* data = WeixinX::util::heapAlloc<uint64_t>(0x20);
	data[0] = reinterpret_cast<uint64_t>(imageMessage + 2);
	data[1] = reinterpret_cast<uint64_t>(imageMessage);
	data[2] = 0;
	uint64_t* arg1 = WeixinX::util::heapAlloc<uint64_t>(0x28);
	arg1[0] = base + WeixinX::weixin_dll::v41021::offset::message::param1_vtable;
	arg1[1] = reinterpret_cast<uint64_t>(data);
	arg1[2] = reinterpret_cast<uint64_t>(data) + 0x10;
	arg1[3] = reinterpret_cast<uint64_t>(data) + 0x10;
	arg1[4] = 1;

	uint64_t* arg2 = WeixinX::util::heapAlloc<uint64_t>(0xE8);
	buildSendMessageArg2(arg2);

	WeixinCall send = (WeixinCall)(base + WeixinX::weixin_dll::v41021::offset::message::send_message);
	send(reinterpret_cast<uint64_t>(arg1), reinterpret_cast<uint64_t>(arg2));
	
	util::logging::print("SendImage: Image sent successfully to {}", who);

}

//这就是一个使用数据库句柄进行查询的例子，根据wxid查询用户昵称。联系人信息都保存在contant.db这个sqlite库里，所以需要contact.db这个库的句柄。这些句柄都保存在DBHandles这个字典里
// 用sql语句都能查询了，看代码，这是一个标准的用get_table进行查询的模板，查询完成后一定要用free_table释放资源，不然会内存泄露

// 使用这个方法，可以查询所有的数据库，前提是必须知道微信的数据库表结构，不过这些网上都有，不是个问题。
string WeixinX::Core::GetNameByWxid(string wxid)
{
	// 1. 检查数据库句柄是否存在
	if (WeixinX::Features::DBHandles.find("contact.db") == WeixinX::Features::DBHandles.end())
	{
		util::logging::print("GetNameByWxid: no handle to contact.db (not found in map)");
		return std::string();
	}
	
	// 2. 检查数据库句柄值是否为空（0）
	uintptr_t dbHandle = WeixinX::Features::DBHandles["contact.db"];
	if (dbHandle == 0)
	{
		util::logging::print("GetNameByWxid: contact.db handle is null (0), WeChat may not be logged in");
		return std::string();
	}

	std::string name{ "" };

	uintptr_t base = util::getWeixinDllBase();
	char* err = nullptr;
	char** result = nullptr;
	int row = 0, col = 0;
	int rc;
	
	//调用get_table查询
	std::string sql = std::format("select contact.nick_name from contact where username = '{}'", wxid);
	rc = util::invokeCdecl<int>((void*)(base + WeixinX::weixin_dll::v41021::offset::db::get_table),
		dbHandle,  // 使用之前检查过的 dbHandle
		sql.c_str(), &result, &row, &col, &err
		);

	if (rc == 0)
	{
		if (row > 0)
		{

			int idx = col;

			for (int x = 0; x < row; x++)
			{
				for (int y = 0; y < col; y++)
				{
					name = result[idx++];

				}

			}
		}


	}
	else
	{
		util::logging::print("GetNameByWxid: query failed, error={}", err ? err : "unknown");
	}
	
	// 释放资源（重要！）
	if (result != nullptr)
	{
		util::invokeCdecl<void>((void*)(base + WeixinX::weixin_dll::v41021::offset::db::free_table), result);
		util::logging::print("GetNameByWxid: Resources freed");
	}

	return name;

}

string WeixinX::Core::GetContacts()
{
	util::logging::print("GetContacts: Starting to query contact database");
	
	// 1. 检查数据库句柄是否存在
	if (WeixinX::Features::DBHandles.find("contact.db") == WeixinX::Features::DBHandles.end())
	{
		util::logging::print("GetContacts: no handle to contact.db (not found in map)");
		Json::Value error;
		error["error"] = "contact.db handle not found";
		Json::StreamWriterBuilder builder;
		builder["indentation"] = "";
		builder["emitUTF8"] = true;
		return Json::writeString(builder, error);
	}
	
	// 2. 检查数据库句柄值是否为空（0）
	uintptr_t dbHandle = WeixinX::Features::DBHandles["contact.db"];
	if (dbHandle == 0)
	{
		util::logging::print("GetContacts: contact.db handle is null (0), WeChat may not be logged in");
		Json::Value error;
		error["error"] = "contact.db handle is null, WeChat may not be logged in";
		Json::StreamWriterBuilder builder;
		builder["indentation"] = "";
		builder["emitUTF8"] = true;
		return Json::writeString(builder, error);
	}

	// 3. 准备查询变量
	uintptr_t base = util::getWeixinDllBase();
	char* err = nullptr;
	char** result = nullptr;
	int row = 0, col = 0;
	int rc;
	
	// 4. 构建 SQL 查询语句
	// 查询主要字段，排除 BLOB 字段和一些不重要的字段
	std::string sql = 
		"SELECT "
		"username, "           // wxid
		"nick_name, "          // 昵称
		"alias, "              // 微信号
		"remark, "             // 备注
		"small_head_url, "     // 头像
		"description, "        // 个性签名
		"verify_flag, "        // 认证标志
		"chat_room_type "      // 群聊类型（0=普通好友，1=群聊）
		"FROM contact "
		"WHERE delete_flag = 0 " // 排除已删除的联系人
		"ORDER BY username";
	
	util::logging::print("GetContacts: Executing SQL");
	
	// 5. 调用 get_table 查询
	rc = util::invokeCdecl<int>(
		(void*)(base + WeixinX::weixin_dll::v41021::offset::db::get_table),
		dbHandle,  // 使用之前检查过的 dbHandle
		sql.c_str(), 
		&result, 
		&row, 
		&col, 
		&err
	);
	
	// 6. 构建 JSON 结果
	Json::Value contacts(Json::arrayValue);
	
	if (rc == 0)
	{
		util::logging::print("GetContacts: Query successful, rows={}, cols={}", row, col);
		
		if (row > 0 && col > 0)
		{
			// idx 从 col 开始，跳过列名行
			int idx = col;
			
			// 遍历每一行数据
			for (int x = 0; x < row; x++)
			{
				Json::Value contact;
				
				// 遍历每一列
				for (int y = 0; y < col; y++)
				{
					// 获取列名（从 result 的前 col 个元素）
					const char* columnName = result[y];
					// 获取数据（可能为 NULL）
					const char* value = result[idx++];
					
					// 将数据添加到 JSON 对象
					if (value != nullptr && strlen(value) > 0)
					{
						contact[columnName] = value;
					}
					else
					{
						contact[columnName] = "";
					}
				}
				
				// 将联系人添加到数组
				contacts.append(contact);
			}
			
			util::logging::print("GetContacts: Parsed {} contacts", contacts.size());
		}
		else
		{
			util::logging::print("GetContacts: No contacts found");
		}
	}
	else
	{
		// 查询失败
		util::logging::print("GetContacts: Query failed, error={}", err ? err : "unknown");
		Json::Value error;
		error["error"] = err ? err : "unknown database error";
		contacts = error;
	}
	
	// 7. 释放资源（重要！）
	if (result != nullptr)
	{
		util::invokeCdecl<void>(
			(void*)(base + WeixinX::weixin_dll::v41021::offset::db::free_table), 
			result
		);
		util::logging::print("GetContacts: Resources freed");
	}
	
	// 8. 转换 JSON 为字符串并返回
	Json::StreamWriterBuilder builder;
	builder["indentation"] = "  ";
	builder["emitUTF8"] = true;
	std::string jsonString = Json::writeString(builder, contacts);
	
	util::logging::print("GetContacts: Returning {} bytes of JSON", jsonString.length());
	return jsonString;
}


string WeixinX::Core::GetGroupContacts(string wxid)
{
	util::logging::print("GetGroupContacts: Starting to query chatroom_member database with details");
	
	// 1. 检查数据库句柄是否存在
	if (WeixinX::Features::DBHandles.find("contact.db") == WeixinX::Features::DBHandles.end())
	{
		util::logging::print("GetGroupContacts: no handle to contact.db (not found in map)");
		Json::Value error;
		error["error"] = "contact.db handle not found";
		Json::StreamWriterBuilder builder;
		builder["indentation"] = "";
		builder["emitUTF8"] = true;
		return Json::writeString(builder, error);
	}
	
	// 2. 检查数据库句柄值是否为空（0）
	uintptr_t dbHandle = WeixinX::Features::DBHandles["contact.db"];
	if (dbHandle == 0)
	{
		util::logging::print("GetGroupContacts: contact.db handle is null (0), WeChat may not be logged in");
		Json::Value error;
		error["error"] = "contact.db handle is null, WeChat may not be logged in";
		Json::StreamWriterBuilder builder;
		builder["indentation"] = "";
		builder["emitUTF8"] = true;
		return Json::writeString(builder, error);
	}

	// 3. 准备查询变量
	uintptr_t base = util::getWeixinDllBase();
	char* err = nullptr;
	char** result = nullptr;
	int row = 0, col = 0;
	int rc;
	
	// 4. 构建 SQL 查询语句 - 使用多表 JOIN
	// chatroom_member 存储的是数字 ID，需要通过 chat_room 和 contact 表映射到微信 ID
	
	// 🔥 记录传入的 wxid 参数（用于调试）
	util::logging::print("GetGroupContacts: Received wxid parameter: [{}] (length={})", 
		wxid.c_str(), wxid.length());
	
	std::string sql = 
		"SELECT "
		// 群成员关系（原始数字 ID）
		"cm.room_id, "
		"cm.member_id, "
		// 群微信 ID（从 chat_room 表映射）
		"cr.username AS room_wxid, "
		// 成员微信 ID（从 contact 表通过 id 映射）
		"c_member.username AS member_wxid, "
		// 成员详细信息（从 contact 表）
		"c_member.nick_name AS member_nickname, "
		"c_member.alias AS member_alias, "
		"c_member.remark AS member_remark, "
		"c_member.small_head_url AS member_avatar, "
		// 群详细信息（从 contact 表）
		"c_room.nick_name AS room_nickname, "
		"c_room.small_head_url AS room_avatar, "
		"c_room.remark AS room_remark "
		"FROM chatroom_member cm "
		// LEFT JOIN chat_room 表：room_id (数字) -> username (微信ID)
		"LEFT JOIN chat_room cr ON cm.room_id = cr.id "
		// LEFT JOIN contact 表（成员）：member_id (数字) -> id -> username
		"LEFT JOIN contact c_member ON cm.member_id = c_member.id "
		// LEFT JOIN contact 表（群）：room_wxid (微信ID) -> username
		"LEFT JOIN contact c_room ON cr.username = c_room.username ";
	
	// 🔥 如果提供了 wxid 参数，添加 WHERE 子句过滤指定群
	if (!wxid.empty())
	{
		// 转义单引号（防止 SQL 注入）
		std::string escaped_wxid = wxid;
		size_t pos = 0;
		while ((pos = escaped_wxid.find("'", pos)) != std::string::npos)
		{
			escaped_wxid.replace(pos, 1, "''");
			pos += 2;
		}
		
		sql += "WHERE cr.username = '" + escaped_wxid + "' ";
		util::logging::print("GetGroupContacts: ✅ Filtering by wxid=[{}] (escaped=[{}])", 
			wxid.c_str(), escaped_wxid.c_str());
	}
	else
	{
		util::logging::print("GetGroupContacts: ⚠️ No wxid filter - querying ALL groups");
	}
	
	sql += "ORDER BY cm.room_id, cm.member_id";
	
	// 🔥 输出完整的 SQL 语句（用于调试）
	util::logging::print("GetGroupContacts: Executing SQL: {}", sql.c_str());
	
	// 5. 调用 get_table 查询
	rc = util::invokeCdecl<int>(
		(void*)(base + WeixinX::weixin_dll::v41021::offset::db::get_table),
		dbHandle,
		sql.c_str(), 
		&result, 
		&row, 
		&col, 
		&err
	);
	
	// 6. 构建 JSON 结果
	Json::Value members(Json::arrayValue);
	
	if (rc == 0)
	{
		util::logging::print("GetGroupContacts: Query successful, rows={}, cols={}", row, col);
		
		if (row > 0 && col > 0)
		{
			// idx 从 col 开始，跳过列名行
			int idx = col;
			
			// 遍历每一行数据
			for (int x = 0; x < row; x++)
			{
				Json::Value member;
				
				// 遍历每一列
				for (int y = 0; y < col; y++)
				{
					// 获取列名（从 result 的前 col 个元素）
					const char* columnName = result[y];
					// 获取数据（可能为 NULL）
					const char* value = result[idx++];
					
					// 将数据添加到 JSON 对象
					if (value != nullptr && strlen(value) > 0)
					{
						member[columnName] = value;
					}
					else
					{
						member[columnName] = "";
					}
				}
				
				// 将成员添加到数组
				members.append(member);
			}
			
			util::logging::print("GetGroupContacts: Parsed {} members with details", members.size());
		}
		else
		{
			util::logging::print("GetGroupContacts: No members found");
		}
	}
	else
	{
		// 查询失败
		util::logging::print("GetGroupContacts: Query failed, error={}", err ? err : "unknown");
		Json::Value error;
		error["error"] = err ? err : "unknown database error";
		members = error;
	}
	
	// 7. 释放资源（重要！）
	if (result != nullptr)
	{
		util::invokeCdecl<void>(
			(void*)(base + WeixinX::weixin_dll::v41021::offset::db::free_table), 
			result
		);
		util::logging::print("GetGroupContacts: Resources freed");
	}
	
	// 8. 转换 JSON 为字符串并返回
	Json::StreamWriterBuilder builder;
	builder["indentation"] = "  ";
	builder["emitUTF8"] = true;
	std::string jsonString = Json::writeString(builder, members);
	
	util::logging::print("GetGroupContacts: Returning {} bytes of JSON", jsonString.length());
	return jsonString;
}


concurrency::concurrent_queue<WeixinX::MsgReceived> WeixinX::MsgReceived::msgReceived_queue = concurrency::concurrent_queue<WeixinX::MsgReceived>();
void WeixinX::MsgReceived::Received(WeixinX::weixin_dll::v41021::weixin_struct::MsgReceived* msg) {



	size_t pos = msg->content.str().find("\n");
	if (pos == std::string::npos) {
		return;
	}

	std::string rawContent = util::trim(msg->content.str().substr(pos + 1).c_str());

	MsgReceived msgReceived;
	msgReceived.receiver1 = msg->receiver1.str();
	msgReceived.receiver2 = msg->receiver2.str();
	msgReceived.sender = msg->sender.str();
	msgReceived.ts = msg->ts;
	msgReceived.fromChatroom = msg->receiver1.str().find("@chatroom") != std::string::npos;
	msgReceived.content = rawContent;

	// 将 msgReceived 转换为 JSON 并打印（不转义中文字符）
	Json::Value j;
	j["receiver1"] = msgReceived.receiver1;	//这个是聊天对象ID, 如果是群的话，就是群ID
	j["receiver2"] = msgReceived.receiver2; //群主ID?
	j["sender"] = msgReceived.sender;		//发送者ID
	j["ts"] = (Json::Int64)msgReceived.ts;	//消息发送时间戳
	j["fromChatroom"] = msgReceived.fromChatroom; //是否是群聊 true:是群聊 false:不是群聊
	j["content"] = msgReceived.content;	//消息内容
	
	Json::StreamWriterBuilder builder;
	builder["indentation"] = "  ";
	builder["emitUTF8"] = true;  // 启用 UTF-8 输出，不转义中文
	const std::string jsonString = Json::writeString(builder, j);
	
	util::logging::wPrint(L"MsgReceived JSON:\n{}", util::utf8ToUtf16(jsonString.c_str()));
	
	// 🔥 推送消息到所有已连接的 Socket 客户端（C# 端）
	auto& core = util::Singleton<Core>::Get();
	auto* socketServer = core.GetSocketServer();
	if (socketServer) {
		// 构造推送消息的 params（直接使用 j 作为参数）
		socketServer->Broadcast("OnMessage", j);
		util::logging::print("📤 Message pushed to C# client via Socket");
	}

	//test_queue.push(rawContent);
	//util::logging::print("ts = {:d} msg.ts = {:d} ts - msg.ts = {:d}", util::Timestamp(), msgReceived.ts, util::Timestamp() - msgReceived.ts);
	if (msgReceived.fromChatroom && util::Timestamp() - msgReceived.ts < 5000 && (msgReceived.content.starts_with("/") || msgReceived.content.starts_with("╱"))) {
		if (msgReceived.content.starts_with("╱")) {

			string content = msgReceived.content.substr(msgReceived.content.find_first_not_of("╱"));
			string slash = "/";
			msgReceived.content = slash.append(content);
		}
		WeixinX::MsgReceived::msgReceived_queue.push(msgReceived);
	}

}


#define SQLITE_OK (0)
//这个是hook微信打开所有数据库的函数，并保存数据库的句柄，以备后续使用sql语句查询数据库时使用
int _fastcall WeixinX::Detour::hkOpenDatabase(const char* dbName, uintptr_t** ppHandle, unsigned int flags, const char* zVfs) {

	int retval = 0;
	static auto oOpenDatabae = Detour::OpenDatabase.GetOriginal<decltype(&Detour::hkOpenDatabase)>();

	retval = oOpenDatabae(dbName, ppHandle, flags, zVfs);


	if (retval == SQLITE_OK) {
		std::string name = util::split(dbName, "\\").back();
		Features::DBHandles[name] = (uintptr_t)(*ppHandle);
		util::logging::print("openDatabase {:#08X} {}", Features::DBHandles[name], name);
	}


	return retval;
}

//这是hook微信添加历史消息到sqlite的函数，历史消息是一个链表
__int64 _fastcall WeixinX::Detour::hkAddMsgListToDb(__int64 rcx, __int64 rdx, uintptr_t r8)
{

	weixin_dll::v41021::weixin_struct::MsgReceived* head = ((weixin_dll::v41021::weixin_struct::MsgReceived**)r8)[0];
	weixin_dll::v41021::weixin_struct::MsgReceived* tail = ((weixin_dll::v41021::weixin_struct::MsgReceived**)r8)[1];

	while (head != tail)
	{

		if (head->type == 1) {// || head->type == 0x31) {
			MsgReceived::Received(head);
		}

		head += 1;
	}

	static auto oAddMsgListToDb = Detour::AddMsgListToDb.GetOriginal<decltype(&Detour::hkAddMsgListToDb)>();
	return oAddMsgListToDb(rcx, rdx, r8);
}

// 初始化 Socket 服务器
void WeixinX::Core::InitializeSocketServer()
{
	util::logging::print("Initializing Socket Server...");
	
	m_socketServer = std::make_unique<Socket::SocketServer>(6328);
	
	// 注册所有命令处理器
	Socket::SocketCommands::RegisterAll(m_socketServer.get());
	
	// 启动服务器
	if (m_socketServer->Start()) {
		util::logging::print("Socket Server started successfully on port 6328");
	} else {
		util::logging::print("Failed to start Socket Server");
	}
}