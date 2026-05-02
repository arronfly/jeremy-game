import Player from '../entities/Player.js';
import Bot from '../entities/Bot.js';
import { TEAM_CONFIG, RULES } from '../config/game.js';
import { CONTROLS } from '../config/controls.js';
import { HitEffects } from '../effects/HitEffects.js';
import { GarageMap } from '../maps/GarageMap.js';

export default class GameScene extends Phaser.Scene {
    constructor() {
        super({ key: 'GameScene' });

        this.players = [];
        this.bots = [];
        this.cursors = null;
        this.wasd = null;
        this.pointer = null;
    }

    create() {
        const { width, height } = this.sys.game.config;

        // Create the garage-style map
        this.garageMap = new GarageMap(this);
        this.garageMap.create();

        // Initialize controls
        this.setupControls();

        // Spawn teams using map spawn positions
        this.spawnTeams();

        // Setup mouse input
        this.setupMouseInput();

        // Display team info
        this.createHUD();

        // Initialize particle effects
        this.hitEffects = new HitEffects(this);

        // Store collision objects reference for bullet collision detection
        this.collisionObjects = this.garageMap.getCollisionObjects();
    }

    createMapElements() {
        const { width, height } = this.sys.game.config;

        // Add some simple cover objects (placeholder)
        const coverPositions = [
            { x: 400, y: 300, w: 80, h: 60 },
            { x: 800, y: 400, w: 120, h: 80 },
            { x: 1200, y: 300, w: 80, h: 60 },
            { x: 400, y: 900, w: 80, h: 60 },
            { x: 800, y: 800, w: 120, h: 80 },
            { x: 1200, y: 900, w: 80, h: 60 },
            { x: 200, y: 600, w: 60, h: 100 },
            { x: 1400, y: 600, w: 60, h: 100 }
        ];

        coverPositions.forEach(pos => {
            // Create simple gray cover rectangles
            const cover = this.add.rectangle(pos.x, pos.y, pos.w, pos.h, 0x555555);
        });

        // Center obstacles
        this.add.rectangle(600, 600, 200, 40, 0x444444);
        this.add.rectangle(1000, 600, 200, 40, 0x444444);
    }

    setupControls() {
        // Keyboard
        this.wasd = this.input.keyboard.addKeys({
            up: Phaser.Input.Keyboard.KeyCodes.W,
            down: Phaser.Input.Keyboard.KeyCodes.S,
            left: Phaser.Input.Keyboard.KeyCodes.A,
            right: Phaser.Input.Keyboard.KeyCodes.D,
            reload: Phaser.Input.Keyboard.KeyCodes.R,
            weapon1: Phaser.Input.Keyboard.KeyCodes.ONE,
            weapon2: Phaser.Input.Keyboard.KeyCodes.TWO,
            weapon3: Phaser.Input.Keyboard.KeyCodes.THREE,
            weapon4: Phaser.Input.Keyboard.KeyCodes.FOUR,
            grenade: Phaser.Input.Keyboard.KeyCodes.G,
            medkit: Phaser.Input.Keyboard.KeyCodes.SEVEN,
            bandage: Phaser.Input.Keyboard.KeyCodes.EIGHT,
            energy: Phaser.Input.Keyboard.KeyCodes.NINE
        });

        // Mouse wheel for grenade switching
        this.input.on('wheel', (pointer, game, deltaX, deltaY) => {
            const humanPlayer = this.players.find(p => p.team === 'red' && !p.isBot);
            if (humanPlayer && humanPlayer.isAlive) {
                humanPlayer.switchGrenade();
            }
        });

        // Mouse
        this.pointer = this.input.activePointer;
    }

