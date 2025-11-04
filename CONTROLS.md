# Controls Guide

### Edit / Simulation Mode
- **Space** — Toggle between *Edit* and *Simulation* mode  
  - **Edit Mode**: place, move, and delete components  
  - **Simulation Mode**: run the clock and toggle inputs with **Left Click**

### Input
- **Left Click** — Toggle input  
- **Q** or **1** — Place input toggle

### Wire
- Sends signal in all four directions  
- **B** or **2** — Place wire

### NOT Gate
- Inverts input (left → right)  
- **N** or **3** — Place NOT gate

### Cross
- Swaps signals: Left ↔ Right, Top ↔ Bottom  
- **X** or **4** — Place cross

### Clock
- 4 Hz level-triggered clock by default  
- **C** or **5** — Place clock  
- Clock frequency adjustable via the bottom-right input field (Hz)

### Eraser
- **Right Mouse Button** — Erase components  
- **E** or **6** — Eraser tool

### Marquee (Selection Tool)
- Select and move components  
- **M** or **7** — Marquee tool  

#### Marquee Operations
| Action | Shortcut |
|--------|-----------|
| Cut | Ctrl + X |
| Copy | Ctrl + C |
| Paste | Ctrl + V |
| Duplicate | Ctrl + D |
| Select All | Ctrl + A |
| Delete | Del |
| Deselect | Right Mouse Button |
| Multiselect | Ctrl / Shift |

### View Controls
- **Middle Mouse Button** — Pan  
- **Scroll Wheel** — Zoom

### Undo / Redo
- **Undo:** Ctrl + Z  
- **Redo:** Ctrl + Y / Ctrl + Shift + Z

### Save Slots
- **Ctrl + S** — Save current state  
- 10 save slots (0–9)  
- **Shift + [0–9]** — Load slot  
- Clipboard preserved between sessions

### Grid and Clock Settings
- Bottom-right UI panel:  
  - **+ / -** — Adjust grid size  
  - Components stay centered when reducing grid size  
  - Clock frequency input (Hz)

---

## Circular Dependency Warning ⚠️

Circular dependencies occur when components depend on each other’s outputs in a feedback loop.

- Highlighted with a **yellow box**  
- Sometimes false positives may occur — try **recompiling**  
- Actual loops should be fixed before running simulation  
- **Zero-tick** means only *level-triggered* circuits work properly  
  - Edge-triggered designs (like JK flip-flops) can cause circular dependencies  
  - Use **master-slave flip-flops** instead
