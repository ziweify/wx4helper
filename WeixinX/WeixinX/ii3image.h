#pragma once

#include <string>
namespace ii3images {

	void ensure_image_directory_exists();
	std::wstring save_ii3_image(const char* data, int len);
	void remove_stale_files();

}