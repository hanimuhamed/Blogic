# nandScape

**nandScape** is a sandbox-style **digital logic simulator** that lets you draw circuits and watch them come alive in real time.  
Built to be a playground for tinkerers, computer architecture enthusiasts, and anyone fascinated by how computation emerges from nothing but logic gates. From a single NAND to a fully working CPU, you can build it all, right inside nandScape.

---

## Why nandScape?

Unlike traditional circuit simulators, nandScape is designed as a **creative sandbox** rather than a strict engineering tool.  
It treats logic design as an art form, each circuit is a pattern, each signal a living pulse.  
You’re free to explore, break, and rebuild ideas from the ground up.

- Build flip-flops, adders, multiplexers, and even CPUs  
- Visualize real-time logic flow in a grid-based space  
- Mix engineering with pixel-perfect creativity  
- No simulation delay, everything reacts instantly  
- Designed to encourage experimentation and discovery  

---

## Features

- Zero-tick simulation (instant level-triggered logic)
- Component-based sandbox: input, wire, not gate, clocks, and more
- Ten save slots within each projects with clipboard persistence for manageability
- Adjustable grid and clock frequency
- Full editing suite: copy, paste, duplicate, undo, redo
- Dynamic view control and quick mode switching
- Capable of simulating full computational units (ALUs, registers, memory, etc.)

---

## Download

The latest playable build of **nandScape** is available under the  
[**Releases**](https://github.com/<your-username>/nandScape/releases) section of this repository.  
1. Download the latest `.7z` build for your platform.  
2. Extract and run it, no installation required.

> **Note:** First-time startup may take a few seconds while the simulation environment initializes.

---

## Building from Source

If you’d like to explore or modify the project in Unity, you can clone it from source.

### Clone the repository
```bash
git clone https://github.com/<your-username>/nandScape.git
```
### Open in Unity

1. Launch **Unity 6000.1.13f1** (tested and verified version).  
2. Choose **Open Project** and select the cloned `nandScape` folder.  
3. Wait for Unity to complete the first-time asset import — this may take a few minutes.  
4. Once loaded, enter **Play Mode** to start the sandbox environment.

> **Note:** The project is designed for Unity 6 (2025).  
> Newer Unity 6.x releases should remain compatible, though reimports may be required.

---

## License

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)**.  
You are free to use, modify, and distribute this software under the same open terms.  
Any derivative work must also remain open source under GPL-3.0.

For details, see the [LICENSE](./LICENSE) file.

---

## Known Issues

- Circular dependency detection can occasionally trigger **false positives**  
  - If this occurs, recompile the project  
  - Genuine feedback loops must be manually resolved

---

## Author

**Hani Muhamed**  
Created as an experimental logic sandbox, a digital playground for exploring how computation emerges from fundamental logic.  
Contributions, forks, and feedback are welcome.
