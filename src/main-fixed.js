const Phaser = window.Phaser;
const GAME_CONFIG = {
    width: 1600,
    height: 1200,
    pixelArt: false,
    backgroundColor: '#2d2d44',
    physics: { default: 'arcade', arcade: { debug: false } }
};
const TEAM_CONFIG = {
    red: { color: 0xE74C3C, name: '红队', spawnX: 150 },
    blue: { color: 0x3498DB, name: '蓝队', spawnX: 1450 }
};
const RULES = {
    winScore: 4,
    maxRounds: 7,
    respawnTime: 3000,
    playerSpeed: 150,
    playerHealth: 100
};
const WEAPONS = {
    assaultRifle: { name: '突击步枪', damage: 25, fireRate: 400, magSize: 30, maxReserve: 120, spread: 0.05 },
    sniper: { name: '狙击枪', damage: 80, fireRate: 1500, magSize: 5, maxReserve: 20, spread: 0 },
    smg: { name: '冲锋枪', damage: 18, fireRate: 200, magSize: 35, maxReserve: 140, spread: 0.1 },
    shotgun: { name: '散弹枪', damage: 15, fireRate: 800, magSize: 8, maxReserve: 32, spread: 0.3, pellets: 8 }
};

class BootScene extends Phaser.Scene {
    constructor() { super({ key: 'BootScene' }); }
    create() { this.scene.start('MenuScene'); }
}

class MenuScene extends Phaser.Scene {
    constructor() { super({ key: 'MenuScene' }); }
    create() {
        const { width, height } = this.sys.game.config;
        this.add.rectangle(width/2, height/2, width, height, 0x1a1a2e);
        this.add.text(width/2, 200, '歼灭团竞2', { fontSize: '72px', color: '#fff' }).setOrigin(0.5);
        this.add.text(width/2, 280, '写实风格团队竞技', { fontSize: '28px', color: '#aaa' }).setOrigin(0.5);
        const startBtn = this.add.rectangle(width/2, 450, 300, 80, 0x27AE60).setInteractive();
        this.add.text(width/2, 450, '开始游戏', { fontSize: '36px', color: '#fff' }).setOrigin(0.5);
        startBtn.on('pointerover', () => startBtn.setFillStyle(0x2ECC71));
        startBtn.on('pointerout', () => startBtn.setFillStyle(0x27AE60));
        startBtn.on('pointerdown', () => this.scene.start('LoadScene'));
        this.add.text(width/2, 600, 'WASD移动 | 鼠标瞄准 | 左键射击 | R换弹 | 1-4武器 | G投掷 | 7/8/9药品', {
            fontSize: '20px', color: '#888'
        }).setOrigin(0.5);
    }
}

class LoadScene extends Phaser.Scene {
    constructor() { super({ key: 'LoadScene' }); }
    create() {
        const { width, height } = this.sys.game.config;
        this.add.text(width/2, height/2 - 50, '加载资源中...', { fontSize: '36px', color: '#fff' }).setOrigin(0.5);
        const progress = this.add.graphics();
        const barWidth = 400, barHeight = 30, barX = width/2 - barWidth/2, barY = height/2 + 20;
        progress.fillStyle(0x27AE60, 1);
        progress.fillRect(barX, barY, barWidth, barHeight);
        progress.lineStyle(2, 0xffffff);
        progress.strokeRect(barX, barY, barWidth, barHeight);
        this.add.text(width/2, height/2 + 60, '加载完成！', { fontSize: '24px', color: '#27AE60' }).setOrigin(0.5);
        this.time.delayedCall(2000, () => this.scene.start('GameScene'));
    }
}

