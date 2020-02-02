chunithm-vcontroller
---

A touch-based virtual controller for Chunithm. The simulated 16-keys touchpad maps from A to P. The space above emulates the IR sensor. The screenshot below shows how it looks in-game. 

![screenshot](https://raw.githubusercontent.com/Nat-Lab/chunithm-vcontroller/master/doc/screenshot.png)

There are a few options available at the bottom of the window. Here's what they do:

Item|Description
---|---
Key width|Controls how wide the keys are. 
Key length|Controls how long (tall) the keys are.
Sensor height|Controls how tall the IR sensor area is.
Apply|Apply width & height settings. 
Opacity|Controls window opacity.
Allow Mouse|Allow the mouse to interact with virtual keys. The virtual controller only accepts touch control by default. 
Lock Window|Lock the window in the current position. (disable dragging)
Coin|Insert coin.
Service|Service button.
Test|Test button.
S+T|Press "Service" and "Test" at the same time.
Exit|Exit.

### Usage

Downloads are available on the [release](https://github.com/Nat-Lab/chunithm-vcontroller/releases) page. Replace the "chuniio.dll" in your game folder with the one provided in the zip file. Run ChuniVController.exe, then start the game as you usually would.

The modified `chuniio.dll` binds on UDP port 24864 and listens for incoming IO messages. `ChuniVController.exe` connects to it on the localhost. The protocol specification can be found [here](https://github.com/Nat-Lab/chunithm-vcontroller/blob/master/ChuniVController/ChuniIO/chuniio.h). It should be straightforward to create other clients. (e.g., touchscreen tablet client)

### License

UNLICENSE
