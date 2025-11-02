#pragma once

#include <string>
#include <string_view>
#include <sstream>
#include <functional>
#include <map>



using namespace std;

template<typename Opts>
struct CmdOpts:Opts 
{
	using prop_t = wstring Opts::*;
	using arg_t = pair<wstring, prop_t>;


public:

	~CmdOpts() = default;

    Opts parse(int argc, const wchar_t* argv[])
    {
        vector<wstring_view> vargv(argv, argv + argc);
        for (unsigned int idx = 0; idx < argc; ++idx)
            for (auto& cbk : callbacks)
                cbk.second(idx, vargv);

        return static_cast<Opts>(*this);
    }


    static unique_ptr<CmdOpts> Create(std::initializer_list<arg_t> args)
    {
        auto cmd_opts = unique_ptr<CmdOpts>(new CmdOpts());
        for (auto arg : args)
            cmd_opts->register_callback(arg.first, arg.second);
        return cmd_opts;
    }

private:
    using callback_t = function<void(int, const vector<wstring_view>&)>;
    map<wstring, callback_t> callbacks;

	CmdOpts() = default;
	CmdOpts(const CmdOpts&) = delete;
	CmdOpts(CmdOpts&&) = delete;
	CmdOpts& operator=(const CmdOpts&) = delete;
	CmdOpts& operator=(CmdOpts&&) = delete;


    void register_callback(wstring name, prop_t prop)
    {
        callbacks[name] = [this, name, prop](unsigned int idx, const vector<wstring_view>& argv)
        {
            if (argv[idx] == name && idx < argv.size() - 1)
            {
                wstringstream value;
                value << argv[idx + 1];
                value >> this->*prop;
            }
        };
    };

};

 