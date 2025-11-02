#pragma once

#include <windows.h>
#include <tlhelp32.h>
//#include <NTSecAPI.h>
#include <iostream>
#include <vector>


#define NT_SUCCESS(x) ((x) >= 0)
#define STATUS_INFO_LENGTH_MISMATCH 0xc0000004

#define SystemHandleInformation 16

typedef enum _OBJECT_INFORMATION_CLASS {
    ObjectBasicInformation,
    ObjectNameInformation,
    ObjectTypeInformation,
    ObjectAllInformation,
    ObjectDataInformation
} OBJECT_INFORMATION_CLASS;


typedef NTSTATUS(NTAPI* _NtQuerySystemInformation)(
    ULONG SystemInformationClass,
    PVOID SystemInformation,
    ULONG SystemInformationLength,
    PULONG ReturnLength
    );
typedef NTSTATUS(NTAPI* _NtDuplicateObject)(
    HANDLE SourceProcessHandle,
    HANDLE SourceHandle,
    HANDLE TargetProcessHandle,
    PHANDLE TargetHandle,
    ACCESS_MASK DesiredAccess,
    ULONG Attributes,
    ULONG Options
    );
typedef NTSTATUS(NTAPI* _NtQueryObject)(
    HANDLE ObjectHandle,
    ULONG ObjectInformationClass,
    PVOID ObjectInformation,
    ULONG ObjectInformationLength,
    PULONG ReturnLength
);

typedef struct _UNICODE_STRING {
    USHORT Length;
    USHORT MaximumLength;
    PWSTR Buffer;
} UNICODE_STRING, * PUNICODE_STRING;

typedef struct _SYSTEM_HANDLE {
    ULONG ProcessId;
    BYTE ObjectTypeNumber;
    BYTE Flags;
    USHORT Handle;
    PVOID Object;
    ACCESS_MASK GrantedAccess;
} SYSTEM_HANDLE, * PSYSTEM_HANDLE;

typedef struct _SYSTEM_HANDLE_INFORMATION {
    ULONG HandleCount;
    SYSTEM_HANDLE Handles[1];
} SYSTEM_HANDLE_INFORMATION, * PSYSTEM_HANDLE_INFORMATION;

typedef enum _POOL_TYPE {
    NonPagedPool,
    PagedPool,
    NonPagedPoolMustSucceed,
    DontUseThisType,
    NonPagedPoolCacheAligned,
    PagedPoolCacheAligned,
    NonPagedPoolCacheAlignedMustS
} POOL_TYPE, * PPOOL_TYPE;

typedef struct _OBJECT_TYPE_INFORMATION {
    UNICODE_STRING Name;
    ULONG TotalNumberOfObjects;
    ULONG TotalNumberOfHandles;
    ULONG TotalPagedPoolUsage;
    ULONG TotalNonPagedPoolUsage;
    ULONG TotalNamePoolUsage;
    ULONG TotalHandleTableUsage;
    ULONG HighWaterNumberOfObjects;
    ULONG HighWaterNumberOfHandles;
    ULONG HighWaterPagedPoolUsage;
    ULONG HighWaterNonPagedPoolUsage;
    ULONG HighWaterNamePoolUsage;
    ULONG HighWaterHandleTableUsage;
    ULONG InvalidAttributes;
    GENERIC_MAPPING GenericMapping;
    ULONG ValidAccess;
    BOOLEAN SecurityRequired;
    BOOLEAN MaintainHandleCount;
    USHORT MaintainTypeList;
    POOL_TYPE PoolType;
    ULONG PagedPoolUsage;
    ULONG NonPagedPoolUsage;
} OBJECT_TYPE_INFORMATION, * POBJECT_TYPE_INFORMATION;


