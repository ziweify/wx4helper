#pragma once

#include <Windows.h>
#include <winnt.h>
#include <chrono>

#include <format>
#include <ctime>

#include "3rd/include/minhook.h"

using namespace std;
namespace WeixinX {


	namespace util {

		template <typename T>
		static T* heapAlloc(size_t bytes) {
			return (T*)HeapAlloc(GetProcessHeap(), 8, bytes);
		}

		template <typename T, typename... Args>
		T invokeCdecl(void* functionAddress, Args... args) noexcept
		{
			typedef T(__cdecl* cDeclFuncDef)(Args...);
			cDeclFuncDef f = (cDeclFuncDef)functionAddress;
			return f(args...);
		}

		inline uintptr_t getWeixinDllBase() {
			return (uintptr_t)GetModuleHandleA("Weixin.dll");
		}


		inline uint64_t Timestamp() {

			/*std::chrono::system_clock::time_point current_time = std::chrono::system_clock::now();
			std::chrono::seconds sec = std::chrono::duration_cast<std::chrono::seconds>(current_time.time_since_epoch());*/
			//std::chrono::nanoseconds nsec = std::chrono::duration_cast<std::chrono::nanoseconds>(current_time.time_since_epoch());
			std::chrono::milliseconds ms = std::chrono::duration_cast<std::chrono::milliseconds>(
				std::chrono::system_clock::now().time_since_epoch()
				);
			return ms.count();
		}

		inline std::wstring utf8ToUtf16(const char* str)
		{
			wstring wstr;
			int len = MultiByteToWideChar(CP_UTF8, 0, str, -1, 0, 0);
			if (len > 0) {
				wchar_t* wbuf = new wchar_t[len];
				memset(wbuf, '\0', sizeof(wchar_t) * (len));

				MultiByteToWideChar(CP_UTF8, 0, str, -1, wbuf, len);
				wstr = wbuf;
				delete[] wbuf;
			}

			return wstr;
		}


		inline std::string utf16ToUtf8(const std::wstring& wstr)
		{
			std::string str;
			int len = WideCharToMultiByte(CP_UTF8, 0, wstr.c_str(), -1, NULL, 0, NULL, NULL);
			if (len > 0) {
				char* buf = new char[len];
				memset(buf, '\0', sizeof(char) * (len));

				WideCharToMultiByte(CP_UTF8, 0, wstr.c_str(), -1, buf, len, NULL, NULL);
				str = buf;
				delete[] buf;
			}

			return str;
		}

		inline std::vector<string> split(const char* str, string&& delimiter)
		{
			vector<string> substrs;
			string sstr(str);
			size_t pos = 0;
			string token;
			while ((pos = sstr.find(delimiter)) != std::string::npos) {
				token = sstr.substr(0, pos);
				sstr.erase(0, pos + delimiter.length());
				substrs.push_back(token);
			}
			substrs.push_back(sstr);

			return substrs;
		}

		inline std::string trim(const char* str) {

			std::string sstr(str);

			sstr = sstr.substr(sstr.find_first_not_of("\n"));
			sstr = sstr.substr(sstr.find_first_not_of(" "));

			return sstr;
		}

		template <typename T>
		class Singleton
		{
		protected:
			Singleton() { }
			~Singleton() { }

			Singleton(const Singleton&) = delete;
			Singleton& operator=(const Singleton&) = delete;

			Singleton(Singleton&&) = delete;
			Singleton& operator=(Singleton&&) = delete;
		public:
			static T& Get()
			{
				static T Instance{ };
				return Instance;
			}
		};


		class DetourHook
		{
		public:
			DetourHook() = default;

			explicit DetourHook(void* pFunction, void* pDetour)
				: pBaseFn(pFunction), pReplaceFn(pDetour) { }


			bool Create(void* pFunction, void* pDetour)
			{
				pBaseFn = pFunction;

				if (pBaseFn == nullptr)
					return false;

				pReplaceFn = pDetour;

				if (pReplaceFn == nullptr)
					return false;

				const MH_STATUS status = MH_CreateHook(pBaseFn, pReplaceFn, &pOriginalFn);

				if (status != MH_OK)
					throw std::runtime_error(std::format("failed to create hook function, status: {}\nbase function -> {:#08X}", MH_StatusToString(status), reinterpret_cast<std::uintptr_t>(pBaseFn)));

				if (!this->Replace())
					return false;

				return true;
			}


