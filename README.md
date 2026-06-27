# NeonCity — AAA Multiplayer Open-World FPS

> A professional-grade open-world cyberpunk shooter built with **Unity 6** (Universal Render Pipeline, Netcode for GameObjects, Behavior Trees).

---

## 🎮 Features

| Category | Capability |
|---|---|
| **Rendering** | URP + HDR + Ray Tracing (PC), realistic shadows, post-processing, dynamic weather |
| **World** | Open-world city, slums, ports, neon districts, highways, day/night cycle |
| **Gameplay** | FPS + 3rd Person switch, vehicles (cars, bikes, heli), parkour, combat |
| **Multiplayer** | Netcode for GameObjects, dedicated server, voice chat (Vivox/Dissonance), anti-cheat hooks |
| **AI** | Behavior Tree + Utility AI NPCs, FSM enemies, smart cover, hearing/vision |
| **Inventory** | Grid-based inventory, weapon modding, loot tables, hotbar |
| **Progression** | XP, levels, skills, daily/weekly missions, Battle Pass, shop, cosmetics |
| **Monetization** | IAP (Unity IAP), ads (Unity LevelPlay), battle pass, cosmetics-only (no P2W) |
| **Backend** | Node.js + Socket.IO authoritative server, Firebase Auth + Firestore saves |
| **Build targets** | Windows, macOS, Linux, Android, iOS, WebGL |

---

## 🛠 Tech Stack

- **Engine:** Unity 6 (URP, Netcode for GameObjects, Addressables, Burst, Jobs, Entities optional)
- **Language:** C# 9.0
- **Server:** Node.js 20 + Socket.IO 4 (authoritative)
- **Database:** Firebase Firestore + Realtime Database
- **Auth:** Firebase Auth (Google, Apple, anonymous)
- **Analytics:** Unity Analytics + Firebase Analytics
- **Crash:** Firebase Crashlytics
- **CI:** GitHub Actions (auto-build WebGL + APK)

---

## 📁 Project Structure

```
NeonCity/
├── Assets/
│   ├── Art/                  # Models, textures, materials
│   ├── Audio/                # SFX, music, voice
│   ├── Animations/           # Mecanim clips + controllers
│   ├── Materials/            # URP materials
│   ├── Prefabs/              # Reusable game objects
│   ├── Scenes/               # Game scenes (.unity)
│   ├── Scripts/              # All C# code
│   │   ├── Core/             # GameManager, bootstrap, services
│   │   ├── Player/           # FPS/3rd person controller, camera
│   │   ├── AI/               # Behavior tree, perception, FSM
│   │   ├── Combat/           # Weapons, projectiles, damage
│   │   ├── Vehicles/         # Car/bike/heli controllers
│   │   ├── Inventory/        # Items, grid, modding
│   │   ├── UI/               # HUD, menus, settings
│   │   ├── Networking/       # Netcode wrappers, RPCs
│   │   ├── Save/             # Save/load system
│   │   ├── Audio/            # Audio managers, footstep system
│   │   ├── Missions/         # Quest/storyline system
│   │   ├── Shop/             # Cosmetics, battle pass
│   │   └── Utils/            # Helpers, extensions
│   ├── Settings/             # URP settings, input actions
│   └── StreamingAssets/      # Loading assets
├── Packages/                 # Unity packages manifest
├── ProjectSettings/          # Unity project settings
├── Server/                   # Node.js multiplayer backend
├── Tools/                    # Build scripts, CI
└── Docs/                     # Architecture, design docs
```

---

## 🚀 Quick Start

1. **Clone:** `git clone https://github.com/KRYZENSYS/NeonCity.git`
2. **Open in Unity Hub** → add project → select Unity 6 (6000.0.x)
3. **Install packages** — see `Packages/manifest.json`
4. **Run server:** `cd Server && npm install && npm start`
5. **Press Play** in Unity

---

## 📜 License

MIT — see `LICENSE`. Models/audio from third-party packs require their own licenses (see `Docs/ASSETS.md`).

---

## 🎬 Cinematic Trailer (built with Unity Timeline)

See `Assets/Scenes/Trailer.unity` — built using Timeline + Cinemachine + URP Volume stack.

---

Built with ❤️ for AAA indie devs.