//#define STATUS_INFO_LENGTH_MISMATCH     ((NTSTATUS)0xC0000004L) 
//#define SystemHandleInformation 6
//
//typedef struct _PUBLIC_OBJECT_BASIC_INFORMATION {
//    ULONG       Attributes;
//    ACCESS_MASK GrantedAccess;
//    ULONG       HandleCount;
//    ULONG       PointerCount;
//    ULONG       Reserved[10];
//} PUBLIC_OBJECT_BASIC_INFORMATION, * PPUBLIC_OBJECT_BASIC_INFORMATION;
//
//typedef enum _OBJECT_INFORMATION_CLASS {
//    ObjectBasicInformation,
//    ObjectNameInformation,
//    ObjectTypeInformation,
//    ObjectAllInformation,
//    ObjectDataInformation
//} OBJECT_INFORMATION_CLASS;
//
//typedef struct _UNICODE_STRING {
//    USHORT Length;
//    USHORT MaximumLength;
//#ifdef MIDL_PASS
//    [size_is(MaximumLength / 2), length_is((Length) / 2)] USHORT* Buffer;
//#else // MIDL_PASS
//    PWSTR  Buffer;
//#endif // MIDL_PASS
//} UNICODE_STRING;
//
typedef struct _OBJECT_NAME_INFORMATION
{
    UNICODE_STRING          Name;
    BYTE                    Unknown2[32];
} OBJECT_NAME_INFORMATION, * POBJECT_NAME_INFORMATION;
typedef UNICODE_STRING* PUNICODE_STRING;

//typedef struct _SYSTEM_HANDLE_INFORMATION
//{
//    ULONG ProcessId;
//    UCHAR ObjectTypeNumber;
//    UCHAR Flags;
//    USHORT Handle;
//    PVOID Object;
//    ACCESS_MASK GrantedAccess;
//}SYSTEM_HANDLE_INFORMATION, * PSYSTEM_HANDLE_INFORMATION;
//
typedef struct _SYSTEM_HANDLE_INFORMATION_EX
{
    ULONG NumberOfHandles;
    SYSTEM_HANDLE_INFORMATION Information[1];
}SYSTEM_HANDLE_INFORMATION_EX, * PSYSTEM_HANDLE_INFORMATION_EX;
//
//typedef NTSTATUS(NTAPI* ZWQUERYSYSTEMINFORMATION)(ULONG SystemInformationClass, PVOID SystemInformation, ULONG SystemInformationLength, PULONG ReturnLength);
//typedef NTSTATUS(NTAPI* NtQueryObject)(HANDLE  Handle, OBJECT_INFORMATION_CLASS objectInformationClass, PVOID   ObjectInformation, ULONG  ObjectInformationLength, PULONG ReturnLength);

PVOID GetLibraryProcAddress(const char* LibraryName, const char* ProcName) {
    return GetProcAddress(GetModuleHandleA(LibraryName), ProcName);
}

using namespace std;

class Parallel {

public:
	Parallel() {

        adjust_privileges();
        vector<DWORD> pids = all_wechat_processes();
        if (pids.size() == 0)
        {
            return;
        }
        PSYSTEM_HANDLE_INFORMATION handles = process_handles();

        for (int i = 0; i < pids.size(); i++) {
            close_handle(handles, pids[i], L"Weixin.exe");
        }
        //close_handles(pids);

  
	}

	~Parallel() {

	}

	void adjust_privileges() {

        HANDLE token = NULL;
        
        bool success = OpenProcessToken(GetCurrentProcess(), TOKEN_ALL_ACCESS, &token);
        if (success)
        {
            TOKEN_PRIVILEGES tp;
            tp.PrivilegeCount = 1;
           
            success = LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &tp.Privileges[0].Luid);
            tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
            
            success = AdjustTokenPrivileges(token, FALSE, &tp, sizeof(tp), NULL, NULL);
            CloseHandle(token);

        } 

