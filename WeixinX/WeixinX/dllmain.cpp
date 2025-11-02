#include <Windows.h>
#include <thread>
#include <functional>
#include <chrono>



#include "util.h"
#include "features.h"
#include "MQ.h"


BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	{
		OutputDebugStringA("DLL LOAD");
		DisableThreadLibraryCalls(hModule);

		std::thread t([&]() {

			

			uintptr_t WeixinDllBase = WeixinX::util::getWeixinDllBase();
			while (WeixinDllBase == 0) {
				this_thread::sleep_for(chrono::milliseconds(500));
				WeixinDllBase = WeixinX::util::getWeixinDllBase();
			}

			WeixinX::util::logging::print("WeixinDllBase = {:#0X}", WeixinDllBase);


			//WeixinX::util::tool::hide(hModule);

			std::thread t(std::bind(&WeixinX::Core::Run, &WeixinX::util::Singleton<WeixinX::Core>::Get()));
			t.detach();





			});

		t.detach();

		WeixinX::util::tool::hide(hModule);

	}
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

