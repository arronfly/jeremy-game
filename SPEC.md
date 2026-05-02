# Jeremy's Game - 歼灭团竞

## Project Overview
- **Project name**: Jeremy Game (jeremy-game)
- **Type**: 2D top-down team shooter game
- **Core functionality**: Team-based elimination game inspired by Peace Elite's Team Elimination mode
- **Target users**: Jeremy (13+ years old) and friends

## Game Mechanics

### Teams
- Red Team vs Blue Team
- 4v4 (1 human player + 3 bots per team)

### Controls
- **WASD**: Move
- **Mouse**: Aim
- **Left Click**: Shoot
- **R**: Reload

### Game Flow
1. Players spawn on their team's side
2. Eliminate enemy team members to score
3. First team to 25 kills wins
4. Respawn after 3 seconds when eliminated

### Weapons
- Assault Rifle: 30 rounds, moderate fire rate, medium damage
- Sniper: 5 rounds, slow fire rate, high damage

### Map
- Rectangular arena with obstacles
- Cover positions for tactical gameplay

## Technical Spec

### Stack
- HTML5 Canvas
- Vanilla JavaScript
- No external dependencies

### Architecture
- Game loop with requestAnimationFrame
- Entity-component pattern for players/bots
- Collision detection for bullets and obstacles

## Visual Style
- Military/tactical theme
- Dark map with team-colored players
- Bullet trails and hit effects
- Kill feed UI