			bool Replace()
			{

				if (pBaseFn == nullptr)
					return false;


				if (bIsHooked)
					return false;

				const MH_STATUS status = MH_EnableHook(pBaseFn);

				if (status != MH_OK)
					throw std::runtime_error(std::format("failed to enable hook function, status: {}\nbase function -> {:#08X} address", MH_StatusToString(status), reinterpret_cast<std::uintptr_t>(pBaseFn)));

				bIsHooked = true;
				return true;
			}


			bool Remove()
			{

				if (!this->Restore())
					return false;

				const MH_STATUS status = MH_RemoveHook(pBaseFn);

				if (status != MH_OK)
					throw std::runtime_error(std::format("failed to remove hook, status: {}\n base function -> {:#08X} address", MH_StatusToString(status), reinterpret_cast<std::uintptr_t>(pBaseFn)));

				return true;
			}

			bool Restore()
			{

				if (!bIsHooked)
					return false;

				const MH_STATUS status = MH_DisableHook(pBaseFn);

				if (status != MH_OK)
					throw std::runtime_error(std::format("failed to restore hook, status: {}\n base function -> {:#08X} address", MH_StatusToString(status), reinterpret_cast<std::uintptr_t>(pBaseFn)));


				bIsHooked = false;
				return true;
			}


			template <typename Fn>
			Fn GetOriginal()
			{
				return static_cast<Fn>(pOriginalFn);
			}

			/* returns hook state */
			inline bool IsHooked() const
			{
				return bIsHooked;
			}

		private:

			bool bIsHooked = false;

			void* pBaseFn = nullptr;

			void* pReplaceFn = nullptr;

			void* pOriginalFn = nullptr;
		};


		namespace logging {


			template <typename ... Args_t>
			void print(const std::string_view szText, const Args_t& ... argList)
			{

				// format time
				auto now = std::chrono::system_clock::now();
				auto ttNow = std::chrono::system_clock::to_time_t(now);

				std::stringstream ss;
				ss << std::put_time(std::localtime(&ttNow), "%Y-%m-%d %X");

				const std::string szTime = ss.str();

				std::string szDbgString{};
				if constexpr (sizeof...(argList) > 0)
					szDbgString = std::vformat(szText, std::make_format_args(argList...));
				else
					szDbgString = szText;

				OutputDebugStringA(std::vformat("[WeixinX][{}]{}", std::make_format_args(szTime, szDbgString)).c_str());

			}

			template <typename ... Args_t>
			void wPrint(const std::wstring_view szText, const Args_t& ... argList)
			{

				// format time
				auto now = std::chrono::system_clock::now();
				auto ttNow = std::chrono::system_clock::to_time_t(now);

				std::wstringstream ss;
				ss << std::put_time(std::localtime(&ttNow), L"%Y-%m-%d %X");

				const std::wstring szTime = ss.str();

				std::wstring szDbgString;
				if constexpr (sizeof...(argList) > 0)
					szDbgString = std::vformat(szText, std::make_wformat_args(argList...));
				else
					szDbgString = szText;

				OutputDebugStringW(std::vformat(L"[WeixinX][{}]{}", std::make_wformat_args(szTime, szDbgString)).c_str());
			}

		}


		namespace tool {

#pragma pack(push, 1) 
			struct _UNICODE_STRING
			{
				USHORT Length;                                                          //0x0
				USHORT MaximumLength;                                                   //0x2
				WCHAR* Buffer;                                                          //0x8
			};

			struct _RTL_BALANCED_NODE
			{
				union
				{
					struct _RTL_BALANCED_NODE* Children[2];                             //0x0
					struct
					{
						struct _RTL_BALANCED_NODE* Left;                                //0x0
						struct _RTL_BALANCED_NODE* Right;                               //0x8
					};
				};
				union
				{
					struct
					{
						UCHAR Red : 1;                                                    //0x10
						UCHAR Balance : 2;                                                //0x10
					};
					ULONGLONG ParentValue;                                              //0x10
				};
			};

