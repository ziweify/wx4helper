#include "Loader.h"
#include "Process.h"
#include "Injector.h"
#include "Parallel.h"
#include <windows.h>
#include <shlobj.h>
#include <string>
#include <sstream>
#include <filesystem>
#include <fstream>
#include <regex>

using namespace std;
namespace fs = std::filesystem;

// 辅助函数：清空文件夹
void clear_folder(wstring folder)
{
    if (fs::is_directory(fs::path(folder)))
    {
        for (auto& p : fs::directory_iterator(folder))
        {
            if (fs::is_directory(p))
            {
                clear_folder(p.path().wstring());
                if (fs::is_empty(p))
                {
                    fs::remove(p.path().wstring());
                }
            }
            else
            {
                fs::remove(p.path().wstring());
            }
        }
    }
}

// 辅助函数：从字符串转换到 int
int safe_wstring_to_int(const wstring& str)
{
    try
    {
        return stoi(str);
    }
    catch (...)
    {
        return 0;
    }
}

extern "C" LOADER_API bool LaunchWeChatWithInjection(
    const wchar_t* ip,
    const wchar_t* port,
    const wchar_t* dllPath,
    wchar_t* errorMessage,
    int errorMessageSize)
{
    try
    {
        wstring error_msg;

        // 验证参数
        if (!ip || !port || !dllPath)
        {
            wcscpy_s(errorMessage, errorMessageSize, L"参数不能为空");
            return false;
        }

        wstring ip_str(ip);
        wstring port_str(port);

        // 验证 IP 和端口格式
        const wregex port_pattern(L"^\\d{4,5}$");
        const wregex ip_pattern(L"^((\\d|[1-9]\\d|1\\d\\d|2[0-4]\\d|25[0-5])\\.){3}(\\d|[1-9]\\d|1\\d\\d|2[0-4]\\d|25[0-5])$");
        
        if (!regex_match(ip_str, ip_pattern) || !regex_match(port_str, port_pattern))
        {
            wcscpy_s(errorMessage, errorMessageSize, L"IP地址或端口格式不正确");
            return false;
        }

        int port_num = safe_wstring_to_int(port_str);
        if (port_num < 5001 || port_num > 65535)
        {
            wcscpy_s(errorMessage, errorMessageSize, L"端口号范围必须在 5001 ~ 65535");
            return false;
        }

        // 检查 DLL 是否存在
        ifstream dll_file(dllPath);
        if (!dll_file.good())
        {
            wstringstream ss;
            ss << L"找不到 DLL 文件: " << dllPath;
            wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
            return false;
        }
        dll_file.close();

        // 从注册表获取微信路径
        HKEY wechat_key;
        LSTATUS status = RegOpenKeyEx(HKEY_CURRENT_USER, L"Software\\Tencent\\Weixin", 0, KEY_ALL_ACCESS, &wechat_key);
        if (status != ERROR_SUCCESS)
        {
            wcscpy_s(errorMessage, errorMessageSize, L"注册表中找不到微信安装信息");
            return false;
        }

        wchar_t install_path[MAX_PATH];
        DWORD size = MAX_PATH;
        status = RegQueryValueEx(wechat_key, L"InstallPath", 0, 0, (LPBYTE)install_path, &size);
        if (status != ERROR_SUCCESS)
        {
            RegCloseKey(wechat_key);
            wcscpy_s(errorMessage, errorMessageSize, L"无法读取微信安装路径");
            return false;
        }

        wstring path_to_wechat_exe(install_path);
        path_to_wechat_exe.append(L"\\Weixin.exe");

        // 禁用微信自动更新
        DWORD value = 0;
        RegSetValueEx(wechat_key, L"NeedUpdateType", 0, REG_DWORD, (const BYTE*)&value, 4);

        // 清空微信更新文件夹
        wchar_t buffer[MAX_PATH];
        SHGetSpecialFolderPath(0, buffer, CSIDL_APPDATA, false);
        wstring update_folder = wstring(buffer) + L"/Tencent/xwechat/update";
        clear_folder(update_folder);

        RegCloseKey(wechat_key);

        // 设置环境变量
        if (!SetEnvironmentVariable(L"rabbitmq_ip", ip_str.c_str()) ||
            !SetEnvironmentVariable(L"rabbitmq_port", port_str.c_str()))
        {
            wstringstream ss;
            ss << L"设置环境变量失败，错误代码: " << GetLastError();
            wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
            return false;
        }

        // 启动微信进程（挂起状态）
        Parallel p;

        // 转换 DLL 路径为 ANSI 字符串
        int dll_path_len = WideCharToMultiByte(CP_ACP, 0, dllPath, -1, NULL, 0, NULL, NULL);
        char* dll_path_ansi = new char[dll_path_len];
        WideCharToMultiByte(CP_ACP, 0, dllPath, -1, dll_path_ansi, dll_path_len, NULL, NULL);
        string path_to_dll(dll_path_ansi);
        delete[] dll_path_ansi;

        unique_ptr<Process> wechat_process(new Process(path_to_wechat_exe));
        if (!wechat_process->launch(CREATE_SUSPENDED, error_msg))
        {
            wcscpy_s(errorMessage, errorMessageSize, error_msg.c_str());
            return false;
        }

        // 注入 DLL
        Injector injector(move(wechat_process), path_to_dll);
        if (!injector.inject(error_msg))
        {
            wcscpy_s(errorMessage, errorMessageSize, error_msg.c_str());
            return false;
        }

        wcscpy_s(errorMessage, errorMessageSize, L"成功启动微信并注入 DLL");
        return true;
    }
    catch (const exception& e)
    {
        wstringstream ss;
        ss << L"发生异常: " << e.what();
        wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
        return false;
    }
    catch (...)
    {
        wcscpy_s(errorMessage, errorMessageSize, L"发生未知异常");
        return false;
    }
}

