# 假车库 (Fake Garage) Map Design Document

## Overview

- **Map Name**: 假车库 (Fake Garage)
- **World Size**: 1600 x 1200 world units (standard 4:3 ratio for tactical shooters)
- **Coordinate System**: Origin (0, 0) at map center; X increases right, Y increases up
- **Theme**: Urban tactical environment with two opposing garage structures

---

## ASCII Map Layout

```
================================================================================
                        假车库 (FAKE GARAGE) - 1600x1200
================================================================================

    Y: 600
    +----------------------------------------------------------------+
    |                                                                |
    |   [RED SPAWN ZONE]                    [BLUE SPAWN ZONE]       |
    |   x: -6 to -4                         x: 4 to 6                |
    |                                                                |
    |        +------+                              +------+         |
    |        |      |                              |      |         |
    |        | GAR- |    [W3]        [W4]          | GAR- |         |
    |        | AGE  |                              | AGE  |         |
    |        |  A   |                              |  B   |         |
    |   [W1] +------+                              +------+ [W2]    |
    |                                                                |
    |                                                                |
    |                    +---------------+                          |
    |                    |   CENTRAL     |                          |
    |                    |   COVER       |     [COVER]              |
    |                    +---------------+                          |
    |         [COVER]                        [COVER]                 |
    |                                                                |
    |   +------+                              +------+               |
    |   |GARAGE|     [COVER]    [COVER]      |GARAGE|               |
    |   |  C   |                              |  D   |               |
    |   +------+                              +------+               |
    |        [W5]        [W6]                              [W7]     |
    |                                                                |
    |   [RED SPAWN ZONE]                    [BLUE SPAWN ZONE]       |
    |   x: -6 to -4                         x: 4 to 6                |
    |                                                                |
    +----------------------------------------------------------------+
    Y: -600

================================================================================
```

---

## Zone Definitions

### Spawn Zones

| Zone | Position X | Position Y | Size (W x H) | Description |
|------|------------|------------|--------------|-------------|
| Red Spawn | x: -6 to -4 | y: -400 to 400 | 200 x 800 | Left side, red team |
| Blue Spawn | x: 4 to 6 | y: -400 to 400 | 200 x 800 | Right side, blue team |

---

## Building Specifications

### Garage A (Top-Left)

```
Position: x: -500 to -300, y: 100 to 300
Size: 200 x 200
```

| Element | Position (x,y) | Size (W x H) | Collision | Sprite/Material |
|---------|----------------|--------------|-----------|-----------------|
| North Wall | (-500, 300) | 200 x 20 | solid | Concrete wall (gray) |
| South Wall | (-500, 100) | 200 x 20 | solid | Concrete wall (gray) |
| West Wall | (-500, 100) | 20 x 200 | solid | Concrete wall (gray) |
| East Wall (lower) | (-300, 100) | 20 x 100 | solid | Concrete wall (gray) |
| East Wall (upper) | (-300, 220) | 20 x 80 | solid | Concrete wall (gray) |
| Garage Door | (-300, 180) | 20 x 60 | none | Open (no collision) |
| Window A1 | (-400, 260) | 40 x 40 | none | Glass (shoot-through) |
| Cover Internal | (-400, 200) | 60 x 60 | cover | Metal crate (brown) |

### Garage B (Top-Right)

```
Position: x: 300 to 500, y: 100 to 300
Size: 200 x 200
```

| Element | Position (x,y) | Size (W x H) | Collision | Sprite/Material |
|---------|----------------|--------------|-----------|-----------------|
| North Wall | (300, 300) | 200 x 20 | solid | Concrete wall (gray) |
| South Wall | (300, 100) | 200 x 20 | solid | Concrete wall (gray) |
| East Wall | (480, 100) | 20 x 200 | solid | Concrete wall (gray) |
| West Wall (lower) | (300, 100) | 20 x 100 | solid | Concrete wall (gray) |
| West Wall (upper) | (300, 220) | 20 x 80 | solid | Concrete wall (gray) |
| Garage Door | (300, 180) | 20 x 60 | none | Open (no collision) |
| Window B1 | (400, 260) | 40 x 40 | none | Glass (shoot-through) |
| Cover Internal | (400, 200) | 60 x 60 | cover | Metal crate (brown) |

