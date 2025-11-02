#pragma once

#include <windows.h>
#include <string>
#include <iostream>

using namespace std;
class Process
{
public:
	Process(wstring path_to_exe) : path_to_exe(path_to_exe), handle_to_process(0), handle_to_mainthread(0) {}

    Process(Process&& p) noexcept : handle_to_process(p.handle_to_process), handle_to_mainthread(p.handle_to_mainthread) 
	{
		p.handle_to_process = 0;
		p.handle_to_mainthread = 0;
	}

	~Process()
	{
		if (handle_to_process)
		{
			CloseHandle(handle_to_process);
		}

		if (handle_to_mainthread)
		{
			CloseHandle(handle_to_mainthread);
		}

	}

	bool launch(DWORD creation_flags)
	{
		STARTUPINFO startup_info;
		PROCESS_INFORMATION process_info;

		SecureZeroMemory(&startup_info, sizeof(STARTUPINFO));
		startup_info.cb = sizeof(STARTUPINFO);

		bool result = CreateProcess(path_to_exe.c_str(), NULL, NULL, NULL, TRUE, creation_flags, NULL, NULL, &startup_info, &process_info);
		
		if (result)
		{
			handle_to_process = process_info.hProcess;
			handle_to_mainthread = process_info.hThread;
		}
		else
		{
			wcout << L"failed to launch wechat, errcode = " << GetLastError() << L"." << endl;
		}

		return result;
	}

	void terminate()
	{
		if (handle_to_process)
		{
			TerminateProcess(handle_to_process, 0);
			handle_to_process = 0;
			handle_to_mainthread = 0;
		}
	}

	HANDLE handle()
	{
		return handle_to_process;
	}

	void resume()
	{
		if (handle_to_mainthread)
		{
			ResumeThread(handle_to_mainthread);
		}
	}

protected:
	wstring path_to_exe;
	HANDLE handle_to_process;
	HANDLE handle_to_mainthread;

private:

	Process(const Process&) = delete;
	Process& operator=(const Process&) = delete;
	Process& operator=(Process&&) = delete;
};

