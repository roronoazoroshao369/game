# Puppet Pipeline — quick guide

Procedural rigid-limb puppet (Don't Starve / Cuphead style). Same animation core,
swap "skin" via folder. Demo runs **without art** via auto-generated colored-rectangle skeleton.

> Layered docs: scope + tradeoffs trong [`AI_PROMPTS.md`](AI_PROMPTS.md), animation math
> trong `Scripts/Vfx/PuppetAnimController.cs` xml-doc, và asset-side practical rules trong
> [`DST_RIG_ASSET_GUIDE.md`](DST_RIG_ASSET_GUIDE.md).

---

## TL;DR

1. **No art?** Bootstrap → 13-joint colored-rectangle skeleton di chuyển → demo runs ngay.
2. **Drop PNG vào `Art/Characters/{id}/{E|N|S}/`** → re-Bootstrap → puppet uses real art.
3. **New character = folder + tunings struct.** Zero animation code thay đổi.
4. Nếu art nhìn rời rạc / paper-doll → đọc [`DST_RIG_ASSET_GUIDE.md`](DST_RIG_ASSET_GUIDE.md) trước khi regen.

---

## 1. Demo mode (no art)

`Tools → Wilderness Cultivation → Bootstrap Default Scene` (clean checkout):

- BootstrapWizard call `CharacterArtImporter.TryLoadCharacterSpriteSet("player")` → folder rỗng → `null`
- Fallback: `PuppetPlaceholderGenerator.EnsureSpriteSet("player", includeTail=false)` →
  bake 10 PNGs vào `Sprites/puppet/player/` (head, torso, 2 arm, 2 forearm, 2 leg, 2 shin)
- Same hierarchy build path → `PuppetAnimController` tunings (walkFreq=3Hz, armSwing=28°)
- Run scene → blue-robe stickman walking visible: arm + forearm swing, leg + shin bend khi back-swing

Wolf + FoxSpirit identical flow (gray + orange palette, includeTail=true).

Output mỗi character ~10-11 PNGs trong `Sprites/puppet/{id}/`. Idempotent — chạy lại không
overwrite.

---

## 2. Replace placeholder với real art

Drop PNG vào `Art/Characters/{id}/`:

### Flat layout (side-only)

```
Art/Characters/player/
├── head.png
├── torso.png
├── arm_left.png  arm_right.png
├── forearm_left.png  forearm_right.png
├── leg_left.png  leg_right.png
└── shin_left.png  shin_right.png
```

→ East-only puppet, West auto-flip via spriteRoot.localScale.

### Multi-direction layout (L3+ DST style)

```
Art/Characters/player/
├── E/   head.png torso.png ...   ← side facing right
├── N/   head.png torso.png ...   ← back view
└── S/   head.png torso.png ...   ← front view (W auto = flip E)
```

→ PuppetAnimController switches sprite refs per direction theo velocity angle (PR J).

`Tools → Wilderness Cultivation → Bootstrap Default Scene` lại → `TryLoadCharacterSpriteSet`
finds real PNGs → returns non-null → placeholder generator skipped. Re-build prefab dùng real
art. **No code change.**

Required parts: `head.png` + `torso.png` (others optional). Missing optional parts → joint
bị skip (controller null-guard).

---

## 3. Add new character (3 steps, zero animation code)

### Step 1 — Create folder

```bash
mkdir -p Assets/_Project/Art/Characters/my_new_mob
touch Assets/_Project/Art/Characters/my_new_mob/.gitkeep
```

### Step 2 — Add palette case

`Assets/_Project/Scripts/Core/PuppetPlaceholderSpec.cs` → `PaletteFor()`:

```csharp
case "my_new_mob":
    return new Palette
    {
        skin = new Color(0.9f, 0.6f, 0.4f),
        tunic = new Color(0.7f, 0.3f, 0.2f),     // signature body color
        trousers = new Color(0.5f, 0.2f, 0.1f),
        shin = new Color(0.3f, 0.1f, 0.05f),
        tail = new Color(0.6f, 0.25f, 0.15f),
    };
```

### Step 3 — Add prefab build trong BootstrapWizard

```csharp
static GameObject BuildMyNewMobPrefab(/*...*/)
{
    var go = new GameObject("MyNewMob");
    var puppetSet = CharacterArtImporter.TryLoadCharacterSpriteSet("my_new_mob", placeholderHeightPx: 32)
        ?? BuildPlaceholderSpriteSet("my_new_mob", includeTail: true);
    BuildPuppetHierarchy(go, puppetSet.EastSprites, sortingOrderBase: 3, /*...out transforms*/);
    var puppet = go.AddComponent<PuppetAnimController>();
    // Wire transforms...
    puppet.walkFrequency = 4f;
    puppet.armSwingDeg = 30f;
    puppet.legSwingDeg = 22f;
    WirePuppetMultiDirSprites(puppet, puppetSet);
    // AI + collider + Rigidbody2D as usual...
    return SaveAsPrefab(go, $"{PrefabsDir}/MyNewMob.prefab");
}
```

Bootstrap → puppet skeleton sinh ra với palette mới. Drop real PNG sau cũng work y hệt.

> **Animation logic không thay đổi.** Walk swing, lunge, crouch, squash, direction snap, elbow
> bend, knee bend — tất cả live trong `PuppetAnimController` data-driven via tunings public
> fields. New character = data (tunings + palette + folder), not code (FSM/state).

---

## 4. Tunings reference

Mỗi character override các fields sau trên `PuppetAnimController` instance trong BootstrapWizard
build. Defaults (xem `PuppetAnimController.cs`) phù hợp humanoid Player; mob nimble cần higher
freq + larger swing.

| Field | Player | Wolf | FoxSpirit | Note |
|---|---|---|---|---|
| `walkFrequency` | 3 Hz | 4.5 Hz | 5.5 Hz | Step rate. Mob faster than humanoid. |
| `armSwingDeg` | 28° | 32° | 35° | Shoulder rotation amplitude. |
| `legSwingDeg` | 18° | 28° | 26° | Hip rotation amplitude. |
| `tailSwayDeg` | n/a | 16° | 24° | Tail sway (humanoid skip via includeTail=false). |
| `referenceSpeed` | default | 2.5 m/s | 2.6 m/s | Speed at which swings reach max amplitude. |
| `elbowBendDeg` | 40° | 40° | 40° | Forearm bend at attack peak (PR K). |
| `kneeCrouchBendDeg` | 35° | 35° | 35° | Shin bend khi crouching. |
| `kneeWalkBendDeg` | 12° | 12° | 12° | Shin bend khi walking back-swing. |

---

## 5. Architecture

```
PuppetPlaceholderSpec (Core/, runtime)
  ├── Palette per character ID
  ├── Rect dimensions per role
  └── DefaultRoles enumeration

PuppetPlaceholderGenerator (Editor/, IO wrapper)
  └── EnsureSpriteSet(id) → bake PNG + load Sprite

CharacterArtImporter (Editor/)
  └── TryLoadCharacterSpriteSet(id) → real PNG → CharacterSpriteSet | null

BootstrapWizard.BuildPlayerPrefab / BuildWolfPrefab / BuildFoxSpiritPrefab
  └── puppetSet = TryLoad(...) ?? BuildPlaceholderSpriteSet(...)
       └── BuildPuppetHierarchy → AddComponent<PuppetAnimController>
            └── wire transforms + override tunings + WirePuppetMultiDirSprites

PuppetAnimController (Scripts/Vfx/, runtime)
  ├── Walk: sin(t × walkFrequency) → arm/leg/head bob
  ├── Attack: ComputeLungeArmAngle bell-curve, elbow bend at peak
  ├── Crouch: torso scale Y + shin knee bend
  ├── Direction: ComputeDirectionFromVelocity → snap with hysteresis 8°
  └── Sprite swap per direction (E/N/S, W=flip(E))
```

Adding character touches **2 data files** (PuppetPlaceholderSpec.PaletteFor + BootstrapWizard
build method). Animation logic untouched.

---

## 6. Common gotchas

- **Sprite pivot:** placeholder uses center pivot. Real art author should set pivot ở joint anchor
  (head → bottom, arm → top) trong Sprite Editor cho cleaner rotation. Center pivot still
  works — animation looks slightly cartoony but visible.
- **Multi-dir partial:** if `Art/Characters/player/E/` exists nhưng `N/` không, controller
  fallback East khi velocity hướng N (no error). Add N/ folder để full coverage.
- **Re-bootstrap idempotency:** safe to re-run. Real PNG luôn priority; placeholder chỉ generate
  khi missing. Won't overwrite real art ever.
- **Forearm/Shin missing parts:** drop arm_left.png nhưng KHÔNG forearm_left.png → arm rigid
  (no elbow bend). Generator default includes both → demo always shows full L2 motion.
