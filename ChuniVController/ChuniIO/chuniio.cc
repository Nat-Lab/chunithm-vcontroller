#include <winsock2.h>
#include <windows.h>
#include <process.h>
#include <stdio.h>
#include <stdbool.h>
#include <fcntl.h>
#include <io.h>
#include "chuniio.h"
#include "log.h"
#pragma comment(lib,"ws2_32.lib")

static unsigned int __stdcall chuni_io_slider_thread_proc(void* ctx);

static bool chuni_coin_pending = false;
static bool chuni_service_pending = false;
static bool chuni_test_pending = false;
static uint16_t chuni_coins = 0;
static uint8_t chuni_ir_sensor_map = 0;
static HANDLE chuni_io_slider_thread;
static bool chuni_io_slider_stop_flag;
static SOCKET chuni_socket;
static USHORT chuni_port = 24864; // CHUNI on dialpad
static char recv_buf[32];

HRESULT chuni_io_jvs_init(void)
{
    // alloc console for debug output
    AllocConsole();
    FILE* fp;
    freopen_s(&fp, "CONOUT$", "w", stdout);

    log_debug("allocated debug console.\n");

    struct sockaddr_in local;
    memset(&local, 0, sizeof(struct sockaddr_in));

    WSAData wsad;
    if (WSAStartup(MAKEWORD(2, 2), &wsad) != 0) {
        log_error("WSAStartup failed.\n");
        return S_FALSE;
    }

    chuni_socket = socket(AF_INET, SOCK_DGRAM, 0);
    if (chuni_socket == INVALID_SOCKET) {
        log_error("socket() failed.\n");
        return S_FALSE;
    }

    local.sin_addr.s_addr = INADDR_ANY; // TODO: make configurable
    local.sin_family = AF_INET;
    local.sin_port = htons(chuni_port);

    DWORD timeout = 1000;
    if (setsockopt(chuni_socket, SOL_SOCKET, SO_RCVTIMEO, (const char*)&timeout, sizeof(DWORD)) != 0) {
        log_error("setsockopt() failed.\n",);
        return S_FALSE;
    }

    if (bind(chuni_socket, (struct sockaddr*) & local, sizeof(struct sockaddr_in)) == SOCKET_ERROR) {
        log_error("bind() failed.\n");
        return S_FALSE;
    }

    log_info("initialization completed.\n");

    return S_OK;
}

void chuni_io_jvs_read_coin_counter(uint16_t* out)
{
    if (out == NULL) {
        return;
    }

    if (chuni_coin_pending) chuni_coins++;

    *out = chuni_coins;
}

void chuni_io_jvs_poll(uint8_t* opbtn, uint8_t* beams)
{
    if (chuni_test_pending) {
        *opbtn |= 0x01; /* Test */
    }

    if (chuni_service_pending) {
        *opbtn |= 0x02; /* Service */
    }

    *beams = chuni_ir_sensor_map;
}

void chuni_io_jvs_set_coin_blocker(bool open)
{
    if (open) log_info("coin blocker disabled");
    else log_info("coin blocker enabled.");
    
}

HRESULT chuni_io_slider_init(void)
{
    log_info("init slider...\n");
    return S_OK;
}

void chuni_io_slider_start(chuni_io_slider_callback_t callback)
{
    log_info("starting slider...\n");
    if (chuni_io_slider_thread != NULL) {
        return;
    }

    chuni_io_slider_thread = (HANDLE)_beginthreadex(
        NULL,
        0,
        chuni_io_slider_thread_proc,
        callback,
        0,
        NULL);
}

void chuni_io_slider_stop(void)
{
    log_info("stopping slider...\n");
    if (chuni_io_slider_thread == NULL) {
        return;
    }

    chuni_io_slider_stop_flag = true;

    WaitForSingleObject(chuni_io_slider_thread, INFINITE);
    CloseHandle(chuni_io_slider_thread);
    closesocket(chuni_socket);
    chuni_io_slider_thread = NULL;
    chuni_io_slider_stop_flag = false;
}

void chuni_io_slider_set_leds(const uint8_t* rgb)
{
    /*for (uint8_t i = 0; i < 32; i++) {
        log_debug("SET_LED[%d]: %d\n", i, rgb[i]);
    }*/
}

static unsigned int __stdcall chuni_io_slider_thread_proc(void* ctx)
{
    chuni_io_slider_callback_t callback;
    uint8_t pressure[32];
    size_t i;

    for (i = 0; i < _countof(pressure); i++) pressure[i] = 0;

    callback = (chuni_io_slider_callback_t) ctx;

    while (!chuni_io_slider_stop_flag) {
        int len = recv(chuni_socket, recv_buf, 32, 0);
        if (len == (int) sizeof(chuni_msg_t)) {
            // TOD: message parsing, event dispatch logic
        }
        else if (len > 0) {
            log_warn("got invalid packet of length %d.", len);
        }

        callback(pressure);
    }

    return 0;
}
