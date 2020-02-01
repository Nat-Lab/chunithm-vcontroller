#pragma once
#define log_debug(fmt, ...) log("DEBUG", fmt, ## __VA_ARGS__)
#define log_info(fmt, ...) log("INFO ", fmt, ## __VA_ARGS__)
#define log_notice(fmt, ...) log("NOTE ", fmt, ## __VA_ARGS__)
#define log_warn(fmt, ...) log("WARN ", fmt, ## __VA_ARGS__)
#define log_error(fmt, ...) log("ERROR", fmt, ## __VA_ARGS__)
#define log_fatal(fmt, ...) log("FATAL", fmt, ## __VA_ARGS__)
#define log(log_level, fmt, ...) printf("[" log_level "] %s: " fmt, __FUNCSIG__, ## __VA_ARGS__)