### Garage C (Bottom-Left)

```
Position: x: -500 to -300, y: -300 to -100
Size: 200 x 200
```

| Element | Position (x,y) | Size (W x H) | Collision | Sprite/Material |
|---------|----------------|--------------|-----------|-----------------|
| North Wall | (-500, -100) | 200 x 20 | solid | Concrete wall (gray) |
| South Wall | (-500, -300) | 200 x 20 | solid | Concrete wall (gray) |
| West Wall | (-500, -300) | 20 x 200 | solid | Concrete wall (gray) |
| East Wall (lower) | (-300, -300) | 20 x 100 | solid | Concrete wall (gray) |
| East Wall (upper) | (-300, -180) | 20 x 80 | solid | Concrete wall (gray) |
| Garage Door | (-300, -220) | 20 x 60 | none | Open (no collision) |
| Window C1 | (-400, -140) | 40 x 40 | none | Glass (shoot-through) |
| Cover Internal | (-400, -200) | 60 x 60 | cover | Metal crate (brown) |

### Garage D (Bottom-Right)

```
Position: x: 300 to 500, y: -300 to -100
Size: 200 x 200
```

| Element | Position (x,y) | Size (W x H) | Collision | Sprite/Material |
|---------|----------------|--------------|-----------|-----------------|
| North Wall | (300, -100) | 200 x 20 | solid | Concrete wall (gray) |
| South Wall | (300, -300) | 200 x 20 | solid | Concrete wall (gray) |
| East Wall | (480, -300) | 20 x 200 | solid | Concrete wall (gray) |
| West Wall (lower) | (300, -300) | 20 x 100 | solid | Concrete wall (gray) |
| West Wall (upper) | (300, -180) | 20 x 80 | solid | Concrete wall (gray) |
| Garage Door | (300, -220) | 20 x 60 | none | Open (no collision) |
| Window D1 | (400, -140) | 40 x 40 | none | Glass (shoot-through) |
| Cover Internal | (400, -200) | 60 x 60 | cover | Metal crate (brown) |

---

## Cover Positions (Outside Buildings)

| Cover ID | Position (x,y) | Size (W x H) | Collision | Sprite/Material | Notes |
|----------|----------------|--------------|-----------|-----------------|-------|
| COVER_01 | (-200, 350) | 80 x 80 | cover | Sandbag (tan) | Top mid-left |
| COVER_02 | (200, 350) | 80 x 80 | cover | Sandbag (tan) | Top mid-right |
| COVER_03 | (-150, 0) | 100 x 60 | cover | Concrete barrier (gray) | Mid left |
| COVER_04 | (150, 0) | 100 x 60 | cover | Concrete barrier (gray) | Mid right |
| COVER_05 | (-200, -350) | 80 x 80 | cover | Sandbag (tan) | Bottom mid-left |
| COVER_06 | (200, -350) | 80 x 80 | cover | Sandbag (tan) | Bottom mid-right |
| COVER_07 | (-100, 150) | 50 x 50 | cover | Metal barrel (red) | Near Garage A |
| COVER_08 | (100, 150) | 50 x 50 | cover | Metal barrel (red) | Near Garage B |
| COVER_09 | (-100, -150) | 50 x 50 | cover | Metal barrel (red) | Near Garage C |
| COVER_10 | (100, -150) | 50 x 50 | cover | Metal barrel (red) | Near Garage D |

---

## Central Obstacles

| Obstacle ID | Position (x,y) | Size (W x H) | Collision | Sprite/Material | Notes |
|-------------|----------------|--------------|-----------|-----------------|-------|
| CTR_01 | (0, 200) | 300 x 40 | solid | Concrete pillar (gray) | Top center |
| CTR_02 | (0, -200) | 300 x 40 | solid | Concrete pillar (gray) | Bottom center |
| CTR_03 | (0, 0) | 120 x 120 | cover | Crate stack (brown) | Center of map |
| CTR_04 | (-50, 80) | 40 x 40 | cover | Wooden pallet (brown) | Slightly off-center left |
| CTR_05 | (50, -80) | 40 x 40 | cover | Wooden pallet (brown) | Slightly off-center right |