    setupMouseInput() {
        // Left click to shoot
        this.input.on('pointerdown', (pointer) => {
            if (pointer.leftButtonDown()) {
                // Find the local player (first red team player is human)
                const localPlayer = this.players.find(p => p.team === 'red' && !p.isBot);
                if (localPlayer && localPlayer.isAlive) {
                    localPlayer.shoot(pointer.x, pointer.y);
                }
            }

            // Right click to throw grenade (hold for distance)
            if (pointer.rightButtonDown()) {
                const localPlayer = this.players.find(p => p.team === 'red' && !p.isBot);
                if (localPlayer && localPlayer.isAlive) {
                    if (!localPlayer.isThrowing) {
                        localPlayer.startGrenadeThrow();
                    }
                }
            }
        });

        // Right click release to throw
        this.input.on('pointerup', (pointer) => {
            if (pointer.rightButtonReleased()) {
                const localPlayer = this.players.find(p => p.team === 'red' && !p.isBot);
                if (localPlayer && localPlayer.isThrowing) {
                    localPlayer.releaseGrenade(pointer.x, pointer.y);
                }
            }
        });
    }

    spawnTeams() {
        // Get spawn positions from the map
        const redSpawnPositions = this.garageMap.getSpawnPositions('red');
        const blueSpawnPositions = this.garageMap.getSpawnPositions('blue');

        // Red team: 1 human player + 3 bots
        // Human player at first spawn position
        const redHumanPos = redSpawnPositions[0];
        const humanPlayer = new Player(this, redHumanPos.x, redHumanPos.y, 'red', this.hitEffects);
        this.players.push(humanPlayer);

        // 3 bots
        for (let i = 0; i < 3; i++) {
            const pos = redSpawnPositions[i + 1];
            const bot = new Bot(this, pos.x, pos.y, 'red', this.hitEffects);
            bot.setPatrolPoints([
                { x: 150, y: 400 },
                { x: 350, y: 600 },
                { x: 150, y: 800 }
            ]);
            this.bots.push(bot);
            this.players.push(bot);
        }

        // Blue team: 1 human player + 3 bots
        // Human player (AI controlled for now - can be switched to local)
        const blueHumanPos = blueSpawnPositions[0];
        const blueHuman = new Player(this, blueHumanPos.x, blueHumanPos.y, 'blue', this.hitEffects);
        this.players.push(blueHuman);

        // 3 bots
        for (let i = 0; i < 3; i++) {
            const pos = blueSpawnPositions[i + 1];
            const bot = new Bot(this, pos.x, pos.y, 'blue', this.hitEffects);
            bot.setPatrolPoints([
                { x: 1450, y: 400 },
                { x: 1250, y: 600 },
                { x: 1450, y: 800 }
            ]);
            this.bots.push(bot);
            this.players.push(bot);
        }
    }

    getEnemies(team) {
        return this.players.filter(p => p.team !== team && p.isAlive);
    }

    getEnemiesForTeam(team) {
        return this.getEnemies(team);
    }

    createHUD() {
        const { width, height } = this.sys.game.config;

        // Red team indicator
        this.add.rectangle(100, 30, 150, 40, 0xE74C3C, 0.8);
        this.add.text(100, 30, `${TEAM_CONFIG.red.name}: 4`, {
            fontSize: '20px',
            color: '#fff'
        }).setOrigin(0.5);

        // Blue team indicator
        this.add.rectangle(width - 100, 30, 150, 40, 0x3498DB, 0.8);
        this.add.text(width - 100, 30, `${TEAM_CONFIG.blue.name}: 0`, {
            fontSize: '20px',
            color: '#fff'
        }).setOrigin(0.5);

        // Round indicator
        this.add.text(width / 2, 30, '第1回合/7', {
            fontSize: '24px',
            color: '#fff'
        }).setOrigin(0.5);

        // Player health bars
        this.createHealthBar(this.players[0], 50, height - 100);
    }

    createHealthBar(player, x, y) {
        const barWidth = 200;
        const barHeight = 20;

        // Background
        const bg = this.add.rectangle(x, y, barWidth, barHeight, 0x333333);

        // Health fill
        const healthFill = this.add.rectangle(x - barWidth/2 + barWidth/2, y, barWidth, barHeight, 0x27AE60);

        // Label
        this.add.text(x, y - 15, `${player.teamName} - 100/100`, {
            fontSize: '16px',
            color: '#fff'
        }).setOrigin(0.5);

        return { bg, healthFill };
    }

