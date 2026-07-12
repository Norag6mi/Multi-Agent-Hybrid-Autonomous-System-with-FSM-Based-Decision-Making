# Multi-Agent NPC Combat System with Hybrid FSM-Based Decision Making

A Unity-based intelligent NPC combat system that combines **Finite State Machine (FSM) architecture**, autonomous decision-making, and tactical behaviors to simulate coordinated multi-agent combat in dynamic environments. The project focuses on creating responsive, scalable, and modular AI capable of making real-time combat decisions while adapting to changing battlefield conditions.

---

## Overview

This project explores the design of a hybrid AI architecture for autonomous NPCs by integrating deterministic **Finite State Machines (FSMs)** with modular decision-making systems. Each NPC continuously evaluates its surroundings, selects appropriate combat strategies, and transitions between behavioral states such as engaging enemies, retreating, seeking cover, repositioning, and supporting allied agents.

The architecture emphasizes maintainability, extensibility, and predictable behavior while supporting complex combat interactions among multiple AI-controlled agents.

---

## Key Features

- Hybrid **FSM-Based AI Architecture** for modular and predictable decision-making.
- Autonomous **Multi-Agent Combat** with independent behavior evaluation for each NPC.
- Dynamic state transitions driven by combat conditions, threat assessment, and environmental context.
- Tactical position selection using combat position evaluation and navigation systems.
- Cover, retreat, hold position, and attack behaviors for adaptive combat strategies.
- Real-time target detection and threat evaluation for intelligent engagement decisions.
- Modular AI components enabling easy extension with additional behaviors and combat states.
- Animation-driven combat system integrated with movement, aiming, shooting, and state transitions.
- Desktop and VR compatibility with a shared AI architecture across platforms.
- Performance-oriented design suitable for managing multiple autonomous NPCs simultaneously.

---

## Tech Stack

- **Engine:** Unity
- **Language:** C#
- **AI Architecture:** Finite State Machine (FSM)
- **Navigation:** Unity NavMesh
- **Animation:** Unity Animator & Animation State Machine
- **Physics:** Unity Physics & Line-of-Sight Detection

---

## Project Goals

- Develop a scalable and maintainable NPC AI architecture.
- Simulate intelligent squad-based combat behavior.
- Improve realism through tactical decision-making and adaptive combat responses.
- Demonstrate software engineering principles including modular design, separation of concerns, and reusable AI systems.

---

## Future Enhancements

- Utility AI / Behavior Tree hybrid decision system.
- Advanced squad coordination and communication.
- Dynamic cover reservation and flanking strategies.
- Suppression mechanics and morale system.
- Learning-based behavior optimization using reinforcement learning or neural networks.

---

## Project Status

**Completed** — Active research and feature improvements continue as new AI behaviors and combat mechanics are explored.

---

## Repository Structure

```text
Assets/
├── Scripts/
│   ├── AI/
│   ├── Navigation/
│   ├── Combat/
│   ├── Animation/
│   └── Utilities/
├── Prefabs/
├── Animations/
├── Scenes/
└── Materials/
```

> **Note:** The exact folder structure may vary depending on the current implementation.

---

## License

This project is intended for educational, research, and portfolio purposes.