---

## Windows (Shoot-Through)

| Window ID | Position (x,y) | Size (W x H) | Collision | Sprite/Material | Notes |
|-----------|----------------|--------------|-----------|-----------------|-------|
| W1 | (-300, 400) | 60 x 40 | none | Broken glass frame | Top-left wall |
| W2 | (300, 400) | 60 x 40 | none | Broken glass frame | Top-right wall |
| W3 | (-150, 400) | 60 x 40 | none | Broken glass frame | Top-center-left |
| W4 | (150, 400) | 60 x 40 | none | Broken glass frame | Top-center-right |
| W5 | (-300, -400) | 60 x 40 | none | Broken glass frame | Bottom-left wall |
| W6 | (0, -400) | 60 x 40 | none | Broken glass frame | Bottom-center |
| W7 | (300, -400) | 60 x 40 | none | Broken glass frame | Bottom-right wall |

---

## Boundary Walls

| Wall | Position (x,y) | Size (W x H) | Collision | Sprite/Material |
|------|----------------|--------------|-----------|-----------------|
| North Boundary | (0, 580) | 1600 x 40 | solid | Concrete wall (dark gray) |
| South Boundary | (0, -580) | 1600 x 40 | solid | Concrete wall (dark gray) |
| West Boundary | (-780, 0) | 40 x 1200 | solid | Concrete wall (dark gray) |
| East Boundary | (780, 0) | 40 x 1200 | solid | Concrete wall (dark gray) |

---

## Spawn Point Coordinates

### Red Team (4 Players) - Left Side

| Player | Position (x, y) | Notes |
|--------|------------------|-------|
| Red_01 | (-5, 300) | Top-left area |
| Red_02 | (-5, 100) | Garage A spawn |
| Red_03 | (-5, -100) | Garage C spawn |
| Red_04 | (-5, -300) | Bottom-left area |

### Blue Team (4 Players) - Right Side

| Player | Position (x, y) | Notes |
|--------|------------------|-------|
| Blue_01 | (5, 300) | Top-right area |
| Blue_02 | (5, 100) | Garage B spawn |
| Blue_03 | (5, -100) | Garage D spawn |
| Blue_04 | (5, -300) | Bottom-right area |

---

## Collision Types Reference

| Type | Description | Can Shoot Through | Can Walk Through |
|------|-------------|-------------------|------------------|
| solid | Full collision | No | No |
| cover | Half collision (crouch cover) | No | Yes (walking) |
| none | No collision | Yes | Yes |

---

## Sprite/Material Summary

| Asset Name | Type | Suggested Color/Texture |
|------------|------|-------------------------|
| Concrete Wall | solid | Gray (#666666) with noise |
| Broken Glass | none (window) | Light blue with crack pattern |
| Metal Crate | cover | Brown (#8B4513) with metal bands |
| Sandbag | cover | Tan (#D2B48C) |
| Concrete Barrier | cover | Gray (#808080) |
| Metal Barrel | cover | Red (#B22222) |
| Crate Stack | cover | Brown (#A0522D) |
| Wooden Pallet | cover | Light brown (#DEB887) |
| Ground | none | Asphalt/concrete texture |

---

## Map Balance Notes

1. **Symmetrical Layout**: The map is designed with near-perfect symmetry to ensure fair gameplay
2. **Spawn Distance**: Spawn zones are equidistant from center (500 units)
3. **Garage Advantage**: Each team has two garages providing defensive cover
4. **Central Contest**: The center area (CTR_03) is the primary tactical objective
5. **Flanking Routes**: Multiple cover positions allow for flanking maneuvers
6. **Window Tactical**: Shoot-through windows allow for ranged support from garages

---

## Implementation Checklist

- [ ] Create ground plane (1600 x 1200)
- [ ] Place boundary walls
- [ ] Construct Garage A, B, C, D with all walls
- [ ] Add windows to buildings
- [ ] Place all cover objects
- [ ] Place central obstacles
- [ ] Configure spawn zones
- [ ] Set collision types for all objects
- [ ] Apply materials/sprites
- [ ] Test spawn point placement
- [ ] Verify sight lines through windows