extern "C" LOADER_API bool InjectDllToProcess(
    DWORD processId,
    const wchar_t* dllPath,
    wchar_t* errorMessage,
    int errorMessageSize)
{
    try
    {
        if (!dllPath)
        {
            wcscpy_s(errorMessage, errorMessageSize, L"DLL路径不能为空");
            return false;
        }

        // 检查 DLL 是否存在
        ifstream dll_file(dllPath);
        if (!dll_file.good())
        {
            wstringstream ss;
            ss << L"找不到 DLL 文件: " << dllPath;
            wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
            return false;
        }
        dll_file.close();

        // 打开目标进程
        HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, processId);
        if (hProcess == NULL)
        {
            wstringstream ss;
            ss << L"无法打开进程 " << processId << L"，错误代码: " << GetLastError();
            wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
            return false;
        }

        HMODULE kernel32 = GetModuleHandle(L"kernel32.dll");
        if (kernel32 == nullptr)
        {
            CloseHandle(hProcess);
            wstringstream ss;
            ss << L"GetModuleHandle(\"kernel32.dll\") 失败，错误代码: " << GetLastError();
            wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
            return false;
        }

        LPVOID load_library = (LPVOID)GetProcAddress(kernel32, "LoadLibraryW");
        if (load_library == nullptr)
        {
            CloseHandle(hProcess);
            wstringstream ss;
            ss << L"GetProcAddress(kernel32, \"LoadLibraryW\") 失败，错误代码: " << GetLastError();
            wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
            return false;
        }

        size_t dll_path_size = (wcslen(dllPath) + 1) * sizeof(wchar_t);
        LPVOID dll_path_remote = VirtualAllocEx(hProcess, NULL, dll_path_size, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
        if (dll_path_remote == NULL)
        {
            CloseHandle(hProcess);
            wstringstream ss;
            ss << L"VirtualAllocEx 失败，错误代码: " << GetLastError();
            wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
            return false;
        }

        if (!WriteProcessMemory(hProcess, dll_path_remote, dllPath, dll_path_size, NULL))
        {
            VirtualFreeEx(hProcess, dll_path_remote, 0, MEM_RELEASE);
            CloseHandle(hProcess);
            wstringstream ss;
            ss << L"WriteProcessMemory 失败，错误代码: " << GetLastError();
            wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
            return false;
        }

        HANDLE hThread = CreateRemoteThread(hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)load_library, dll_path_remote, 0, NULL);
        if (hThread == NULL)
        {
            VirtualFreeEx(hProcess, dll_path_remote, 0, MEM_RELEASE);
            CloseHandle(hProcess);
            wstringstream ss;
            ss << L"CreateRemoteThread 失败，错误代码: " << GetLastError();
            wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
            return false;
        }

        WaitForSingleObject(hThread, INFINITE);
        CloseHandle(hThread);
        VirtualFreeEx(hProcess, dll_path_remote, 0, MEM_RELEASE);
        CloseHandle(hProcess);

        wcscpy_s(errorMessage, errorMessageSize, L"成功注入 DLL 到进程");
        return true;
    }
    catch (const exception& e)
    {
        wstringstream ss;
        ss << L"发生异常: " << e.what();
        wcscpy_s(errorMessage, errorMessageSize, ss.str().c_str());
        return false;
    }
    catch (...)
    {
        wcscpy_s(errorMessage, errorMessageSize, L"发生未知异常");
        return false;
    }
}

extern "C" LOADER_API int GetWeChatProcesses(
    DWORD* processIds,
    int maxCount)
{
    try
    {
        Parallel p;
        vector<DWORD> pids = p.all_wechat_processes();

        int count = min((int)pids.size(), maxCount);
        for (int i = 0; i < count; i++)
        {
            processIds[i] = pids[i];
        }

        return count;
    }
    catch (...)
    {
        return 0;
    }
}

