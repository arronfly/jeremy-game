# 歼灭团竞2 - 实施计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 构建一个写实风格的2D俯视角团队竞技游戏，包含多种武器、投掷物、药品，以及7局4胜制的比赛规则。

**Architecture:** 使用Phaser.js 3.x作为游戏引擎，程序化生成所有美术资源（角色、地图、特效），无需外部图片。游戏采用场景切换架构：BootScene → MenuScene → LoadScene → GameScene → EndScene。

**Tech Stack:** Phaser 3.60+, Vanilla JavaScript, HTML5 Canvas

---

## 阶段1: 项目结构与Phaser基础设置

### Task 1: 创建项目基础结构

**Files:**
- Create: `package.json`
- Create: `index.html`
- Create: `src/scenes/BootScene.js`
- Create: `src/scenes/MenuScene.js`
- Create: `src/scenes/LoadScene.js`
- Create: `src/scenes/GameScene.js`
- Create: `src/scenes/EndScene.js`
- Create: `src/config/game.js`

**Step 1: 创建package.json**

```json
{
  "name": "jeremy-game-2",
  "version": "1.0.0",
  "description": "Peace Elite Style Team Elimination Game",
  "scripts": {
    "start": "npx http-server . -p 8080",
    "test": "echo \"No tests configured\""
  },
  "dependencies": {
    "phaser": "^3.60.0"
  }
}
```

**Step 2: 创建index.html**

```html
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>歼灭团竞2</title>
    <script src="https://cdn.jsdelivr.net/npm/phaser@3.60.0/dist/phaser.min.js"></script>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { background: #1a1a2e; display: flex; justify-content: center; align-items: center; min-height: 100vh; }
        #game-container { position: relative; }
    </style>
</head>
<body>
    <div id="game-container"></div>
    <script type="module" src="src/main.js"></script>
</body>
</html>
```

**Step 3: 创建src/config/game.js**

```javascript
export const GAME_CONFIG = {
    width: 1600,
    height: 1200,
    pixelArt: false,
    backgroundColor: '#2d2d44',
    physics: {
        default: 'arcade',
        arcade: { debug: false }
    }
};

export const TEAM_CONFIG = {
    red: { color: 0xE74C3C, name: '红队', spawnX: 150 },
    blue: { color: 0x3498DB, name: '蓝队', spawnX: 1450 }
};

export const RULES = {
    winScore: 4,
    maxRounds: 7,
    respawnTime: 3000,
    playerSpeed: 150,
    playerHealth: 100
};
```

**Step 4: 创建src/main.js**

```javascript
import { GAME_CONFIG } from './config/game.js';
import BootScene from './scenes/BootScene.js';
import MenuScene from './scenes/MenuScene.js';
import LoadScene from './scenes/LoadScene.js';
import GameScene from './scenes/GameScene.js';
import EndScene from './scenes/EndScene.js';

const config = {
    ...GAME_CONFIG,
    scene: [BootScene, MenuScene, LoadScene, GameScene, EndScene]
};

const game = new Phaser.Game(config);
```

**Step 5: 创建基础场景文件**

创建空的场景占位符，后续填充。

**Step 6: Commit**

```bash
git add package.json index.html src/
git commit -m "feat: setup Phaser.js project structure with basic scenes"
```

---

### Task 2: 实现BootScene和资源生成系统

**Files:**
- Modify: `src/scenes/BootScene.js`

**Step 1: 创建Graphics资源生成器**

创建 `src/utils/SpriteGenerator.js` 用于程序化生成角色和物品纹理。

**Step 2: 实现BootScene加载画面**

**Step 3: Commit**

---

### Task 3: 实现MenuScene开始菜单

**Files:**
- Modify: `src/scenes/MenuScene.js`

**Step 1: 创建菜单UI**

**Step 2: 实现按钮交互**

**Step 3: Commit**

---

## 阶段2: 角色系统

### Task 4: 创建Player类（含动画和立体外观）

**Files:**
- Create: `src/entities/Player.js`
- Create: `src/utils/SpriteGenerator.js`

**Step 1: 定义角色结构**

角色包含：
- 头部（圆形，带高光）
- 躯干（椭圆形）
- 四肢（线条，根据朝向变化）
- 阴影（椭圆形黑色）

**Step 2: 实现动画系统**

使用Phaser的frames和animation系统实现6种动画状态。

**Step 3: 实现移动和碰撞**

**Step 4: Commit**

---

### Task 5: 实现Bot AI

**Files:**
- Create: `src/entities/Bot.js`

**Step 1: 创建Bot类，继承Player**

**Step 2: 实现简单AI行为**

- 巡逻
- 发现敌人时追击
- 使用武器

**Step 3: Commit**

---

## 阶段3: 武器系统

### Task 6: 创建武器配置和类