			struct _PEB_LDR_DATA
			{
				ULONG Length;                                                           //0x0
				UCHAR Initialized;                                                      //0x4
				VOID* SsHandle;                                                         //0x8
				struct _LIST_ENTRY InLoadOrderModuleList;                               //0x10
				struct _LIST_ENTRY InMemoryOrderModuleList;                             //0x20
				struct _LIST_ENTRY InInitializationOrderModuleList;                     //0x30
				VOID* EntryInProgress;                                                  //0x40
				UCHAR ShutdownInProgress;                                               //0x48
				VOID* ShutdownThreadId;                                                 //0x50
			};

			struct _PEB
			{
				UCHAR InheritedAddressSpace;                                            //0x0
				UCHAR ReadImageFileExecOptions;                                         //0x1
				UCHAR BeingDebugged;                                                    //0x2
				union
				{
					UCHAR BitField;                                                     //0x3
					struct
					{
						UCHAR ImageUsesLargePages : 1;                                    //0x3
						UCHAR IsProtectedProcess : 1;                                     //0x3
						UCHAR IsImageDynamicallyRelocated : 1;                            //0x3
						UCHAR SkipPatchingUser32Forwarders : 1;                           //0x3
						UCHAR IsPackagedProcess : 1;                                      //0x3
						UCHAR IsAppContainer : 1;                                         //0x3
						UCHAR IsProtectedProcessLight : 1;                                //0x3
						UCHAR SpareBits : 1;                                              //0x3
					};
				};
				UCHAR Padding0[4];                                                      //0x4
				VOID* Mutant;                                                           //0x8
				VOID* ImageBaseAddress;                                                 //0x10
				struct _PEB_LDR_DATA* Ldr;                                              //0x18
				struct _RTL_USER_PROCESS_PARAMETERS* ProcessParameters;                 //0x20
				VOID* SubSystemData;                                                    //0x28
				VOID* ProcessHeap;                                                      //0x30
				struct _RTL_CRITICAL_SECTION* FastPebLock;                              //0x38
				VOID* AtlThunkSListPtr;                                                 //0x40
				VOID* IFEOKey;                                                          //0x48
				union
				{
					ULONG CrossProcessFlags;                                            //0x50
					struct
					{
						ULONG ProcessInJob : 1;                                           //0x50
						ULONG ProcessInitializing : 1;                                    //0x50
						ULONG ProcessUsingVEH : 1;                                        //0x50
						ULONG ProcessUsingVCH : 1;                                        //0x50
						ULONG ProcessUsingFTH : 1;                                        //0x50
						ULONG ReservedBits0 : 27;                                         //0x50
					};
				};
				UCHAR Padding1[4];                                                      //0x54
				union
				{
					VOID* KernelCallbackTable;                                          //0x58
					VOID* UserSharedInfoPtr;                                            //0x58
				};
				ULONG SystemReserved[1];                                                //0x60
				ULONG AtlThunkSListPtr32;                                               //0x64
				VOID* ApiSetMap;                                                        //0x68
				ULONG TlsExpansionCounter;                                              //0x70
				UCHAR Padding2[4];                                                      //0x74
				VOID* TlsBitmap;                                                        //0x78
				ULONG TlsBitmapBits[2];                                                 //0x80
				VOID* ReadOnlySharedMemoryBase;                                         //0x88
				VOID* SparePvoid0;                                                      //0x90
				VOID** ReadOnlyStaticServerData;                                        //0x98
				VOID* AnsiCodePageData;                                                 //0xa0
				VOID* OemCodePageData;                                                  //0xa8
				VOID* UnicodeCaseTableData;                                             //0xb0
				ULONG NumberOfProcessors;                                               //0xb8
				ULONG NtGlobalFlag;                                                     //0xbc
				union _LARGE_INTEGER CriticalSectionTimeout;                            //0xc0
				ULONGLONG HeapSegmentReserve;                                           //0xc8
				ULONGLONG HeapSegmentCommit;                                            //0xd0
				ULONGLONG HeapDeCommitTotalFreeThreshold;                               //0xd8
				ULONGLONG HeapDeCommitFreeBlockThreshold;                               //0xe0
				ULONG NumberOfHeaps;                                                    //0xe8
				ULONG MaximumNumberOfHeaps;                                             //0xec
				VOID** ProcessHeaps;                                                    //0xf0
				VOID* GdiSharedHandleTable;                                             //0xf8
				VOID* ProcessStarterHelper;                                             //0x100
				ULONG GdiDCAttributeList;                                               //0x108
				UCHAR Padding3[4];                                                      //0x10c
				struct _RTL_CRITICAL_SECTION* LoaderLock;                               //0x110
				ULONG OSMajorVersion;                                                   //0x118
				ULONG OSMinorVersion;                                                   //0x11c
				USHORT OSBuildNumber;                                                   //0x120
				USHORT OSCSDVersion;                                                    //0x122
				ULONG OSPlatformId;                                                     //0x124
				ULONG ImageSubsystem;                                                   //0x128
				ULONG ImageSubsystemMajorVersion;                                       //0x12c
				ULONG ImageSubsystemMinorVersion;                                       //0x130
				UCHAR Padding4[4];                                                      //0x134
				ULONGLONG ActiveProcessAffinityMask;                                    //0x138
				ULONG GdiHandleBuffer[60];                                              //0x140
				VOID(*PostProcessInitRoutine)();                                       //0x230
				VOID* TlsExpansionBitmap;                                               //0x238
				ULONG TlsExpansionBitmapBits[32];                                       //0x240
				ULONG SessionId;                                                        //0x2c0
				UCHAR Padding5[4];                                                      //0x2c4
				union _ULARGE_INTEGER AppCompatFlags;                                   //0x2c8
				union _ULARGE_INTEGER AppCompatFlagsUser;                               //0x2d0
				VOID* pShimData;                                                        //0x2d8
				VOID* AppCompatInfo;                                                    //0x2e0
				struct _UNICODE_STRING CSDVersion;                                      //0x2e8
				struct _ACTIVATION_CONTEXT_DATA* ActivationContextData;                 //0x2f8
				struct _ASSEMBLY_STORAGE_MAP* ProcessAssemblyStorageMap;                //0x300
				struct _ACTIVATION_CONTEXT_DATA* SystemDefaultActivationContextData;    //0x308
				struct _ASSEMBLY_STORAGE_MAP* SystemAssemblyStorageMap;                 //0x310
				ULONGLONG MinimumStackCommit;                                           //0x318
				struct _FLS_CALLBACK_INFO* FlsCallback;                                 //0x320
				struct _LIST_ENTRY FlsListHead;                                         //0x328
				VOID* FlsBitmap;                                                        //0x338
				ULONG FlsBitmapBits[4];                                                 //0x340
				ULONG FlsHighIndex;                                                     //0x350
				VOID* WerRegistrationData;                                              //0x358
				VOID* WerShipAssertPtr;                                                 //0x360
				VOID* pUnused;                                                          //0x368
				VOID* pImageHeaderHash;                                                 //0x370
				union
				{
					ULONG TracingFlags;                                                 //0x378
					struct
					{
						ULONG HeapTracingEnabled : 1;                                     //0x378
						ULONG CritSecTracingEnabled : 1;                                  //0x378
						ULONG LibLoaderTracingEnabled : 1;                                //0x378
						ULONG SpareTracingBits : 29;                                      //0x378
					};
				};
				UCHAR Padding6[4];                                                      //0x37c
				ULONGLONG CsrServerReadOnlySharedMemoryBase;                            //0x380
			};