    update() {
        // Handle player movement
        this.handlePlayerMovement();

        // Handle bullet collisions
        this.handleBulletCollisions();

        // Update all players
        const humanPlayer = this.players.find(p => p.team === 'red' && !p.isBot);

        this.players.forEach(player => {
            if (player.update) {
                player.update(this.pointer);
            }

            // Update grenades for human player
            if (player === humanPlayer && player.updateGrenades) {
                player.updateGrenades(this.game.loop.delta);
            }
        });
    }

    handlePlayerMovement() {
        const humanPlayer = this.players.find(p => p.team === 'red' && !p.isBot);
        if (!humanPlayer || !humanPlayer.isAlive) return;

        // Movement
        if (this.wasd.up.isDown) {
            humanPlayer.moveUp();
        }
        if (this.wasd.down.isDown) {
            humanPlayer.moveDown();
        }
        if (this.wasd.left.isDown) {
            humanPlayer.moveLeft();
        }
        if (this.wasd.right.isDown) {
            humanPlayer.moveRight();
        }

        // Reload
        if (Phaser.Input.Keyboard.JustDown(this.wasd.reload)) {
            humanPlayer.reload();
        }

        // Weapon switching (1-4)
        if (Phaser.Input.Keyboard.JustDown(this.wasd.weapon1)) {
            humanPlayer.switchWeapon(1);
        }
        if (Phaser.Input.Keyboard.JustDown(this.wasd.weapon2)) {
            humanPlayer.switchWeapon(2);
        }
        if (Phaser.Input.Keyboard.JustDown(this.wasd.weapon3)) {
            humanPlayer.switchWeapon(3);
        }
        if (Phaser.Input.Keyboard.JustDown(this.wasd.weapon4)) {
            humanPlayer.switchWeapon(4);
        }

        // Grenade switch (G key)
        if (Phaser.Input.Keyboard.JustDown(this.wasd.grenade)) {
            humanPlayer.switchGrenade();
        }

        // Medical items (7, 8, 9 keys)
        if (Phaser.Input.Keyboard.JustDown(this.wasd.medkit)) {
            humanPlayer.useMedkit('firstAidKit');
        }
        if (Phaser.Input.Keyboard.JustDown(this.wasd.bandage)) {
            humanPlayer.useMedkit('bandage');
        }
        if (Phaser.Input.Keyboard.JustDown(this.wasd.energy)) {
            humanPlayer.useMedkit('energyDrink');
        }

        // Keep player in bounds
        humanPlayer.x = Phaser.Math.Clamp(humanPlayer.x, 50, 1550);
        humanPlayer.y = Phaser.Math.Clamp(humanPlayer.y, 50, 1150);
    }

    handleBulletCollisions() {
        const humanPlayer = this.players.find(p => p.team === 'red' && !p.isBot);
        if (!humanPlayer) return;

        // Get bullets and enemy sprites
        const bullets = humanPlayer.getBullets();
        if (!bullets) return;

        const enemies = this.players.filter(p => p.team === 'blue' && p.isAlive);

        bullets.getChildren().forEach(bullet => {
            if (!bullet.active) return;

            // Check collision with enemies
            enemies.forEach(enemy => {
                const distance = Phaser.Math.Distance.Between(
                    bullet.x, bullet.y,
                    enemy.x, enemy.y
                );

                if (distance < enemy.radius + 4) {
                    // Create blood effect at impact point
                    const angle = Phaser.Math.Angle.Between(enemy.x, enemy.y, bullet.x, bullet.y);
                    this.hitEffects.createBloodEffect(bullet.x, bullet.y, Phaser.Math.RadToDeg(angle));
                    enemy.takeDamage(bullet.damage);
                    bullet.destroy();
                    return;
                }
            });

            if (!bullet.active) return;

            // Check bullet collision with map obstacles using GarageMap collision detection
            const collision = this.garageMap.checkBulletCollision(bullet.x, bullet.y);
            if (collision.hit && collision.type !== this.garageMap.COLLISION_TYPES.WINDOW) {
                // Create spark effect at impact point
                const angle = Phaser.Math.RadToDeg(Math.atan2(bullet.velocity.y, bullet.velocity.x));
                this.hitEffects.createSparkEffect(bullet.x, bullet.y, angle);
                bullet.destroy();
            }
        });
    }
}