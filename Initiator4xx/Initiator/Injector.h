#pragma once


#include <windows.h>
#include <string>
#include <iostream>
#include <memory>
#include <thread>
#include <chrono>

#include "Process.h"

class Injector
{
public:
	Injector(std::unique_ptr<Process> process, string payload) : process(std::move(process)), payload(payload) {}

	bool inject() 
	{
		//process->resume();
		this_thread::sleep_for(chrono::milliseconds(500));

		HMODULE kernel32 = GetModuleHandle(L"kernel32.dll");
		if (kernel32 == nullptr)
		{
			process->terminate();
			wcout << L"GetModuleHandle(\"kernel32.dll\") returned 0, errcode = " << GetLastError() << L"." << endl;
			
			return false;
		}

		LPVOID load_library = (LPVOID)GetProcAddress(kernel32, "LoadLibraryA");
		if (load_library == nullptr)
		{
			process->terminate();
			wcout << L"GetProcAddress(kernel32, \"LoadLibraryA\") returned 0, errcode = " << GetLastError() << L"." << endl;

			return false;
		}

		LPVOID arg_of_load_library = (LPVOID)VirtualAllocEx(process->handle(), NULL, payload.length(), MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
		if (arg_of_load_library == 0)
		{
			process->terminate();
			wcout << L"VirtualAllocEx returned 0, errcode = " << GetLastError() << L"." << endl;

			return false;
		}

		if (0 == WriteProcessMemory(process->handle(), arg_of_load_library, payload.c_str(), strlen(payload.c_str()),0))
		{
			process->terminate();
			wcout << L"WriteProcessMemory returned 0, errcode = " << GetLastError() << L"." << endl;

			return false;
		}

		if (0 == CreateRemoteThread(process->handle(), 0, 0, (LPTHREAD_START_ROUTINE)load_library, arg_of_load_library, 0, 0))
		{
			process->terminate();
			wcout << L"CreateRemoteThread returned 0, errcode = " << GetLastError() << L"." << endl;

			return false;
		}

		process->resume();
		return true;
	}

protected:
	std::unique_ptr<Process> process;
	string payload;
};