			struct _LDR_DATA_TABLE_ENTRY
			{
				struct _LIST_ENTRY InLoadOrderLinks;                                    //0x0
				struct _LIST_ENTRY InMemoryOrderLinks;                                  //0x10
				struct _LIST_ENTRY InInitializationOrderLinks;                          //0x20
				VOID* DllBase;                                                          //0x30
				VOID* EntryPoint;                                                       //0x38
				ULONG SizeOfImage;                                                      //0x40
				struct _UNICODE_STRING FullDllName;                                     //0x48
				struct _UNICODE_STRING BaseDllName;                                     //0x58
				union
				{
					UCHAR FlagGroup[4];                                                 //0x68
					ULONG Flags;                                                        //0x68
					struct
					{
						ULONG PackagedBinary : 1;                                         //0x68
						ULONG MarkedForRemoval : 1;                                       //0x68
						ULONG ImageDll : 1;                                               //0x68
						ULONG LoadNotificationsSent : 1;                                  //0x68
						ULONG TelemetryEntryProcessed : 1;                                //0x68
						ULONG ProcessStaticImport : 1;                                    //0x68
						ULONG InLegacyLists : 1;                                          //0x68
						ULONG InIndexes : 1;                                              //0x68
						ULONG ShimDll : 1;                                                //0x68
						ULONG InExceptionTable : 1;                                       //0x68
						ULONG ReservedFlags1 : 2;                                         //0x68
						ULONG LoadInProgress : 1;                                         //0x68
						ULONG LoadConfigProcessed : 1;                                    //0x68
						ULONG EntryProcessed : 1;                                         //0x68
						ULONG ProtectDelayLoad : 1;                                       //0x68
						ULONG ReservedFlags3 : 2;                                         //0x68
						ULONG DontCallForThreads : 1;                                     //0x68
						ULONG ProcessAttachCalled : 1;                                    //0x68
						ULONG ProcessAttachFailed : 1;                                    //0x68
						ULONG CorDeferredValidate : 1;                                    //0x68
						ULONG CorImage : 1;                                               //0x68
						ULONG DontRelocate : 1;                                           //0x68
						ULONG CorILOnly : 1;                                              //0x68
						ULONG ReservedFlags5 : 3;                                         //0x68
						ULONG Redirected : 1;                                             //0x68
						ULONG ReservedFlags6 : 2;                                         //0x68
						ULONG CompatDatabaseProcessed : 1;                                //0x68
					};
				};
				USHORT ObsoleteLoadCount;                                               //0x6c
				USHORT TlsIndex;                                                        //0x6e
				struct _LIST_ENTRY HashLinks;                                           //0x70
				ULONG TimeDateStamp;                                                    //0x80
				struct _ACTIVATION_CONTEXT* EntryPointActivationContext;                //0x88
				VOID* Lock;                                                             //0x90
				struct _LDR_DDAG_NODE* DdagNode;                                        //0x98
				struct _LIST_ENTRY NodeModuleLink;                                      //0xa0
				struct _LDRP_LOAD_CONTEXT* LoadContext;                                 //0xb0
				VOID* ParentDllBase;                                                    //0xb8
				VOID* SwitchBackContext;                                                //0xc0
				struct _RTL_BALANCED_NODE BaseAddressIndexNode;                         //0xc8
				struct _RTL_BALANCED_NODE MappingInfoIndexNode;                         //0xe0
				ULONGLONG OriginalBase;                                                 //0xf8
				union _LARGE_INTEGER LoadTime;                                          //0x100
				ULONG BaseNameHashValue;                                                //0x108
				enum _LDR_DLL_LOAD_REASON LoadReason;                                   //0x10c
				ULONG ImplicitPathOptions;                                              //0x110
				ULONG ReferenceCount;                                                   //0x114
			};
#pragma pack(pop)

