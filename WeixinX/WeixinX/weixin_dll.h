#pragma once

#include <windows.h>
#include <string>

using namespace std;
namespace WeixinX {


	namespace weixin_dll {

		namespace v41021 {

			namespace offset {

				constexpr uintptr_t is_online = 0x8BB9908;
				constexpr uintptr_t current_user_info = 0x8B87420;

				namespace db {

					constexpr uintptr_t open_database = 0x28B9C80;
					constexpr uintptr_t add_msg_list_to_db = 0x2A96A30;
					constexpr uintptr_t get_table = 0x4923550;
					constexpr uintptr_t free_table = 0x49238D0;
				}

				namespace message {

					constexpr uintptr_t send_message = 0x1A3DB50;

					constexpr uintptr_t  param1_vtable = 0x702AA88;

					constexpr uintptr_t  create_param2 = 0xED10;
					constexpr uintptr_t  param2 = 0x8534A98;
					constexpr uintptr_t  param2_1 = 0x6FE6AF8;
					constexpr uintptr_t  param2_2 = 0x6FE6BB8;
					constexpr uintptr_t  param2_3 = 0x6FE6C78;

					constexpr uintptr_t	txt_message_ctr = 0x64E530;
					constexpr uintptr_t txt_message_vtable = 0x6D4A898;

					constexpr uintptr_t img_message_ctr = 0x1879290;
					constexpr uintptr_t img_message_vtable = 0x6F502E8;

					constexpr uintptr_t utf8_to_wstring = 0x94F90;

				}
			
			}

			namespace weixin_struct{

#pragma pack(push, 1)

				struct WeixinWideString {

					wchar_t* ptr;
					uint64_t capacity;
					uint64_t length;
					uint64_t flag;

				};

				struct WeixinString
				{
					uintptr_t start;
					uintptr_t padding;
					uint64_t length;
					uint64_t flag;

					std::string str()
					{
						if (start != 0)
						{
							if (flag != 0xf)
							{
								std::string str((char*)start, length);
								return str;
							}
							else
							{
								std::string str((char*)this, length);
								return str;
							}
						}

						return "";
					}
				};

				struct MsgReceived {
					uintptr_t vftable;
					uint32_t  unknown1;
					uint8_t   type;
					uint8_t   unknown2[3];
					uint32_t  unknown3[2];
					WeixinString receiver1;
					WeixinString receiver2;
					WeixinString sender;
					uint64_t  unknown4[10];
					uint64_t  ts;
					uint64_t  unknown5[9];
					WeixinString content;
					uint64_t  unknown6[33];

				};


				struct TextMessage {

					uint64_t padding1[19] = { 0 };
					string   receiver;
					uint64_t unknown1[1] = { 0 };
					uint64_t type = 1;
					uint64_t padding2[12] = { 0 };
					uint64_t msg_len;
					uint64_t padding3[114] = { 0 };
					string uuid;
					string content;
					string unkonwstr;
				};

				struct ImageMessage {
					uint64_t padding1[19] = { 0 };
					string receiver;
					uint64_t unknown1 = 1;
					uint64_t type = 3;
					WeixinWideString path;
					uint64_t unknown2[8] = { 0 };
					uint64_t size;
					uint64_t padding2[114] = { 0 };
					std::string uuid;
					uint64_t padding3[18] = { 0 };
				};

				struct UnknownBlock
				{
					uint64_t vtable;
					uint64_t temp[6] = { 0 };
					uint64_t self;
				};


#pragma pack(pop)

			}

			 

		}
	}
	
}
