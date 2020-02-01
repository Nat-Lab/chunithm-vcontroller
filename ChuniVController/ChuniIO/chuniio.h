#include <stdint.h>
#include <stdbool.h>
#ifdef CHUNIIO_EXPORTS
#define CHUNIIO_API __declspec(dllexport)
#else
#define CHUNIIO_API __declspec(dllimport)
#endif

typedef enum {
	SRC_GAME,
	SRC_CONTROLLER
} chuni_msg_src_t;

typedef enum {
	COIN_INSERT = 0,
	SLIDER_PRESS = 1,
	SLIDER_RELEASE = 2,
	LED_SET = 3,
	CABINET_TEST = 4,
	CABINET_SERVICE = 5,
	IR_BLOCKED = 6,
	IR_UNBLOCKED = 7
} chuni_msg_type_t;

typedef struct {
	chuni_msg_src_t src;
	chuni_msg_type_t type;

	// For SLIDER_*, IR_* and LED_SET. Index of the target SLIDER/LED/IR_SENSOR
	uint8_t target;

	// for LED_SET only
	uint32_t led_color;
} chuni_msg_t;

typedef void (*chuni_io_slider_callback_t)(const uint8_t* state);

extern "C" {
	CHUNIIO_API long chuni_io_jvs_init(void);
	CHUNIIO_API void chuni_io_jvs_poll(uint8_t* opbtn, uint8_t* beams);
	CHUNIIO_API void chuni_io_jvs_read_coin_counter(uint16_t* total);
	CHUNIIO_API void chuni_io_jvs_set_coin_blocker(bool open);
	CHUNIIO_API long chuni_io_slider_init(void);
	CHUNIIO_API void chuni_io_slider_start(chuni_io_slider_callback_t callback);
	CHUNIIO_API void chuni_io_slider_stop(void);
	CHUNIIO_API void chuni_io_slider_set_leds(const uint8_t* rgb);
}