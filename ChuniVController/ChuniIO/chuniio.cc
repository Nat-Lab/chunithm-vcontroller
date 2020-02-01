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
static struct sockaddr_in remote;
static bool remote_exist = false;

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

    DWORD timeout = 10;
    if (setsockopt(chuni_socket, SOL_SOCKET, SO_RCVTIMEO, (const char*)&timeout, sizeof(DWORD)) != 0) {
        log_error("setsockopt() failed.\n",);
        return S_FALSE;
    }

    if (bind(chuni_socket, (struct sockaddr*) & local, sizeof(struct sockaddr_in)) == SOCKET_ERROR) {
        log_error("bind() failed.\n");
        return S_FALSE;
    }

    log_info("server socket initialization completed.\n");

    return S_OK;
}

void chuni_io_jvs_read_coin_counter(uint16_t* out)
{
    if (out == NULL) {
        return;
    }

    if (chuni_coin_pending) chuni_coins++;
    chuni_coin_pending = false;

    *out = chuni_coins;
}

void chuni_io_jvs_poll(uint8_t* opbtn, uint8_t* beams)
{
    if (chuni_test_pending) {
        *opbtn |= 0x01;
        chuni_test_pending = false;
    }

    if (chuni_service_pending) {
        *opbtn |= 0x02;
        chuni_service_pending = false;

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
    chuni_io_slider_thread = NULL;
    chuni_io_slider_stop_flag = false;
}

void chuni_io_slider_set_leds(const uint8_t* rgb)
{
    static uint8_t prev_rgb_status[96];
    static chuni_msg_t message;
    message.src = SRC_GAME;
    message.type = LED_SET;

    // ignore odd, since no 1/32 color strip exist.
    for (uint8_t i = 0; i < 96; i += 6) {
        if (rgb[i] != prev_rgb_status[i] || rgb[i + 1] != prev_rgb_status[i + 1] || rgb[i + 2] != prev_rgb_status[i + 2]) {
            uint8_t n = i / 6;
            log_debug("SET_LED[%d]: rgb(%d, %d, %d)\n", n, rgb[i + 1], rgb[i + 2], rgb[i]);
            if (!remote_exist) log_warn("remote does not exist.\n");
            else {
                message.target = n;
                message.led_color_r = rgb[i + 1];
                message.led_color_g = rgb[i + 2];
                message.led_color_b = rgb[i];
                sendto(chuni_socket, (const char*)&message, sizeof(chuni_msg_t), 0, (const sockaddr*)&remote, sizeof(struct sockaddr_in));
            }
        }
            
        prev_rgb_status[i] = rgb[i];
        prev_rgb_status[i + 1] = rgb[i + 1];
        prev_rgb_status[i + 2] = rgb[i + 2];
    }
}

static unsigned int __stdcall chuni_io_slider_thread_proc(void* ctx)
{
    chuni_io_slider_callback_t callback;
    static uint8_t pressure[32];
    static char recv_buf[32];
    size_t i;

    for (i = 0; i < _countof(pressure); i++) pressure[i] = 0;

    callback = (chuni_io_slider_callback_t) ctx;

    while (!chuni_io_slider_stop_flag) {
        int addr_sz = sizeof(struct sockaddr_in);
        // FIXME: discard pending data on proc start?
        int len = recvfrom(chuni_socket, recv_buf, 32, 0, (sockaddr*)&remote, &addr_sz);
        remote_exist = true;
        if (len == (int) sizeof(chuni_msg_t)) {
            const chuni_msg_t* msg = (const chuni_msg_t*)recv_buf;
            if (msg->src != SRC_CONTROLLER) {
                log_warn("got non-controller message.\n");
                callback(pressure);
                continue;
            }
            switch (msg->type) {
                case COIN_INSERT: 
                    log_debug("adding coin.\n");
                    chuni_coin_pending = true;
                    break;
                case SLIDER_PRESS: 
                    log_debug("slider_press at %d.\n", msg->target);
                    if (msg->target >= 16) log_error("invalid slider value %d in SLIDER_PRESS.\n", msg->target);
                    else {
                        pressure[(msg->target) * 2] = 128;
                        pressure[(msg->target) * 2 + 1] = 128;
                    }
                    break;
                case SLIDER_RELEASE:
                    log_debug("slider released on %d.\n", msg->target);
                    if (msg->target >= 16) log_error("invalid slider value %d in SLIDER_RELEASE.\n", msg->target);
                    else {
                        pressure[(msg->target) * 2] = 0;
                        pressure[(msg->target) * 2 + 1] = 0;
                    }
                    break;
                case CABINET_TEST:
                    log_debug("setting cabinet_test.\n");
                    chuni_test_pending = true;
                    break;
                case CABINET_SERVICE:
                    log_debug("setting cabinet_service.\n");
                    chuni_service_pending = true;
                    break;
                case IR_BLOCKED:
                    log_debug("ir %d blokced.\n", msg->target);
                    if (msg->target >= 6) log_error("invalid slider value %d in IR_BLOCKED.\n", msg->target);
                    else chuni_ir_sensor_map |= 1 << msg->target;
                    break;
                case IR_UNBLOCKED:
                    log_debug("ir %d unblokced.\n", msg->target);
                    if (msg->target >= 6) log_error("invalid slider value %d in IR_UNBLOCKED.\n", msg->target);
                    else chuni_ir_sensor_map &= ~(1 << msg->target);
                    break;
                default:
                    log_error("bad message type %d.\n", msg->type);
            }
        }
        else if (len > 0) {
            log_warn("got invalid packet of length %d.\n", len);
        }

        callback(pressure);
    }

    return 0;
}
