# 🧭 FlowField Pathfinding (2D Tilemap)

A simple **FlowField pathfinding system** built for Unity.  
Some parts may be a bit rough — this project was made mainly for **learning and experimentation**, but all core features are functional.

---

## 🎯 Overview

This project generates a **2D top-down flow field** based on a Tilemap and allows agents (FlowAgents) to smoothly navigate around obstacles toward their target.  
The system **requires a Tilemap** and has been tested with **Rectangular Tilemaps**.

---

## ⚙️ Features

- 🧩 Fully compatible with **2D Top-Down** and **Tilemap (Rectangular)** setups  
- 🚧 Manual obstacle coverage using `FlowFieldObstacle` — directly modifies covered Tilemap cells  
- 🔄 Dynamic field generation — the flow field can be recalculated during gameplay  
- 🧍‍♂️ Agent integration — FlowAgents teleport to the nearest flow zone and follow the flow directions  
- 🎯 ScriptableObject-based **TileCost** system for flexible movement costs per Tile or Layer

---

## ⚠️ Limitations & Warnings

- ⚠️ Designed **only for 2D Top-Down** games  
- ⚠️ **Tilemap is required** — the system won’t work without one  
- ⚠️ Tested **only on Rectangular Tilemaps** — other layouts may produce unexpected results  
- ⚠️ **There may be bugs or missing features** — this is an early experimental version  
- ⚠️ Be mindful of performance when using large maps (e.g., re-scanning or spawning many agents can be costly)

---

## 🧠 How It Works

1. **FlowFieldController**  
   The main component of the system. Handles creation, destruction, and cost calculations of the flow field.

2. **FlowFieldObstacle**  
   Added to scene objects that act as obstacles.  
   The manually defined `tileCoverage` determines which tiles the object occupies; these tiles are then converted into impassable tiles.  
   This makes the flow system treat them as high-cost regions.  
   (Optionally, this logic can be moved to a hidden Tilemap layer with small modifications.)

3. **TileCostSO (Scriptable Object)**  
   Defines the cost value for each Tile type.  
   The flow field uses this data to calculate the best possible direction for agents.

4. **FlowAgent**  
   Reads the generated flow field and moves smoothly toward the target following the calculated directions.

> Thanks to this structure, both obstacles and movement costs can be **updated in real-time on the Tilemap**.  
> The field can be recreated or modified dynamically during gameplay.

---

## 🧩 Quick Start Guide

1. Create a new **Tilemap (Rectangular)** in your Unity scene.  
2. Add the **`FlowFieldObstacle.cs`** script to any GameObjects that should act as obstacles.  
3. Edit the `tileCoverage` array to manually define the area each object occupies.  
   (Float values can be used as world offsets — the system automatically finds the nearest tile.)  
4. Assign your **`targetTilemap`** and **`obstacleTile`** in the **Inspector**.  
5. Create a **`TileCostSO` (ScriptableObject)** and define movement costs for each tile type.  
   Example:  
   - Ground = 1  
   - Mud = 2  
   - Water = 999  
6. Add the **FlowFieldController** component to your scene and assign references (Tilemap, TileCostSO, target object, etc.).  
7. Add the **FlowAgent** script to your moving characters or objects.  
   Agents automatically read the flow field and move toward the target.  
8. Start the game — the system updates Tilemap data, generates the flow field (currently triggered **manually** via an Inspector button),  
   and agents dynamically move within the generated field.

---

## 📝 NOTE

This project was developed with assistance from **AI (ChatGPT – GPT-5)**.  
Artificial intelligence was used to help with technical ideas, structuring, and implementation suggestions.

---
