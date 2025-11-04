#pragma once

#include <windows.h>
#include <tlhelp32.h>
#include <vector>

using namespace std;

class Parallel
{
public:
    Parallel()
    {
        adjust_privileges();
        // 在构造函数中不执行关闭句柄操作，由外部控制
    }

    ~Parallel()
    {
    }

    void adjust_privileges()
    {
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
    }

    vector<DWORD> all_wechat_processes()
    {
        vector<DWORD> pids;

        HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (snap == INVALID_HANDLE_VALUE)
        {
            return pids;
        }

        PROCESSENTRY32 pe32;
        pe32.dwSize = sizeof(PROCESSENTRY32);

        bool success = Process32First(snap, &pe32);
        while (success)
        {
            if (wcscmp(pe32.szExeFile, L"Weixin.exe") == 0 ||
                wcscmp(pe32.szExeFile, L"WeChat.exe") == 0)
            {
                pids.push_back(pe32.th32ProcessID);
            }
            success = Process32Next(snap, &pe32);
        }
        CloseHandle(snap);
        return pids;
    }
};