class GameScene extends Phaser.Scene {
    constructor() {
        super({ key: 'GameScene' });
        this.players = [];
        this.bots = [];
        this.wasd = null;
        this.bullets = null;
        this.lastBotShootTime = {};
    }
    create() {
        const { width, height } = this.sys.game.config;
        this.bullets = this.add.group();
        this.add.rectangle(width/2, height/2, width, height, 0x2d2d44);
        this.createMap();
        this.setupControls();
        this.spawnTeams();
        this.setupMouseInput();
        this.createHUD();
        this.redScore = 0;
        this.blueScore = 0;
    }
    createMap() {
        const { width, height } = this.sys.game.config;
        const graphics = this.add.graphics();
        graphics.lineStyle(1, 0x3d3d5c, 0.5);
        for (let x = 0; x < width; x += 50) graphics.lineBetween(x, 0, x, height);
        for (let y = 0; y < height; y += 50) graphics.lineBetween(0, y, width, y);
        this.add.rectangle(100, height/2, 200, 800, TEAM_CONFIG.red.color, 0.1);
        this.add.rectangle(width - 100, height/2, 200, 800, TEAM_CONFIG.blue.color, 0.1);
        graphics.lineStyle(2, 0xffffff, 0.2);
        graphics.lineBetween(width/2, 0, width/2, height);
        const covers = [
            { x: 400, y: 300, w: 80, h: 60 },
            { x: 800, y: 400, w: 120, h: 80 },
            { x: 1200, y: 300, w: 80, h: 60 },
            { x: 400, y: 900, w: 80, h: 60 },
            { x: 800, y: 800, w: 120, h: 80 },
            { x: 1200, y: 900, w: 80, h: 60 },
            { x: 200, y: 600, w: 60, h: 100 },
            { x: 1400, y: 600, w: 60, h: 100 }
        ];
        covers.forEach(c => this.add.rectangle(c.x, c.y, c.w, c.h, 0x555555));
        this.add.rectangle(600, 600, 200, 40, 0x444444);
        this.add.rectangle(1000, 600, 200, 40, 0x444444);
        this.coverObjects = covers;
    }
    setupControls() {
        this.wasd = this.input.keyboard.addKeys({
            up: Phaser.Input.Keyboard.KeyCodes.W,
            down: Phaser.Input.Keyboard.KeyCodes.S,
            left: Phaser.Input.Keyboard.KeyCodes.A,
            right: Phaser.Input.Keyboard.KeyCodes.D,
            reload: Phaser.Input.Keyboard.KeyCodes.R,
            weapon1: Phaser.Input.Keyboard.KeyCodes.ONE,
            weapon2: Phaser.Input.Keyboard.KeyCodes.TWO,
            weapon3: Phaser.Input.Keyboard.KeyCodes.THREE,
            weapon4: Phaser.Input.Keyboard.KeyCodes.FOUR
        });
    }
    setupMouseInput() {
        this.input.on('pointerdown', (pointer) => {
            if (pointer.leftButtonDown()) {
                const player = this.players.find(p => p.team === 'red' && !p.isBot);
                if (player && player.isAlive) {
                    this.playerShoot(player, pointer.x, pointer.y);
                }
            }
        });
    }
    playerShoot(player, targetX, targetY) {
        const weapon = player.currentWeapon || WEAPONS.assaultRifle;
        const now = this.time.now;
        if (player.lastShootTime && now - player.lastShootTime < weapon.fireRate) return;
        if (player.magAmmo !== undefined && player.magAmmo <= 0) {
            player.magAmmo = weapon.magSize;
            return;
        }
        player.lastShootTime = now;
        if (player.magAmmo !== undefined) player.magAmmo--;
        const angle = Phaser.Math.Angle.Between(player.x, player.y, targetX, targetY);
        player.bodyContainer.setRotation(angle);
        this.fireBullet(player.x, player.y, angle, weapon);
    }
    fireBullet(x, y, angle, weapon) {
        const bulletCount = weapon.pellets || 1;
        for (let i = 0; i < bulletCount; i++) {
            const spread = (Math.random() - 0.5) * weapon.spread;
            const spreadAngle = angle + spread;
            const bullet = this.add.circle(x + Math.cos(angle) * 24, y + Math.sin(angle) * 24, 4, 0xFFFF00);
            bullet.setVelocity(Math.cos(spreadAngle) * 600, Math.sin(spreadAngle) * 600);
            bullet.damage = weapon.damage;
            bullet.team = this.players.find(p => p.x === x && p.y === y)?.team;
            this.bullets.add(bullet);
            this.time.delayedCall(3000, () => {
                if (!bullet.destroyed) bullet.destroy();
            });
        }
    }
    spawnTeams() {
        const centerY = 600;
        // Red team - human player
        const human = this.createPlayer(TEAM_CONFIG.red.spawnX, centerY, 'red', true);
        this.players.push(human);
        // Red bots
        for (let i = 0; i < 3; i++) {
            const bot = this.createBot(TEAM_CONFIG.red.spawnX + 50, centerY + (i - 1) * 100, 'red');
            this.bots.push(bot);
            this.players.push(bot);
            this.lastBotShootTime[bot.uid] = 0;
        }
        // Blue team - bots
        for (let i = 0; i < 4; i++) {
            const bot = this.createBot(TEAM_CONFIG.blue.spawnX - 50, centerY + (i - 1) * 100, 'blue');
            this.bots.push(bot);
            this.players.push(bot);
            this.lastBotShootTime[bot.uid] = 0;
        }
    }
    createPlayer(x, y, team, isHuman) {
        const container = this.add.container(x, y);
        container.uid = Math.random().toString(36).substr(2, 9);
        const color = team === 'red' ? TEAM_CONFIG.red.color : TEAM_CONFIG.blue.color;
        container.add(this.add.ellipse(0, 16, 32, 12, 0x000000, 0.3));
        container.add(this.add.ellipse(0, 4, 16, 20, color));
        container.add(this.add.circle(0, -12, 10, 0xFFDBB4));
        container.add(this.add.ellipse(0, -16, 12, 6, color, 0.8));
        container.add(this.add.line(0, 0, -12, -2, -20, 8, color, 4));
        container.add(this.add.line(0, 0, 12, -2, 20, 8, color, 4));
        container.add(this.add.line(0, 0, -6, 14, -10, 24, color, 4));
        container.add(this.add.line(0, 0, 6, 14, 10, 24, color, 4));
        const weapon = this.add.line(0, 0, 0, 4, 24, 4, 0x333333, 3);
        container.add(weapon);
        container.setSize(48, 48);
        container.team = team;
        container.isBot = !isHuman;
        container.isAlive = true;
        container.health = RULES.playerHealth;
        container.maxHealth = RULES.playerHealth;
        container.speed = RULES.playerSpeed;
        container.radius = 20;
        container.bodyContainer = container;
        container.currentWeapon = WEAPONS.assaultRifle;
        container.magAmmo = 30;
        container.lastShootTime = 0;
        // Bot AI state
        container.aiState = 'wander';
        container.targetEnemy = null;
        container.wanderAngle = Math.random() * Math.PI * 2;
        container.wanderTimer = 0;
        return container;
    }
    createBot(x, y, team) {
        return this.createPlayer(x, y, team, false);
    }
    updateBotAI(bot, delta) {
        if (!bot.isAlive) return;
        const enemies = this.players.filter(p => p.team !== bot.team && p.isAlive);
        if (enemies.length === 0) {
            bot.aiState = 'wander';
        }
        // Find nearest enemy
        let nearestEnemy = null;
        let nearestDist = Infinity;
        enemies.forEach(enemy => {
            const dist = Phaser.Math.Distance.Between(bot.x, bot.y, enemy.x, enemy.y);
            if (dist < nearestDist) {
                nearestDist = dist;
                nearestEnemy = enemy;
            }
        });
        bot.targetEnemy = nearestEnemy;
        if (nearestEnemy && nearestDist < 600) {
            bot.aiState = 'attack';
            // Aim at enemy
            const angle = Phaser.Math.Angle.Between(bot.x, bot.y, nearestEnemy.x, nearestEnemy.y);
            bot.bodyContainer.setRotation(angle);
            // Move towards enemy if too far, away if too close
            if (nearestDist > 200) {
                bot.x += Math.cos(angle) * bot.speed * 0.008;
                bot.y += Math.sin(angle) * bot.speed * 0.008;
            } else if (nearestDist < 100) {
                bot.x -= Math.cos(angle) * bot.speed * 0.005;
                bot.y -= Math.sin(angle) * bot.speed * 0.005;
            }
            // Shoot at enemy
            const now = this.time.now;
            if (now - this.lastBotShootTime[bot.uid] > 500) {
                this.lastBotShootTime[bot.uid] = now;
                const weapon = bot.currentWeapon || WEAPONS.assaultRifle;
                this.fireBullet(bot.x, bot.y, angle, weapon);
            }
        } else {
            bot.aiState = 'wander';
            bot.wanderTimer += delta;
            if (bot.wanderTimer > 2000) {
                bot.wanderAngle = Math.random() * Math.PI * 2;
                bot.wanderTimer = 0;
            }
            // Wander around spawn area
            const spawnX = bot.team === 'red' ? TEAM_CONFIG.red.spawnX : TEAM_CONFIG.blue.spawnX;
            const dx = spawnX - bot.x;
            const dy = 600 - bot.y;
            const distToSpawn = Math.sqrt(dx * dx + dy * dy);
            if (distToSpawn > 200) {
                const angleToSpawn = Math.atan2(dy, dx);
                bot.x += Math.cos(angleToSpawn) * bot.speed * 0.004;
                bot.y += Math.sin(angleToSpawn) * bot.speed * 0.004;
            } else {
                bot.x += Math.cos(bot.wanderAngle) * bot.speed * 0.003;
                bot.y += Math.sin(bot.wanderAngle) * bot.speed * 0.003;
            }
        }
        // Keep in bounds
        bot.x = Phaser.Math.Clamp(bot.x, 50, 1550);
        bot.y = Phaser.Math.Clamp(bot.y, 50, 1150);
    }
    handleBullets() {
        this.bullets.getChildren().forEach(bullet => {
            if (!bullet.active) return;
            this.players.forEach(player => {
                if (!player.isAlive || player.isBot) return;
                const dist = Phaser.Math.Distance.Between(bullet.x, bullet.y, player.x, player.y);
                if (dist < player.radius + 4) {
                    player.health -= bullet.damage;
                    this.showDamage(player, bullet.damage);
                    if (player.health <= 0) {
                        player.isAlive = false;
                        player.setAlpha(0.3);
                    }
                    bullet.destroy();
                }
            });
            // Bot vs bot bullets handled separately
            this.bots.forEach(bot => {
                if (!bot.isAlive) return;
                const bulletTeam = bullet.team;
                if (bulletTeam && bulletTeam !== bot.team) {
                    const dist = Phaser.Math.Distance.Between(bullet.x, bullet.y, bot.x, bot.y);
                    if (dist < bot.radius + 4) {
                        bot.health -= bullet.damage;
                        if (bot.health <= 0) {
                            bot.isAlive = false;
                            bot.setAlpha(0.3);
                        }
                        bullet.destroy();
                    }
                }
            });
        });
    }
    showDamage(player, amount) {
        const text = this.add.text(player.x, player.y - 30, `-${amount}`, {
            fontSize: '16px', color: '#FF6B6B'
        }).setOrigin(0.5);
        this.tweens.add({
            targets: text, y: text.y - 40, alpha: 0, duration: 800,
            onComplete: () => text.destroy()
        });
    }
    createHUD() {
        const { width, height } = this.sys.game.config;
        this.add.rectangle(100, 30, 150, 40, TEAM_CONFIG.red.color, 0.8);
        this.redScoreText = this.add.text(100, 30, `${TEAM_CONFIG.red.name}: 0`, { fontSize: '20px', color: '#fff' }).setOrigin(0.5);
        this.add.rectangle(width - 100, 30, 150, 40, TEAM_CONFIG.blue.color, 0.8);
        this.blueScoreText = this.add.text(width - 100, 30, `${TEAM_CONFIG.blue.name}: 0`, { fontSize: '20px', color: '#fff' }).setOrigin(0.5);
        this.roundText = this.add.text(width/2, 30, '第1回合/7', { fontSize: '24px', color: '#fff' }).setOrigin(0.5);
        this.healthBar = this.add.rectangle(150, height - 50, 200, 20, 0x333333);
        this.healthFill = this.add.rectangle(50, height - 50, 200, 20, 0x27AE60);
        this.healthText = this.add.text(150, height - 80, 'HP: 100/100', { fontSize: '16px', color: '#fff' }).setOrigin(0.5);
    }
    updateHUD() {
        const player = this.players.find(p => p.team === 'red' && !p.isBot);
        if (player) {
            const pct = Math.max(0, player.health / player.maxHealth * 100);
            this.healthFill.setScale(pct / 100, 1);
            this.healthText.setText(`HP: ${Math.floor(player.health)}/${player.maxHealth}`);
            if (pct < 30) this.healthFill.setFillStyle(0xE74C3C);
            else if (pct < 60) this.healthFill.setFillStyle(0xF39C12);
            else this.healthFill.setFillStyle(0x27AE60);
        }
    }
    checkRoundEnd() {
        const redAlive = this.players.filter(p => p.team === 'red' && p.isAlive).length;
        const blueAlive = this.players.filter(p => p.team === 'blue' && p.isAlive).length;
        if (redAlive === 0 || blueAlive === 0) {
            if (redAlive === 0 && blueAlive > 0) {
                this.blueScore++;
                this.blueScoreText.setText(`${TEAM_CONFIG.blue.name}: ${this.blueScore}`);
            } else if (blueAlive === 0 && redAlive > 0) {
                this.redScore++;
                this.redScoreText.setText(`${TEAM_CONFIG.red.name}: ${this.redScore}`);
            }
            // Check win condition
            if (this.redScore >= 4 || this.blueScore >= 4) {
                const winner = this.redScore >= 4 ? '红队' : '蓝队';
                this.add.text(800, 400, `${winner}胜利!`, {
                    fontSize: '64px', color: '#ffff00'
                }).setOrigin(0.5);
            } else {
                // Respawn after delay
                this.time.delayedCall(3000, () => this.respawnAll());
            }
        }
    }
    respawnAll() {
        this.players.forEach(player => {
            player.isAlive = true;
            player.setAlpha(1);
            player.health = player.maxHealth;
            const spawnX = player.team === 'red' ? TEAM_CONFIG.red.spawnX : TEAM_CONFIG.blue.spawnX;
            player.x = spawnX + (Math.random() - 0.5) * 100;
            player.y = 400 + (Math.random() - 0.5) * 300;
        });
    }
    update(time, delta) {
        const player = this.players.find(p => p.team === 'red' && !p.isBot);
        if (player && player.isAlive) {
            if (this.wasd.up.isDown) player.y -= player.speed * 0.016;
            if (this.wasd.down.isDown) player.y += player.speed * 0.016;
            if (this.wasd.left.isDown) player.x -= player.speed * 0.016;
            if (this.wasd.right.isDown) player.x += player.speed * 0.016;
            const pointer = this.input.activePointer;
            const angle = Phaser.Math.Angle.Between(player.x, player.y, pointer.x, pointer.y);
            player.bodyContainer.setRotation(angle);
            player.x = Phaser.Math.Clamp(player.x, 50, 1550);
            player.y = Phaser.Math.Clamp(player.y, 50, 1150);
        }
        // Update bot AI
        this.bots.forEach(bot => {
            if (bot.isAlive) {
                this.updateBotAI(bot, delta);
            }
        });
        // Handle bullets
        this.handleBullets();
        // Update HUD
        this.updateHUD();
        // Check round end
        this.checkRoundEnd();
    }
}

const config = {
    ...GAME_CONFIG,
    scene: [BootScene, MenuScene, LoadScene, GameScene]
};

const game = new Phaser.Game(config);
window.game = game;