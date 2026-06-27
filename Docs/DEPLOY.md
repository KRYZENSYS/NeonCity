# Deploying NeonCity

This document walks through every step required to ship NeonCity to players —
from local development to Steam, Play Store, App Store and WebGL.

---

## 1. Local development

### Prerequisites
| Tool | Version |
|---|---|
| Unity Hub | latest |
| Unity Editor | **6000.0.x** (Unity 6) |
| Node.js | 20 LTS |
| Git LFS | latest |
| .NET SDK | 8.0 |
| Visual Studio / Rider | 2022 / 2024 |

### Setup

```bash
git clone https://github.com/KRYZENSYS/NeonCity.git
cd NeonCity
git lfs pull
```

Open the folder in **Unity Hub** → add project → select Unity 6.
First import will install ~80 packages — go grab a coffee.

### Run the multiplayer server

```bash
cd Server
cp .env.example .env
# Edit .env, add Firebase credentials (see step 3)
npm install
npm run dev
```

Server runs on `http://localhost:4000`. Open in browser — you should see
`{ ok: true, rooms: 0 }` on `/health`.

### Play the game

Open `Assets/Scenes/Bootstrap.unity` in Unity → press Play.

In another instance, click **Host** or **Join** with `127.0.0.1:7777`.

---

## 2. Building

### Local CLI build

```bash
# Linux64 build
Unity -batchmode -quit -executeMethod BuildScript.Linux64

# WebGL build
Unity -batchmode -quit -executeMethod BuildScript.WebGL

# Android APK
Unity -batchmode -quit -executeMethod BuildScript.Android
```

Output goes to `build/<platform>/NeonCity[.exe]`.

### CI build

`.github/workflows/ci.yml` builds **Linux64 + WebGL** on every push to `main`.
Artifacts are uploaded for download in the workflow run.

To add Android/iOS builds, add macOS / Linux containers with the Android / iOS
modules installed (see `unityci/editor` images).

---

## 3. Firebase setup (auth, Firestore, analytics)

1. Create a Firebase project at https://console.firebase.google.com
2. Enable **Authentication** (Google + Anonymous).
3. Enable **Firestore** (production mode) and **Realtime Database**.
4. Project settings → Service accounts → **Generate new private key**.
5. Minify the JSON to one line, paste into `Server/.env` as `FIREBASE_CREDENTIALS`.
6. Unity-side: install `com.unity.services.authentication` and `com.unity.services.cloudsave`
   (already in `Packages/manifest.json`).
7. In Unity, open **Services** window, link your project.

---

## 4. Steam release

1. Register on [Steamworks](https://partner.steamgames.com) — pay $100 once.
2. Install **Steamworks.NET** NuGet package.
3. Add Steamworks SDK to `Assets/Plugins/Steamworks/`.
4. Implement `ISteamManager` (`SteamAppId`, achievements, DLC).
5. Configure in-app purchases to use Steam wallet.
6. Upload build via `SteamPipe`:
   ```bash
   steamcmd +login myuser +run_app_build_http ...
   ```
7. Submit for review.

---

## 5. Google Play (Android)

1. Build APK or AAB:
   ```bash
   Unity -batchmode -quit -executeMethod BuildScript.Android
   ```
2. **Player Settings → Publishing Settings** → create keystore, sign APK.
3. Enable IL2CPP, ARM64 only, optimize for mobile (set Texture Quality = ASTC).
4. Create Play Console account ($25 once).
5. Upload AAB, fill store listing, submit.

For mobile-specific optimizations see `Docs/PERFORMANCE.md` (texture compression,
shader LOD, GPU instancing, etc.).

---

## 6. WebGL deploy

WebGL build outputs to `build/WebGL/`.

```bash
# Compress for fast loading
zip -r build/WebGL.zip build/WebGL

# Deploy to Netlify
netlify deploy --dir=build/WebGL --prod
```

Configure `NetworkService.BackendApi.BaseUrl` to point at your live server.

---

## 7. Voice chat

Voice uses **WebRTC** for low-latency P2P. Signaling runs through the Node.js
server (`voice:offer/answer/ice` events).

For >4 players per room, swap P2P for an **SFU** (mediasoup or LiveKit). The
server already routes the signaling messages — swap only the audio routing.

---

## 8. Anti-cheat

Server validates:
- Max fire rate (30 RPS per player)
- Max damage per tick
- Position deltas (lag-compensated speed check)

Violations are flagged in Firestore `anticheat` collection for review.
Consider integrating Easy Anti-Cheat or BattlEye for shipping.

---

## 9. Monetization

- **Unity IAP** for cosmetics (already wired in `ShopService.cs`)
- **Ad mediation** via Unity LevelPlay (formerly ironSource)
- **Battle Pass** via `BattlePassService.cs`
- **Subscriptions** (VIP) using Unity IAP subscriptions

Remember: cosmetics-only. No P2W.

---

## 10. Roadmap to v1.0

- [ ] Open-world terrain (WorldComposer / Houdini heightmaps)
- [ ] Vehicle traffic system (procedural AI drivers)
- [ ] 30 main missions + 20 side missions
- [ ] 6 player progression trees
- [ ] Day/night cycle + weather (Enviro 3 / Azure Sky)
- [ ] 3D spatial audio (Steam Audio / Meta XR Audio)
- [ ] Console certification (Sony / Microsoft TRC)