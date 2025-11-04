#pragma once

#include <windows.h>
#include <string>
#include <sstream>
#include <memory>
#include <thread>
#include <chrono>

#include "Process.h"

using namespace std;

class Injector
{
public:
    Injector(unique_ptr<Process> process, string payload) : process(move(process)), payload(payload) {}

    bool inject(wstring& error_message)
    {
        this_thread::sleep_for(chrono::milliseconds(500));

        HMODULE kernel32 = GetModuleHandle(L"kernel32.dll");
        if (kernel32 == nullptr)
        {
            process->terminate();
            wstringstream ss;
            ss << L"GetModuleHandle(\"kernel32.dll\") 失败，错误代码: " << GetLastError();
            error_message = ss.str();
            return false;
        }

        LPVOID load_library = (LPVOID)GetProcAddress(kernel32, "LoadLibraryA");
        if (load_library == nullptr)
        {
            process->terminate();
            wstringstream ss;
            ss << L"GetProcAddress(kernel32, \"LoadLibraryA\") 失败，错误代码: " << GetLastError();
            error_message = ss.str();
            return false;
        }

        LPVOID arg_of_load_library = (LPVOID)VirtualAllocEx(process->handle(), NULL, payload.length(), MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
        if (arg_of_load_library == 0)
        {
            process->terminate();
            wstringstream ss;
            ss << L"VirtualAllocEx 失败，错误代码: " << GetLastError();
            error_message = ss.str();
            return false;
        }

        if (0 == WriteProcessMemory(process->handle(), arg_of_load_library, payload.c_str(), strlen(payload.c_str()), 0))
        {
            process->terminate();
            wstringstream ss;
            ss << L"WriteProcessMemory 失败，错误代码: " << GetLastError();
            error_message = ss.str();
            return false;
        }

        if (0 == CreateRemoteThread(process->handle(), 0, 0, (LPTHREAD_START_ROUTINE)load_library, arg_of_load_library, 0, 0))
        {
            process->terminate();
            wstringstream ss;
            ss << L"CreateRemoteThread 失败，错误代码: " << GetLastError();
            error_message = ss.str();
            return false;
        }

        process->resume();
        return true;
    }

protected:
    unique_ptr<Process> process;
    string payload;
};

