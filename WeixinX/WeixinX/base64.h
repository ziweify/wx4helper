#pragma once

#include <map>
#include <string>

namespace base64 {

    void encode(const char* source, size_t size, std::string* target);
    bool decode(const char* source, size_t size, std::string* target);

}