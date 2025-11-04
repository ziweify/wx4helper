#pragma once

#include <windows.h>

#ifdef LOADER_EXPORTS
#define LOADER_API __declspec(dllexport)
#else
#define LOADER_API __declspec(dllimport)
#endif

extern "C" {
    /// <summary>
    /// 启动微信并注入 WeixinX.dll
    /// </summary>
    /// <param name="ip">RabbitMQ服务器IP地址</param>
    /// <param name="port">RabbitMQ服务器端口</param>
    /// <param name="dllPath">WeixinX.dll 的完整路径</param>
    /// <param name="errorMessage">错误信息输出（需要预分配缓冲区，至少512字节）</param>
    /// <param name="errorMessageSize">错误信息缓冲区大小</param>
    /// <returns>成功返回 true，失败返回 false</returns>
    LOADER_API bool LaunchWeChatWithInjection(
        const wchar_t* ip,
        const wchar_t* port,
        const wchar_t* dllPath,
        wchar_t* errorMessage,
        int errorMessageSize
    );

    /// <summary>
    /// 注入 DLL 到指定进程
    /// </summary>
    /// <param name="processId">目标进程ID</param>
    /// <param name="dllPath">DLL 的完整路径</param>
    /// <param name="errorMessage">错误信息输出（需要预分配缓冲区，至少512字节）</param>
    /// <param name="errorMessageSize">错误信息缓冲区大小</param>
    /// <returns>成功返回 true，失败返回 false</returns>
    LOADER_API bool InjectDllToProcess(
        DWORD processId,
        const wchar_t* dllPath,
        wchar_t* errorMessage,
        int errorMessageSize
    );

    /// <summary>
    /// 获取所有正在运行的微信进程ID
    /// </summary>
    /// <param name="processIds">进程ID数组（需要预分配，至少10个DWORD）</param>
    /// <param name="maxCount">数组最大容量</param>
    /// <returns>返回找到的进程数量</returns>
    LOADER_API int GetWeChatProcesses(
        DWORD* processIds,
        int maxCount
    );
}

