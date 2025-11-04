
#include <windows.h>
#include <shlobj.h>   
#include <iostream>
#include <fstream>
#include <filesystem>
#include <set>

#include "ii3image.h"

using namespace std;
namespace fs = std::filesystem;

unsigned long long TimestampII3() {

	std::chrono::system_clock::time_point current_time = std::chrono::system_clock::now();
	std::chrono::seconds sec = std::chrono::duration_cast<std::chrono::seconds>(current_time.time_since_epoch());
	std::chrono::nanoseconds nsec = std::chrono::duration_cast<std::chrono::nanoseconds>(current_time.time_since_epoch());

	return nsec.count();
}

void ii3images::ensure_image_directory_exists()
{
	wchar_t buffer[MAX_PATH];
	SHGetSpecialFolderPath(0, buffer, CSIDL_LOCAL_APPDATA, false);

	wstring pathwxx = format(L"{}/WeixinX", buffer);
	wstring pathii3 = format(L"{}/WeixinX/ii3/", buffer);
	if (!fs::is_directory(fs::path(pathwxx))) {

		fs::create_directory(fs::path(pathwxx));

	}
	if (!fs::is_directory(fs::path(pathii3))) {

		fs::create_directory(fs::path(pathii3));

	}
}

std::wstring ii3images::save_ii3_image(const char* data, int len)
{
	wchar_t buffer[MAX_PATH];
	SHGetSpecialFolderPath(0, buffer, CSIDL_LOCAL_APPDATA, false);

	wstring ii3file = format(L"{}/WeixinX/ii3/{}.jpg", buffer, to_wstring(TimestampII3()));

	ofstream ii3(ii3file, ios::out | ios::binary);
	ii3.write(data, len);

	return ii3file;
}

void ii3images::remove_stale_files()
{

	wchar_t buffer[MAX_PATH];
	SHGetSpecialFolderPath(0, buffer, CSIDL_LOCAL_APPDATA, false);
	set<wstring> stale_files;
	wstring pathii3 = format(L"{}/WeixinX/ii3/", buffer);

	if (!fs::is_directory(fs::path(pathii3))) {
		return;
	}

	for (auto& p : fs::directory_iterator(pathii3)) {

		if (p.path().has_stem()) {
			if (stoull(p.path().stem().string()) < TimestampII3() - std::chrono::duration_cast<std::chrono::nanoseconds>(chrono::seconds(300)).count()) {
				stale_files.insert(p.path().wstring());
			}
		}
	}


	for (set<wstring>::iterator it = stale_files.begin(); it != stale_files.end(); it++) {

		fs::remove(fs::path(*it));
	}

}


