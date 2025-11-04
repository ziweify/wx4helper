#pragma once

#include <string>
#include <concurrent_queue.h>

class MQ
{
public:
	static void GetEnvironmentVars();
	static void Initialize();
	static void InitializeConsumer();
	static void InitializePublisher();

	static std::wstring host;
	static int port;

	static concurrency::concurrent_queue<std::string> in_queue;
	static concurrency::concurrent_queue<std::string> out_queue;
};