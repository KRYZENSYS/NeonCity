# NeonCity — Assets & Plugins

> Where to source art, audio, plugins. All items listed are licensed for
> commercial use or are open-source. ALWAYS verify the license before
> shipping.

---

## 3D Models

### Characters
| Source | Pack | License |
|---|---|---|
| Synty Studios | POLYGON Cyberpunk | Royalty-free, commercial |
| Mr. P's | Cyberpunk Mega Pack | Royalty-free |
| ZENVA | Sci-Fi Characters | Royalty-free |

### Vehicles
| Source | Pack | License |
|---|---|---|
| Synty Studios | POLYGON Cars | Royalty-free |
| Polygon Island | Vehicle Pack | Royalty-free |

### Environment (City / Buildings)
| Source | License |
|---|---|
| Synty POLYGON World | Royalty-free |
| Kenney CC0 (city kit) | Public domain |
| Houdini procedural (self-authored) | — |

---

## Textures

- **AmbientCG** — CC0 (free, no attribution required)
- **Polyhaven** — CC0
- **Substance Source** — free tier available
- Use **Albedo + Normal + ORM** (Occlusion/Roughness/Metallic) packed

---

## HDR / Sky / Lighting

- **Polyhaven HDRIs** — CC0
- **Azure Sky** — URP-compatible shader (Asset Store)
- **Enviro 3** — weather + day/night cycle (Asset Store)

---

## Audio

### SFX
- **Sonniss GDC packs** — yearly royalty-free bundles
- **Freesound.org** — CC0/CC-BY
- **Epidemic Sound** — subscription, commercial-safe

### Music
- **Epidemic Sound** — best for variety
- **Composer's own tracks** (recommended for unique identity)

---

## Plugins (Asset Store / UPM)

| Plugin | Use | License |
|---|---|---|
| **Cinemachine** | Cameras | Built-in (Unity 6) |
| **Behavior Designer** | BT editor (optional — we have raw BT) | $95 |
| **Odin Inspector** | Dev tools | $55 |
| **DOTween Pro** | Tweening | $25 |
| **Easy Save** | Save/load helper | $35 |
| **Mirror** | Alt networking (we use NGO) | free |
| **Vivox** | Voice chat | Free tier + usage fees |
| **Addressables** | Asset loading | Built-in |
| **URP** | Rendering | Built-in |

---

## Shaders

- **Shader Graph** — built-in URP, all custom shaders authored in this
- **HDRP not used** — we stick to URP for mobile compatibility
- Custom **CyberNeon** shader (in repo at `Assets/Materials/CyberNeon.shadergraph`)
  handles emissive + scanline + flicker

---

## Performance

| Platform | Target FPS | Notes |
|---|---|---|
| PC (RTX 3060+) | 120 FPS | RT enabled |
| PC (GTX 1060) | 60 FPS | RT off |
| Console (PS5 / XSX) | 60-120 FPS | RT on, dynamic res |
| Mobile (mid-range) | 30-60 FPS | RT off, low textures |

See `Docs/PERFORMANCE.md` for detailed optimization settings.

---

## Budget

For a serious AAA release, budget ~$300k-$2M depending on team size.
For a portfolio/MVP version like this scaffold, expect **3-6 months full-time**
with a small team of 2-3 devs.