# 歼灭团竞2 - Unity 项目规格

## 游戏概述
- **游戏名**: 歼灭团竞2 (Annihilation Team Battle)
- **类型**: 2D俯视角团队射击游戏
- **玩法**: 4v4团队生存赛，类似和平精英歼灭团竞

## 游戏规则
- 红队 vs 蓝队，4v4
- 一队全灭，另一队得分
- 率先得到4分的队伍获胜
- 共7局，3:3时进行决胜局

## 控制方案
- **WASD**: 移动
- **鼠标**: 瞄准
- **左键**: 射击
- **R**: 换弹
- **1-4**: 切换武器
- **G**: 投掷物
- **7/8/9**: 药品

## 武器系统
| 武器 | 伤害 | 弹夹 | 射速 | 特点 |
|------|------|------|------|------|
| 突击步枪 | 25 | 30发 | 中 | 全能 |
| 狙击枪 | 80 | 5发 | 慢 | 远距离 |
| 冲锋枪 | 18 | 35发 | 快 | 近距离 |
| 散弹枪 | 15x8 | 8发 | 中 | 范围伤害 |

## 投掷物
- 烟雾弹: 遮蔽视野
- 闪光弹: 致盲敌人
- 手雷: 范围伤害

## 药品
- 急救包: 恢复50点
- 绷带: 恢复10点
- 能量饮料: 恢复25点+移速加成

## 角色属性
- 生命值: 100
- 移动速度: 适中（原版60%）
- 半径: 20像素

## 场景设计

### 地图: "假车库"
- 红队出生点（左）
- 蓝队出生点（右）
- 中央有车库建筑
- 多处掩体和围墙
- 窗户可穿透射击

### 场景尺寸
- 1600 x 1200 像素

## 视觉设计
- 写实风格角色
- 头、躯干、四肢可见
- 阴影效果
- 击中特效（血花、火花）
- 子弹轨迹

## Unity 项目结构
```
Assets/
├── Scenes/
│   ├── MainMenu.unity
│   ├── GameScene.unity
│   └── EndScreen.unity
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── BotAI.cs
│   │   ├── WeaponSystem.cs
│   │   └── HealthSystem.cs
│   ├── Game/
│   │   ├── GameManager.cs
│   │   ├── RoundManager.cs
│   │   └── ScoreManager.cs
│   ├── Items/
│   │   ├── Grenade.cs
│   │   └── Medkit.cs
│   └── UI/
│       ├── HUD.cs
│       └── ScoreBoard.cs
├── Prefabs/
│   ├── Player.prefab
│   ├── Bullet.prefab
│   ├── Grenade.prefab
│   └── Cover.prefab
└── Materials/
```

## 建议的Unity组件
- 2D Collider (for collision)
- Rigidbody2D (for physics)
- Sprite Renderer (for visuals)
- Canvas (for UI)
- Event System (for input)

## 后续扩展
- 联网对战功能
- 更多武器
- 更多地图
- 皮肤系统