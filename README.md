## 🎯 Development Goals

When developing **Clash of Steel**, I focused on exploring key aspects of **mobile backend integration**, **UI Toolkit**, and **live-service systems**.

- Learned **UI Toolkit workflows** through building a modular, localized interface with animated views.  
- Built a **backend-driven mobile game architecture** using **PlayFab**, **Azure Functions**, and **ScriptableObjects** mapping backend weapon data to in-game models.  
- Explored **account linking and cross-device progression** by integrating **Google Play Games Services** (GPGS) with PlayFab using Microsoft best practices.  
- Configured a full **Android release pipeline**, including keystore setup, Play Console integration, and internal testing release.  
- Set up an **Azure Function App** to host backend logic and connected **Google Cloud Services** for GPGS and PlayFab integration.  
- Experimented with **live-service monetization systems**, including in-app purchases, rewarded ads (Unity Mediation), loot boxes via PlayFab Economy V2, and server-authoritative cooldowns.

---

## ☁️ 1. Backend-Driven Architecture

To support a fully backend-driven structure, I built a central **`PlayFabManager`** that initializes all core services through a lightweight **Service Locator** pattern.  
Each service (`AuthService`, `EconomyService`, `AzureService`) handles a specific domain such as authentication, data syncing, or server-side logic.

![Architecture](https://img.itch.zone/aW1nLzIzODM5OTAyLnBuZw==/original/GjjyAC.png)

All game data — including **weapon definitions**, **stage configurations**, **enemy setups**, **rewards**, **currencies** (gold and diamonds), and **level-up formulas** — is fetched dynamically from **PlayFab Economy V2** at runtime.  

Nothing gameplay-related is stored locally; only 3D models, sounds, and visual assets remain on the device.  
This design allows designers to rebalance and iterate directly in PlayFab without requiring new builds.

---

### 🧱 Example Weapon Definition (JSON)

Each weapon is defined as a JSON catalog item like this:

![Weapon JSON Example](https://img.itch.zone/aW1nLzIzODQwMDg5LnBuZw==/original/%2BvhYQJ.png)

When a player’s inventory is fetched, each weapon item is mapped to a local **`ScriptableObject` (WeaponAsset)** using a **`WeaponAssetDatabase`**, linking the weapon’s localized name to its prefab and cinematic timeline.  
This ensures visuals are local while gameplay values stay server-authoritative.

![WeaponAsset Diagram 1](https://img.itch.zone/aW1nLzIzODQwMTk2LnBuZw==/original/nPwpwd.png)
![WeaponAsset Diagram 2](https://img.itch.zone/aW1nLzIzODQwMjAxLnBuZw==/original/qaN65k.png)
![WeaponAsset Diagram 3](https://img.itch.zone/aW1nLzIzODQwMjA3LnBuZw==/original/NOet5t.png)

---

### 🔧 Azure Function Integration

To keep all progression, cooldowns, and reward systems **server-trusted**, I built an **`AzureService`** that communicates with **PlayFab (Azure Functions)**.

![Azure Diagram](https://img.itch.zone/aW1nLzIzODQwNDM3LnBuZw==/original/1sgZc7.png)

A dedicated **Function App** in the **Azure Portal** hosts all backend logic in **C#**.  
The entire Azure Functions project is maintained as a **Git submodule** within the main game repository for synchronized development and version control.

![Azure Portal Screenshot](https://img.itch.zone/aW1nLzIzODQwNDk5LnBuZw==/original/1GI0A3.png)

---

## 💵 2. Live-Service Monetization

- Implemented **in-app purchases** with **Unity IAP**; receipts are securely validated via **Azure Functions** and confirmed through **PlayFab Economy V2**, granting bundles directly to the player’s inventory.

![IAP Flow](https://img.itch.zone/aW1nLzIzODUzMzk2LmpwZw==/original/sa0dVU.jpg)

- Integrated **rewarded ads** using **Unity Mediation (LevelPlay)** — combining **Unity Ads**, **ironSource**, and **AdMob** for optimal fill rates.

![Ads Integration](https://img.itch.zone/aW1nLzIzODQwNjgzLnBuZw==/original/0LmQKl.png)

- Designed **loot boxes** as **PlayFab Economy V2 catalog items**, with random rewards determined server-side via **Azure Functions** to ensure fairness.

![Loot Boxes](https://img.itch.zone/aW1nLzIzODQwODg4LnBuZw==/original/mJGj%2FE.png)

---

## 🧩 3. UI Architecture (Unity UI Toolkit)

Designed a modular and scalable **UI architecture** using **Unity’s UI Toolkit**, inspired by Unity’s official sample project and best practices.

- Each scene contains a single `GameObject` called **`UIViews`** with one **`UIDocument`** responsible for rendering the entire interface.  
- All UI screens (Play, Shop, Arsenal, Settings, etc.) exist as child **VisualElements**, keeping the UI centralized and efficient.

![UI Overview 1](https://img.itch.zone/aW1nLzIzODQwOTU3LnBuZw==/original/eDn73o.png)
![UI Overview 2](https://img.itch.zone/aW1nLzIzODQwOTY1LnBuZw==/original/gQKrXV.png)

Each screen inherits from a base **`UIView`** class, with logic handled by dedicated controllers for clean separation between visuals and behavior.

![UIView Diagram](https://img.itch.zone/aW1nLzIzODQwOTM3LnBuZw==/original/oL6CC8.png)

---

## 🧠 Tech Stack

- **Unity Engine**
- **C# (Azure Functions & Game Code)**
- **PlayFab Backend (Economy V2, Auth, Data Sync)**
- **Azure Cloud Functions**
- **Google Play Games Services**
- **Unity UI Toolkit**
- **Unity Mediation (LevelPlay)**
- **ScriptableObjects Architecture**

---

## 📱 Platforms

- **Android (Google Play Internal Testing)**  
- **Editor (Windows/Mac)**

---

## 🧰 Features Overview

| Category | Description |
|-----------|--------------|
| Backend | PlayFab + Azure Functions integration |
| UI | Modular UI Toolkit structure |
| Monetization | IAP, ads, and loot boxes |
| Cross-Platform | GPGS + PlayFab account linking |
| Cloud Architecture | Fully backend-driven design |

---

## 📄 License

This prototype is for **educational and portfolio purposes**.  
All rights to third-party assets, APIs, and services belong to their respective owners.

---

> **Clash of Steel** — a technical exploration of backend-driven mobile game design, built in Unity.