**Files:**
- Create: `src/weapons/Weapon.js`
- Create: `src/config/weapons.js`

**Step 1: 定义4种武器配置**

```javascript
export const WEAPONS = {
    assault_rifle: {
        name: '突击步枪',
        damage: 25,
        fireRate: 400,
        magazineSize: 30,
        maxAmmo: 120,
        bulletSpeed: 800,
        spread: 0.05
    },
    sniper: {
        name: '狙击枪',
        damage: 80,
        fireRate: 1500,
        magazineSize: 5,
        maxAmmo: 20,
        bulletSpeed: 1200,
        spread: 0
    },
    smg: {
        name: '冲锋枪',
        damage: 18,
        fireRate: 200,
        magazineSize: 35,
        maxAmmo: 140,
        bulletSpeed: 600,
        spread: 0.1
    },
    shotgun: {
        name: '散弹枪',
        damage: 15,
        fireRate: 800,
        magazineSize: 8,
        maxAmmo: 32,
        bulletSpeed: 500,
        spread: 0.3,
        pellets: 8
    }
};
```

**Step 2: 创建Weapon类**

**Step 3: 实现射击逻辑**

**Step 4: Commit**

---

## 阶段4: 投掷物系统

### Task 7: 实现投掷物

**Files:**
- Create: `src/items/Grenade.js`
- Create: `src/config/grenades.js`

**Step 1: 定义投掷物配置**

```javascript
export const GRENADES = {
    smoke: { name: '烟雾弹', cooldown: 3000, duration: 5000, radius: 150 },
    flash: { name: '闪光弹', cooldown: 3000, duration: 2000, radius: 200 },
    frag: { name: '手雷', cooldown: 3000, damage: 50, radius: 100 }
};
```

**Step 2: 实现抛物线投掷**

**Step 3: 实现各种效果**

**Step 4: Commit**

---

## 阶段5: 药品系统

### Task 8: 实现药品

**Files:**
- Create: `src/items/Medkit.js`
- Create: `src/config/medicals.js`

**Step 1: 定义药品配置**

```javascript
export const MEDICALLS = {
    first_aid_kit: { name: '急救包', cooldown: 10000, healAmount: 50, duration: 3000 },
    bandage: { name: '绷带', cooldown: 5000, healAmount: 10, duration: 1000 },
    energy_drink: { name: '能量饮料', cooldown: 8000, healAmount: 25, speedBoost: 0.2, duration: 10000 }
};
```

**Step 2: 实现使用逻辑**

**Step 3: Commit**

---

## 阶段6: 地图设计

### Task 9: 创建"假车库"风格地图

**Files:**
- Create: `src/maps/GarageMap.js`
- Create: `src/config/map.js`

**Step 1: 定义地图配置**

地图包含：
- 两个车库建筑（可进入）
- 多处围墙和掩体
- 窗户元素
- 出生点区域

**Step 2: 实现碰撞区域**

**Step 3: 实现窗户穿透逻辑**

**Step 4: Commit**

---

## 阶段7: 游戏规则和UI

### Task 10: 实现回合制规则系统

**Files:**
- Modify: `src/scenes/GameScene.js`
- Create: `src/managers/RoundManager.js`

**Step 1: 实现回合控制**

- 检测队伍全灭
- 计算得分
- 判定胜负

**Step 2: 实现7局4胜制逻辑**

**Step 3: 实现3:3决胜局**

**Step 4: Commit**

---

### Task 11: 实现HUD界面

**Files:**
- Create: `src/ui/HUD.js`
- Create: `src/ui/ScoreBoard.js`
- Create: `src/ui/KillFeed.js`

**Step 1: 创建HUD类**

显示：生命值、弹药、武器、投掷物、药品

**Step 2: 创建记分板**

**Step 3: 创建击杀信息**

**Step 4: Commit**

---

## 阶段8: 粒子特效和音效

### Task 12: 实现击中特效和血花

**Files:**
- Create: `src/effects/HitEffects.js`

**Step 1: 实现击中粒子**

**Step 2: 实现血花效果**

**Step 3: Commit**

---

## 验收测试

### Task 13: 完整测试

**Step 1: 启动游戏测试**

- 浏览器打开index.html
- 验证开始菜单显示
- 验证游戏加载完成

**Step 2: 测试核心功能**

- WASD移动
- 鼠标瞄准射击
- 武器切换和换弹
- 投掷物使用
- 药品使用

**Step 3: 验证比赛规则**

- 测试回合切换
- 测试得分系统
- 测试4分获胜

---

## 执行选项

**Plan complete and saved to `docs/plans/2026-05-02-peace-elite-game-implementation-plan.md`. Two execution options:**

**1. Subagent-Driven (this session)** - I dispatch fresh subagent per task, review between tasks, fast iteration

**2. Parallel Session (separate)** - Open new session with executing-plans, batch execution with checkpoints

**Which approach?**