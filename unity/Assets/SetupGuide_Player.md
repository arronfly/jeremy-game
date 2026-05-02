# Player and Bot Prefab Setup Guide

This guide provides step-by-step instructions for setting up Player and Bot prefabs in Unity.

---

## Player Prefab Setup

### Step 1: Create Empty GameObject

1. Right-click in Hierarchy
2. Select **Create Empty** > **Create Empty GameObject**
3. Name it `Player`

### Step 2: Add Rigidbody2D

1. With Player selected, click **Add Component** in Inspector
2. Search for `Rigidbody2D` and add it
3. Configure:
   - **Gravity Scale**: `0`
   - **Collision Detection**: `Continuous`

### Step 3: Add CircleCollider2D

1. Click **Add Component** > search for `CircleCollider2D`
2. Configure:
   - **Radius**: `0.5`

### Step 4: Add SpriteRenderer

1. Click **Add Component** > search for `SpriteRenderer`
2. Configure:
   - **Sprite**: Select `Circle` (or import a circle sprite)
   - **Color**: Set to `Red` (`#E74C3C`)

### Step 5: Create FirePoint Child

1. Right-click on `Player` in Hierarchy
2. Select **Create Empty Child**
3. Name it `FirePoint`
4. Set position to `(1, 0, 0)`

### Step 6: Create Body Child

1. Right-click on `Player` in Hierarchy
2. Select **Create Empty Child**
3. Name it `Body`
4. This will be used for visual rotation (attach sprite here)

### Step 7: Add PlayerController Script

1. Click **Add Component** > search for `PlayerController`
2. Attach the script file

### Step 8: Add PlayerHealth Script

1. Click **Add Component** > search for `PlayerHealth`
2. Attach the script file

### Step 9: Add WeaponSystem Script

1. Click **Add Component** > search for `WeaponSystem`
2. Configure:
   - **Bullet Prefab**: Assign your bullet prefab
   - **Fire Point**: Drag the `FirePoint` child object here

### Step 10: Add GrenadeSystem Script

1. Click **Add Component** > search for `GrenadeSystem`
2. Attach the script file

### Step 11: Add MedkitSystem Script

1. Click **Add Component** > search for `MedkitSystem`
2. Attach the script file

### Step 12: Set Tag

1. In Inspector, click **Tag** dropdown
2. Select `Player`
3. If `Player` tag does not exist, create it in **Edit** > **Project Settings** > **Tags and Layers**

### Step 13: Set Layer

1. In Inspector, click **Layer** dropdown
2. Select `Player`
3. If `Player` layer does not exist, create it in **Edit** > **Project Settings** > **Tags and Layers**

---

## Bot Prefab Setup

The Bot prefab follows the same setup as Player with the following differences:

### Differences from Player:

| Property | Player | Bot |
|----------|--------|-----|
| Name | `Player` | `Bot` |
| Color | Red `#E74C3C` | Blue `#3498DB` |
| Tag | `Player` | `Bot` |

### Additional Step: Add BotAIAggressive Script

After completing all Player setup steps (1-13), add the Bot-specific AI:

1. Click **Add Component** > search for `BotAIAggressive`
2. Attach the script file

---

## Hierarchy Structure

### Player Hierarchy

```
Player                          <- (Tag: Player, Layer: Player)
├── Rigidbody2D                 <- Gravity Scale: 0, Continuous Collision
├── CircleCollider2D            <- Radius: 0.5
├── SpriteRenderer              <- Sprite: Circle, Color: #E74C3C (Red)
├── PlayerController            <- (Script)
├── PlayerHealth                <- (Script)
├── WeaponSystem                <- (Script, Fire Point: FirePoint)
├── GrenadeSystem               <- (Script)
├── MedkitSystem                <- (Script)
├── FirePoint                   <- (Child) Position: (1, 0, 0)
└── Body                        <- (Child) For visual rotation
```

### Bot Hierarchy

```
Bot                             <- (Tag: Bot, Layer: Player)
├── Rigidbody2D                 <- Gravity Scale: 0, Continuous Collision
├── CircleCollider2D             <- Radius: 0.5
├── SpriteRenderer               <- Sprite: Circle, Color: #3498DB (Blue)
├── PlayerController             <- (Script)
├── PlayerHealth                 <- (Script)
├── WeaponSystem                 <- (Script, Fire Point: FirePoint)
├── GrenadeSystem                <- (Script)
├── MedkitSystem                 <- (Script)
├── BotAIAggressive              <- (Script) [BOT ONLY]
├── FirePoint                    <- (Child) Position: (1, 0, 0)
└── Body                         <- (Child) For visual rotation
```

---

## ASCII Diagram: Full Hierarchy View

```
+-- Player/Bot (Root) --------------------------------+
|  Tag: Player/Bot                                     |
|  Layer: Player                                       |
|                                                      |
|  +-- Rigidbody2D ---------------------------------+  |
|  |  Gravity Scale: 0                               |  |
|  |  Collision Detection: Continuous                |  |
|  +-------------------------------------------------+  |
|                                                      |
|  +-- CircleCollider2D ----------------------------+  |
|  |  Radius: 0.5                                    |  |
|  +-------------------------------------------------+  |
|                                                      |
|  +-- SpriteRenderer ------------------------------+  |
|  |  Sprite: Circle                                |  |
|  |  Color: #E74C3C (Red) / #3498DB (Blue)         |  |
|  +-------------------------------------------------+  |
|                                                      |
|  +-- Scripts --------------------------------------+  |
|  |  - PlayerController                             |  |
|  |  - PlayerHealth                                 |  |
|  |  - WeaponSystem (with FirePoint reference)      |  |
|  |  - GrenadeSystem                                |  |
|  |  - MedkitSystem                                 |  |
|  |  - BotAIAggressive (Bot only)                   |  |
|  +-------------------------------------------------+  |
|                                                      |
|  +-- FirePoint (Child) ---------------------------+  |
|  |  Local Position: (1, 0, 0)                      |  |
|  +-------------------------------------------------+  |
|                                                      |
|  +-- Body (Child) ---------------------------------+  |
|  |  Purpose: Visual rotation container            |  |
|  +-------------------------------------------------+  |
+------------------------------------------------------+
```

---

## Creating the Prefab

After setting up either Player or Bot:

1. Drag the GameObject from Hierarchy into a folder in Project (e.g., `Assets/Prefabs/`)
2. The object will become a prefab (shown in blue text in Hierarchy)
3. Future changes to the prefab can be applied by selecting the prefab and clicking **Apply** in Inspector

---

## Required Scripts

Ensure the following scripts exist in your project before setting up:

- `PlayerController.cs`
- `PlayerHealth.cs`
- `WeaponSystem.cs`
- `GrenadeSystem.cs`
- `MedkitSystem.cs`
- `BotAIAggressive.cs` (Bot only)

---

## Quick Reference: Color Codes

| Entity | Hex Code | RGB |
|--------|----------|-----|
| Player | `#E74C3C` | (231, 76, 60) |
| Bot    | `#3498DB` | (52, 152, 219) |
