//#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <exception>
#include <thread>
#include <chrono>

#include "util.h"
#include "3rd/include/uuid.h"
#include "3rd/include/rmqcxx/rmqcxx.hpp"

#include "MQ.h"



using namespace std;
using namespace rmqcxx;

wstring MQ::host = L"";
int MQ::port = 0;

concurrency::concurrent_queue<string> MQ::in_queue = concurrency::concurrent_queue<string>();
concurrency::concurrent_queue<string> MQ::out_queue = concurrency::concurrent_queue<string>();

void MQ::GetEnvironmentVars()
{
	wchar_t buf[15]{ 0 };
	GetEnvironmentVariableW(L"rabbitmq_ip", buf, 15);
	host.assign(buf, wcslen(buf));

	memset(buf, 0, 15 * sizeof(wchar_t));
	GetEnvironmentVariableW(L"rabbitmq_port", buf, 15);
	port = stoi(buf);

	WeixinX::util::logging::wPrint(L"rabbitmq host = {} rabbitmq port = {}", host, port);
}

void MQ::Initialize()
{
	MQ::GetEnvironmentVars();


	//WeixinX::util::logging::wPrint(L"ip = {} port = {d}", host, port);
	if (host.length() == 0 || port == 0) {
		WeixinX::util::logging::wPrint(L"empty rabbitmq ip or port");
		exit(1);
	}




	MQ::InitializePublisher();
	MQ::InitializeConsumer();


}

class TimeoutException :public std::exception {

public:
	TimeoutException(const char* why) {
		this->reason = why;
	}

	virtual const char* what() {
		return this->reason;
	}


	const char* reason;

};

void MQ::InitializeConsumer()
{
	std::random_device rd;
	auto seed_data = std::array<int, std::mt19937::state_size> {};
	std::generate(std::begin(seed_data), std::end(seed_data), std::ref(rd));
	std::seed_seq seq(std::begin(seed_data), std::end(seed_data));
	std::mt19937 generator(seq);
	uuids::uuid_random_generator gen{ generator };

	uuids::uuid const id = gen();




}

void MQ::InitializePublisher()
{

}
