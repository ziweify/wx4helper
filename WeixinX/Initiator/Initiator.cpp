#include <windows.h>
#include <shlobj.h>   
#include <fstream>
#include <filesystem>
#include <algorithm>
#include <iostream>
#include <fstream>
#include <regex>
#include <set>
#include <locale>
#include <exception>

#include "CmdOpts.h"
#include "Parallel.h"
#include "Injector.h"


using namespace std;

struct Opts
{
	/*wstring port_to_bind{};
	wstring remote_ip_and_port {};*/
	wstring ip;
	wstring port;
};


vector<wstring> split(wstring& str, wstring&& delimiter)
{
	vector<wstring> substrs;
	wregex d(delimiter);

	copy(
		wsregex_token_iterator(str.begin(), str.end(), d, -1),
		wsregex_token_iterator(),
		back_inserter<vector<wstring>>(substrs)
	);

	return substrs;
}



namespace fs = std::filesystem;

void clear_folder(wstring folder) {

	if (fs::is_directory(fs::path(folder))) {

		set<wstring> to_be_removed;
		for (auto& p : fs::directory_iterator(folder)) {

			if (fs::is_directory(p)) {
				clear_folder(p.path().wstring());
				if (fs::is_empty(p)) {
					fs::remove(p.path().wstring());
				}
			}
			else {
				fs::remove(p.path().wstring());
			}
			//to_be_removed.insert(p.path().wstring());
		}


		/*for (set<wstring>::iterator it = to_be_removed.begin(); it != to_be_removed.end(); it++) {

			fs::remove(fs::path(*it));
		}*/
	}
}


int wmain(int argc, const wchar_t* argv[])
{

#pragma region chinese output
	locale::global(locale(""));
	wcout.imbue(locale(""));
#pragma endregion

	//#pragma region hide console window
	//    HWND handle_to_console;
	//    AllocConsole();
	//    handle_to_console = FindWindow(L"ConsoleWindowClass", NULL);
	//    ShowWindow(handle_to_console, 0);
	//#pragma endregion

#ifdef _DEBUG

	copy(argv, argv + argc, ostream_iterator<wstring, wchar_t>(wcout, L"\n"));

#endif


#pragma region parse arguments

	auto parser = CmdOpts<Opts>::Create({
		{ L"-i", &Opts::ip },
		{ L"-p", &Opts::port }
		});

	auto opts = parser->parse(argc, argv);

#pragma endregion


#pragma region check arguments

	/* if (parser->port_to_bind.length() == 0 || parser->remote_ip_and_port.length() == 0)
	 {
		 wcout << L"wrong arguments." << endl;
		 return EXIT_FAILURE;
	 }*/
	if (parser->ip.length() == 0 || parser->port.length() == 0)
	{
		wcout << L"wrong arguments." << endl;
		return EXIT_FAILURE;
	}

	const wregex port_pattern(L"^\\d{4,5}$");
	const wregex ip_pattern(L"^((\\d|[1-9]\\d|1\\d\\d|2[0-4]\\d|25[0-5])\\.){3}(\\d|[1-9]\\d|1\\d\\d|2[0-4]\\d|25[0-5])$");
	if (!regex_match(parser->ip, ip_pattern) || !regex_match(parser->port, port_pattern))
	{
		wcout << L"wrong arguments." << endl;
		return EXIT_FAILURE;
	}

	/*vector<wstring> ip_and_port = split(parser->remote_ip_and_port, L":");
	int remote_port = stoi(ip_and_port[1]);
	int local_port = stoi(parser->port_to_bind);*/
	int port = stoi(parser->port);

	if (port < 5001 || port > 65535)
	{
		wcout << "port number supposed to be 5001 ~ 65535." << endl;
		return EXIT_FAILURE;
	}

#pragma endregion


	HKEY wechat_key;
	LSTATUS status = RegOpenKeyEx(HKEY_CURRENT_USER, L"Software\\Tencent\\Weixin", 0, KEY_ALL_ACCESS, &wechat_key);
	if (status != ERROR_SUCCESS)
	{
		wcout << L"no wechat entry in registry." << endl;
		return EXIT_FAILURE;
	}

#pragma region get path to wechat executable from registry

	wchar_t install_path[MAX_PATH];
	DWORD size = MAX_PATH;
	status = RegQueryValueEx(wechat_key, L"InstallPath", 0, 0, (LPBYTE)install_path, &size);
	if (status != ERROR_SUCCESS)
	{
		wcout << L"failed to read wechat install path from registry." << endl;
		RegCloseKey(wechat_key);
		return EXIT_FAILURE;
	}
	wstring path_to_wechat_exe(install_path);
	path_to_wechat_exe.append(L"\\Weixin.exe");

#pragma endregion

#pragma region zero NeedUpdateType to keep wechat from auto-updating

	DWORD value = 0;
	status = RegSetValueEx(wechat_key, L"NeedUpdateType", 0, REG_DWORD, (const BYTE*)&value, 4);

#ifdef _DEBUG
	if (status != ERROR_SUCCESS)
		wcout << L"failed to zero NeedUpdateType, errcode = " << status << endl;
#endif

#pragma endregion

#pragma region clear weixin update folder

	wchar_t buffer[MAX_PATH];
	SHGetSpecialFolderPath(0, buffer, CSIDL_APPDATA, false);

	wstring update_folder = format(L"{}/Tencent/xwechat/update", buffer);
	clear_folder(update_folder);


#pragma endregion


	RegCloseKey(wechat_key);

#pragma region set environment variables

	/* if (!SetEnvironmentVariable(L"wechatx_port_to_bind", parser->port_to_bind.c_str())
		 || !SetEnvironmentVariable(L"wechatx_remote_ip_and_port", parser->remote_ip_and_port.c_str()))
	 {
		 wcout << L"failed to set environment variables, errcode = " << GetLastError() << L"." << endl;
		 return EXIT_FAILURE;
	 }*/

	if (!SetEnvironmentVariable(L"rabbitmq_ip", parser->ip.c_str())
		|| !SetEnvironmentVariable(L"rabbitmq_port", parser->port.c_str()))
	{
		wcout << L"failed to set environment variables, errcode = " << GetLastError() << L"." << endl;
		return EXIT_FAILURE;
	}

#pragma endregion

#pragma region launch wechat injecting dll

	Parallel p;

	char current_dir[MAX_PATH];
	GetCurrentDirectoryA(MAX_PATH, current_dir);

	string path_to_wechatx(current_dir);
	path_to_wechatx.append("\\WeixinX.dll");
#ifdef _DEBUG
	cout << "payload: " << path_to_wechatx << endl;
#endif
	//path_to_wechatx = "E:\\workspace\\20220402\\Initiator\\Debug\\WeChatX.dll";
	ifstream f(path_to_wechatx.c_str());
	if (!f.good())
	{
		cout << path_to_wechatx << " doesn't exist." << endl;
		return EXIT_FAILURE;
	}

	unique_ptr<Process> wechat_process(new Process(path_to_wechat_exe));
	if (!wechat_process->launch(CREATE_SUSPENDED))
	{
		return EXIT_FAILURE;
	}

	Injector injector(move(wechat_process), path_to_wechatx);
	if (!injector.inject())
	{
		return EXIT_FAILURE;
	}

#pragma endregion


	return EXIT_SUCCESS;
}