			static void hide(HMODULE hMod) {


				_PEB* peb = (_PEB*)__readgsqword(0x60);
				_PEB_LDR_DATA* pLdr = peb->Ldr;

				PLIST_ENTRY pBack, pNext;
				_LDR_DATA_TABLE_ENTRY* pLdm;

				pBack = &(pLdr->InLoadOrderModuleList);
				pNext = pBack->Flink;

				do
				{
					pLdm = CONTAINING_RECORD(pNext, _LDR_DATA_TABLE_ENTRY, InLoadOrderLinks);

					if (hMod == pLdm->DllBase)
					{
						pLdm->InLoadOrderLinks.Blink->Flink =
							pLdm->InLoadOrderLinks.Flink;

						pLdm->InLoadOrderLinks.Flink->Blink =
							pLdm->InLoadOrderLinks.Blink;

						pLdm->InInitializationOrderLinks.Blink->Flink =
							pLdm->InInitializationOrderLinks.Flink;

						pLdm->InInitializationOrderLinks.Flink->Blink =
							pLdm->InInitializationOrderLinks.Blink;

						pLdm->InMemoryOrderLinks.Blink->Flink =
							pLdm->InMemoryOrderLinks.Flink;

						pLdm->InMemoryOrderLinks.Flink->Blink =
							pLdm->InMemoryOrderLinks.Blink;
						break;
					}
					pNext = pNext->Flink;

				} while (pBack != pNext);

			}

			static uint64_t FileSize(const std::string& filePath) {


				if (filePath.length() == 0) {
					return 0;
				}

				struct stat statbuf;
				stat(filePath.c_str(), &statbuf);
				size_t fileSize = statbuf.st_size;

				util::logging::print("img file {} size = {}", filePath.c_str(), fileSize);
				return fileSize;

			}
		}

	}

}

