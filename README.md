chunithm-vcontroller
---

A touch-based virtual controller for Chunithm. The simulated 16-keys touchpad maps from A to P. The space above emulates the IR sensor. The screenshot below shows how it looks in-game. 

![screenshot](https://raw.githubusercontent.com/Nat-Lab/chunithm-vcontroller/master/doc/screenshot.png)

There are a few options available at the bottom of the window. Here's what they do:

Item|Description
---|---
Key width|Controls how wide the keys are. For my configuration width of 80 matches the in-game UI perfectly.
Key length|Controls how long (tall) the keys are.
Sensor height|Controls how tall the IR sensor area is.
Apply|Apply width & height settings. 
Opacity|Controls window opacity.
Allow Mouse|Allow the mouse to interact with virtual keys. The virtual controller only accepts touch control by default. 
Lock Window|Lock the window in the current position. (disable dragging)
Exit|Exit.

Downloads are available on the release page.

### Appendix

If you are using Segatools, here's the cell mapping configuration for the virtual controller:

```
cell32=0x41
cell31=0x41
cell30=0x42
cell29=0x42
cell28=0x43
cell27=0x43
cell26=0x44
cell25=0x44
cell24=0x45
cell23=0x45
cell22=0x46
cell21=0x46
cell20=0x47
cell19=0x47
cell18=0x48
cell17=0x48
cell16=0x49
cell15=0x49
cell14=0x4A
cell13=0x4A
cell12=0x4B
cell11=0x4B
cell10=0x4C
cell9=0x4C
cell8=0x4D
cell7=0x4D
cell6=0x4E
cell5=0x4E
cell4=0x4F
cell3=0x4F
cell2=0x50
cell1=0x50
```

### To-dos

- Handle IO ourselves. Deal with LEDs.

### License

UNLICENSE
