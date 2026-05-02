# Jeremy Game 2 (歼灭团竞2) - Unity Setup Guide

## Prerequisites

- Unity 2021.3 LTS or later (recommended)
- Unity Hub installed

---

## How to Create a New Project

1. Open Unity Hub
2. Click **New Project**
3. Select **3D (Built-in Render Pipeline)** as the template
4. Name your project (e.g., `JeremyGame2`)
5. Set the location to this `unity/` folder
6. Click **Create Project**

---

## How to Import Scripts

### Option 1: Copy Files Manually

1. Navigate to `Assets/Scripts/` in your Unity project
2. Copy all `.cs` files from this folder's `Scripts/` directory into your Unity project's `Assets/Scripts/` folder
3. In Unity, go to **Assets > Refresh** or wait for auto-refresh

### Option 2: Using Unity Package

1. In Unity, go to **Assets > Import Package > Custom Package**
2. Select the package file (if distributed as `.unitypackage`)
3. Click **Import**

### Scripts Available

| Script | Description |
|--------|-------------|
| `BotAI.cs` | Base bot AI behavior |
| `BotAIAggressive.cs` | Aggressive bot variant |
| `Bullet.cs` | Bullet projectile |
| `GameManager.cs` | Game state management |
| `GrenadeSystem.cs` | Grenade mechanics |
| `HUDManager.cs` | UI/HUD management |
| `MedkitSystem.cs` | Health pickup system |
| `PlayerController.cs` | Player movement & input |
| `PlayerHealth.cs` | Player health system |
| `RoundManager.cs` | Round-based game logic |
| `WeaponSystem.cs` | Weapon/gear system |
| `Test*.cs` | Unit tests for each system |

---

## How to Set Up Player Prefab (Step by Step)

### Step 1: Create Player GameObject

1. In Unity, right-click in Hierarchy
2. Select **3D Object > Capsule** (or use a character model)
3. Rename it to `Player`
4. Set transform:
   - Position: `(0, 1, 0)`
   - Scale: `(1, 2, 1)`

### Step 2: Add Components

1. Select the Player object
2. Click **Add Component** in Inspector
3. Add the following components:

#### Rigidbody
- **Mass:** `70` (for realistic movement)
- **Drag:** `4`
- **Angular Drag:** `0.05`
- **Use Gravity:** `true`
- **Is Kinematic:** `false`
- **Collision Detection:** `Continuous`

#### Collider
- If using Capsule, a CapsuleCollider is auto-added
- **Radius:** `0.3`
- **Height:** `2`
- **Center:** `(0, 1, 0)`

### Step 3: Add Custom Scripts

1. **PlayerController** - Handles WASD movement and mouse look
2. **PlayerHealth** - Manages health points and damage
3. **WeaponSystem** - Handles weapon switching and shooting
4. **HUDManager** - Updates player UI elements

### Step 4: Configure Tag and Layer

1. Select the Player object
2. In Inspector, click **Tag** dropdown
3. Select **Player** (or create one if missing)
4. Optionally set **Layer** to `Player`

### Step 5: Setup Camera (Child of Player)

1. Create a **Camera** as a child of Player
2. Position: `(0, 1.6, 0)` - eye level
3. Add **AudioListener** component
4. Optionally add **CinemachineBrain** for smoother camera

---

## Scene Hierarchy Setup

### Recommended Hierarchy Structure

```
Scene
├── GameManager (empty object)
│   └── GameManager.cs
├── RoundManager (empty object)
│   └── RoundManager.cs
├── HUDManager (Canvas + UI)
│   └── HUDManager.cs
├── Player
│   ├── PlayerController.cs
│   ├── PlayerHealth.cs
│   ├── WeaponSystem.cs
│   ├── Camera (child)
│   └── (other player components)
├── SpawnPoints
│   ├── SpawnPoint_Player1
│   ├── SpawnPoint_Player2
│   └── SpawnPoint_Bot1, ...
├── Environment
│   ├── Ground
│   ├── Walls
│   └── Obstacles
└── Bots (empty parent)
    └── Bot_1, Bot_2, ... (spawned at runtime)
```

### Setting Up Spawn Points

1. Create empty GameObjects for each spawn location
2. Position them around the map
3. Tag them appropriately (e.g., `SpawnPointPlayer`, `SpawnPointBot`)

### Setting Up Ground and Environment

1. Create a **Plane** (3D Object > Plane) as ground
2. Scale to fit your map size (e.g., `100 x 100`)
3. Add **Mesh Collider** if needed
4. Add floor/walls as needed for your game mode

---

## Next Steps

1. Configure `GameManager` in scene with references to other managers
2. Set up UI Canvas with HUD elements
3. Configure `RoundManager` with round settings
4. Test basic movement before adding combat systems
5. Build and test bot AI behavior

---

## Troubleshooting

**Scripts not compiling?**
- Check for missing namespaces
- Ensure Unity version compatibility
- Check Console for specific errors

**Prefab issues?**
- Right-click prefab > **Unpack Prefab** to edit
- Make sure to **Apply** changes to save

**Scene not loading?**
- Add scene to Build Settings (File > Build Settings > Add Open Scene)
