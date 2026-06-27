# NeonCity — Architecture

> High-level overview of the systems, their responsibilities, and how they talk to each other.

---

## 1. Layered Architecture

```
┌──────────────────────────────────────────────────┐
│                   UI LAYER                       │
│ HUD, menus, settings, shop, battle pass, chat    │
│ Built with UGUI + UI Toolkit (hybrid)            │
└──────────────────────────────────────────────────┘
                       │
┌──────────────────────────────────────────────────┐
│                GAMEPLAY LAYER                    │
│ Player, AI, Combat, Vehicles, Inventory, Missions│
│ Pure C# systems, scene-bound                     │
└──────────────────────────────────────────────────┘
                       │
┌──────────────────────────────────────────────────┐
│                NETWORKING LAYER                  │
│ Netcode for GameObjects, RPCs, NetworkBehaviours│
│ Wraps gameplay for sync; server-authoritative    │
└──────────────────────────────────────────────────┘
                       │
┌──────────────────────────────────────────────────┐
│                SERVICES LAYER                    │
│ SaveSystem, AudioService, InputService, etc.     │
│ Singleton-ish; injected via GameManager          │
└──────────────────────────────────────────────────┘
                       │
┌──────────────────────────────────────────────────┐
│                BACKEND LAYER                     │
│ Node.js Socket.IO + Firebase Auth/Firestore      │
│ Account, leaderboards, matchmaking, telemetry    │
└──────────────────────────────────────────────────┘
```

---

## 2. Bootstrap Flow

1. **GameManager** (DontDestroyOnLoad) is created in `Bootstrap.unity` scene.
2. **Services** are registered in order: Save → Input → Audio → Network → UI.
3. **Main Menu** scene loads → user logs in → selects mode (solo/multi).
4. **World Streaming** loads chunks via Addressables.
5. **Game loop** tick: input → simulation → network → render.

---

## 3. Multiplayer Model

- **Server-authoritative** for combat, physics, NPC AI.
- **Client-side prediction** for player movement.
- **Lag compensation** via Netcode's built-in rollback.
- **Anti-cheat** hooks: server validates damage, position deltas, fire rate.

See `Assets/Scripts/Networking/`.

---

## 4. AI Architecture

Each NPC owns:

- **Behavior Tree** (`Assets/Scripts/AI/BehaviorTree/`) — high-level goals (patrol/chase/combat).
- **Utility AI** — scores actions based on health/ammo/distance.
- **Perception** — vision cones, hearing radius, memory of last-known position.
- **FSM** (`Assets/Scripts/AI/FSM/`) — low-level states (idle/move/aim/fire/dead).

---

## 5. Rendering Pipeline

- **URP 17** with HDR enabled.
- **Lit shader** + custom **CyberNeon shader** (emissive + scanline).
- **Volume stack** for post-processing: bloom, vignette, chromatic aberration, film grain.
- **Ray Tracing** (PC only) — toggled in QualitySettings.
- **LOD groups** + GPU instancing for crowds.

---

## 6. Save System

- Local: `Application.persistentDataPath/save.dat` (binary, AES-encrypted).
- Cloud: Firebase Firestore doc per user.
- Sync layer reconciles local + cloud (last-write-wins + version vector).

---

## 7. Progression

```
XP ──► Level ──► Skill Points ──► Perks
                                          │
Battle Pass (100 tiers, free+premium track)
                                          │
Shop (cosmetics only — no gameplay P2W)
```

---

## 8. Build & Deploy

- **PC/Mac/Linux:** Unity build pipeline + GitHub Actions (`Tools/ci/`).
- **Mobile:** Addressables + IL2CPP + ARM64.
- **WebGL:** `Tools/build_webgl.sh` → uploaded to `web/` branch → Netlify/Vercel.
- **Steam:** Steamworks.NET integration.