        if (!success) {
            cout << "failed to adjust privileges " <<  endl;
            exit(1);
        }

	}

    vector<DWORD> all_wechat_processes() {

        vector<DWORD> pids;
        
        HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (snap == FALSE)
        {
            return pids;
        }
        PROCESSENTRY32 pe32;
        pe32.dwSize = sizeof(PROCESSENTRY32);

        bool success = Process32First(snap, &pe32);
        while (success)
        {
            if (wcscmp(pe32.szExeFile, L"Weixin.exe") == 0)
            {
                pids.push_back(pe32.th32ProcessID);
            }
            success = Process32Next(snap, &pe32);
        }
        CloseHandle(snap);
        return pids;
    }

    PSYSTEM_HANDLE_INFORMATION process_handles() {

        _NtQuerySystemInformation NtQuerySystemInformation = (_NtQuerySystemInformation)GetLibraryProcAddress("ntdll.dll", "NtQuerySystemInformation");
       /* _NtDuplicateObject NtDuplicateObject = (_NtDuplicateObject)GetLibraryProcAddress("ntdll.dll", "NtDuplicateObject");
        _NtQueryObject NtQueryObject = (_NtQueryObject)GetLibraryProcAddress("ntdll.dll", "NtQueryObject");*/

        NTSTATUS status;
        PSYSTEM_HANDLE_INFORMATION handle_info;
        ULONG handle_info_size = 0x10000;

        handle_info = (PSYSTEM_HANDLE_INFORMATION)malloc(handle_info_size);
        // NtQuerySystemInformation won't give us the correct buffer size,
        //  so we guess by doubling the buffer size.
        while ((status = NtQuerySystemInformation(
            SystemHandleInformation,
            handle_info,
            handle_info_size,
            NULL
        )) == STATUS_INFO_LENGTH_MISMATCH)
        {
            if (handle_info != NULL) {
                handle_info = (PSYSTEM_HANDLE_INFORMATION)realloc(handle_info, handle_info_size *= 2);
            }
            else {
                cout << "alloc space for handle_info failed" << endl;
                exit(1);
            }
           
            
        }
            

        // NtQuerySystemInformation stopped giving us STATUS_INFO_LENGTH_MISMATCH.
        if (!NT_SUCCESS(status)) {
            cout << "NtQuerySystemInformation failed" << endl;
            exit(1);
        }

        return handle_info;
    }

    void close_handle(PSYSTEM_HANDLE_INFORMATION handles, DWORD pid, const WCHAR* processName)
    {

        _NtDuplicateObject NtDuplicateObject = (_NtDuplicateObject)GetLibraryProcAddress("ntdll.dll", "NtDuplicateObject");
        _NtQueryObject NtQueryObject = (_NtQueryObject)GetLibraryProcAddress("ntdll.dll", "NtQueryObject");

        char type[128] = { 0 };
        char name[512] = { 0 };
        DWORD flags = 0;
        POBJECT_NAME_INFORMATION name_info;
        POBJECT_NAME_INFORMATION type_info;
       // PSYSTEM_HANDLE_INFORMATION_EX pInfo = (PSYSTEM_HANDLE_INFORMATION_EX)pBuffer;
       
        for (DWORD i = 0; i < handles->HandleCount; i++)
        {
            SYSTEM_HANDLE handle = handles->Handles[i];
           
            if (handle.ProcessId == pid)
            {
                HANDLE handle_to_close;
              
                DuplicateHandle(OpenProcess(PROCESS_ALL_ACCESS, FALSE, handle.ProcessId), (HANDLE)handle.Handle, GetCurrentProcess(), &handle_to_close, DUPLICATE_SAME_ACCESS, FALSE, DUPLICATE_SAME_ACCESS);
                
                NTSTATUS status1 = NtQueryObject(handle_to_close, ObjectNameInformation, name, 512, &flags);
                NTSTATUS status2 = NtQueryObject(handle_to_close, ObjectTypeInformation, type, 128, &flags);

                name_info = (POBJECT_NAME_INFORMATION)name;
                type_info = (POBJECT_NAME_INFORMATION)type;

                if (strcmp(name, "") && strcmp(type, "") && status1 != 0xc0000008 && status2 != 0xc0000008)
                {
                    if (wcsstr(type_info->Name.Buffer, L"Mutant"))
                    {
                        name_info = (POBJECT_NAME_INFORMATION)name;
                        type_info = (POBJECT_NAME_INFORMATION)type;
                        if (wcsstr(name_info->Name.Buffer, L"XWeChat_App_Instance_Identity_Mutex_Name"))
                        {
                            if (DuplicateHandle(OpenProcess(PROCESS_ALL_ACCESS, FALSE, handle.ProcessId), (HANDLE)handle.Handle, GetCurrentProcess(), &handle_to_close, 0, FALSE, DUPLICATE_CLOSE_SOURCE))
                            {
                                CloseHandle(handle_to_close);
                            }
                            else {
                                cout << "DuplicateHandle failed" << endl;
                                exit(1);
                            }
                        }
                    }
                }
            }
        }
    }
       
 };

    /*void EnumObjInfo(LPVOID pBuffer, DWORD pid, const WCHAR* processName) {
        char szType[128] = { 0 };
        char szName[512] = { 0 };
        DWORD dwFlags = 0;
        POBJECT_NAME_INFORMATION pNameInfo;
        POBJECT_NAME_INFORMATION pNameType;
        PSYSTEM_HANDLE_INFORMATION_EX pInfo = (PSYSTEM_HANDLE_INFORMATION_EX)pBuffer;
        ULONG OldPID = 0;
        HMODULE ntdll = GetModuleHandle(L"ntdll.dll");
        if (!ntdll) {
            cout << "failed get ntdll" << endl;
            exit(1);
        }

        for (DWORD i = 0; i < pInfo->NumberOfHandles; i++)
        {
            if (OldPID != pInfo->Information[i].ProcessId)
            {
                if (pInfo->Information[i].ProcessId == pid)
                {
                    HANDLE newHandle;
                    NtQueryObject p_NtQueryObject = (NtQueryObject)GetProcAddress(ntdll, "NtQueryObject");
                    if (p_NtQueryObject == NULL)
                    {
                        return;
                    }
                    DuplicateHandle(OpenProcess(PROCESS_ALL_ACCESS, FALSE, pInfo->Information[i].ProcessId), (HANDLE)pInfo->Information[i].Handle, GetCurrentProcess(), &newHandle, DUPLICATE_SAME_ACCESS, FALSE, DUPLICATE_SAME_ACCESS);
                    NTSTATUS status1 = p_NtQueryObject(newHandle, ObjectNameInformation, szName, 512, &dwFlags);
                    NTSTATUS status2 = p_NtQueryObject(newHandle, ObjectTypeInformation, szType, 128, &dwFlags);

                    pNameInfo = (POBJECT_NAME_INFORMATION)szName;
                    pNameType = (POBJECT_NAME_INFORMATION)szType;

                    if (strcmp(szName, "") && strcmp(szType, "") && status1 != 0xc0000008 && status2 != 0xc0000008)
                    {
                        if (wcsstr(pNameType->Name.Buffer, L"Mutant"))
                        {
                            pNameInfo = (POBJECT_NAME_INFORMATION)szName;
                            pNameType = (POBJECT_NAME_INFORMATION)szType;
                            if (wcsstr(pNameInfo->Name.Buffer, L"_WeChat_App_Instance_Identity_Mutex_Name"))
                            {
                                if (DuplicateHandle(OpenProcess(PROCESS_ALL_ACCESS, FALSE, pInfo->Information[i].ProcessId), (HANDLE)pInfo->Information[i].Handle, GetCurrentProcess(), &newHandle, 0, FALSE, DUPLICATE_CLOSE_SOURCE))
                                {
                                    CloseHandle(newHandle);
                                };
                            }
                        }
                    }
                }
            }
        }
    }*/


