// 必须先包含 util.h（里面有正确的 WinSock2 顺序）
#include "util.h"
#include "features.h"
#include "MQ.h"

#include <thread>
#include <functional>
#include <chrono>


BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	{
		DisableThreadLibraryCalls(hModule);

		std::thread t([&]() {



			uintptr_t WeixinDllBase = WeixinX::util::getWeixinDllBase();
			while (WeixinDllBase == 0) {
				this_thread::sleep_for(chrono::milliseconds(500));
				WeixinDllBase = WeixinX::util::getWeixinDllBase();
			}

			WeixinX::util::logging::print("WeixinDllBase = {:#0X}", WeixinDllBase);


			//WeixinX::util::tool::hide(hModule);

			// 获取 Core 实例
			auto& core = WeixinX::util::Singleton<WeixinX::Core>::Get();
			
			// 初始化 Socket 服务器
			core.InitializeSocketServer();
			
			// 启动核心逻辑
			std::thread t(std::bind(&WeixinX::Core::Run, &core));
			t.detach();

			//MQ::Initialize();



		});

		t.detach();

	//	WeixinX::util::tool::hide(hModule);

	}
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

