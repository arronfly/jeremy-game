import { TEAM_CONFIG } from '../config/game.js';
import { GRENADES } from '../config/grenades.js';
import { MEDICALS } from '../config/medicals.js';

/**
 * HUDManager - Manages all Heads-Up Display elements
 * Creates and updates score, health, ammo, grenade, and kill feed displays
 */
export class HUDManager {
    constructor(scene) {
        this.scene = scene;
        this.elements = {};

        // Reference to local player (for getting stats)
        this.localPlayer = null;

        // UI styling constants
        this.colors = {
            red: TEAM_CONFIG.red.color,
            blue: TEAM_CONFIG.blue.color,
            background: 0x000000,
            healthGreen: 0x27AE60,
            healthYellow: 0xF39C12,
            healthRed: 0xE74C3C,
            white: 0xFFFFFF,
            darkBg: 0x222222
        };

        this.create();
    }

    setLocalPlayer(player) {
        this.localPlayer = player;
    }

    create() {
        const { width, height } = this.scene.sys.game.config;
        this.screenWidth = width;
        this.screenHeight = height;

        this.createScoreDisplay();
        this.createRoundDisplay();
        this.createHealthBar();
        this.createAmmoDisplay();
        this.createWeaponDisplay();
        this.createGrenadeDisplay();
        this.createMedkitDisplay();
        this.createKillFeed();
        this.createMiniMap();
    }

    createScoreDisplay() {
        // Red team score - left side
        const redBg = this.scene.add.rectangle(90, 35, 160, 50, this.colors.red, 0.9)
            .setOrigin(0.5)
            .setStrokeStyle(2, 0xFFFFFF, 0.5);

        this.elements.redScoreText = this.scene.add.text(90, 35, '红队 0', {
            fontSize: '22px',
            fontFamily: 'Arial',
            color: '#FFFFFF',
            fontStyle: 'bold'
        }).setOrigin(0.5);

        // Blue team score - right side
        const blueBg = this.scene.add.rectangle(this.screenWidth - 90, 35, 160, 50, this.colors.blue, 0.9)
            .setOrigin(0.5)
            .setStrokeStyle(2, 0xFFFFFF, 0.5);

        this.elements.blueScoreText = this.scene.add.text(this.screenWidth - 90, 35, '蓝队 0', {
            fontSize: '22px',
            fontFamily: 'Arial',
            color: '#FFFFFF',
            fontStyle: 'bold'
        }).setOrigin(0.5);
    }

    createRoundDisplay() {
        // Round indicator - center top
        this.elements.roundText = this.scene.add.text(this.screenWidth / 2, 35, '第1回合/7', {
            fontSize: '26px',
            fontFamily: 'Arial',
            color: '#FFFFFF',
            fontStyle: 'bold'
        }).setOrigin(0.5);

        // Background for round text
        this.scene.add.rectangle(this.screenWidth / 2, 35, 180, 50, this.colors.darkBg, 0.7)
            .setOrigin(0.5)
            .setStrokeStyle(1, 0xFFFFFF, 0.3);
    }

    createHealthBar() {
        const y = this.screenHeight - 80;

        // Health bar background
        this.elements.healthBg = this.scene.add.rectangle(120, y, 220, 30, this.colors.darkBg, 0.8)
            .setOrigin(0.5)
            .setStrokeStyle(2, 0xFFFFFF, 0.3);

        // Health bar fill
        this.elements.healthFill = this.scene.add.rectangle(120, y, 210, 22, this.colors.healthGreen)
            .setOrigin(0.5);

        // Health text
        this.elements.healthText = this.scene.add.text(120, y, '100/100', {
            fontSize: '18px',
            fontFamily: 'Arial',
            color: '#FFFFFF',
            fontStyle: 'bold'
        }).setOrigin(0.5);

        // Label
        this.scene.add.text(120, y - 25, '生命值', {
            fontSize: '14px',
            fontFamily: 'Arial',
            color: '#AAAAAA'
        }).setOrigin(0.5);
    }

    createAmmoDisplay() {
        const y = this.screenHeight - 40;

        // Current magazine / reserve ammo display
        this.elements.ammoBg = this.scene.add.rectangle(this.screenWidth / 2, y, 200, 40, this.colors.darkBg, 0.8)
            .setOrigin(0.5)
            .setStrokeStyle(2, 0xFFFFFF, 0.3);

        this.elements.ammoText = this.scene.add.text(this.screenWidth / 2, y, '30 / 120', {
            fontSize: '24px',
            fontFamily: 'Arial',
            color: '#FFFFFF',
            fontStyle: 'bold'
        }).setOrigin(0.5);

        this.scene.add.text(this.screenWidth / 2, y - 30, '弹药', {
            fontSize: '14px',
            fontFamily: 'Arial',
            color: '#AAAAAA'
        }).setOrigin(0.5);
    }

    createWeaponDisplay() {
        const y = this.screenHeight - 80;
        const x = this.screenWidth - 150;

        // Weapon name background
        this.elements.weaponBg = this.scene.add.rectangle(x, y, 180, 40, this.colors.darkBg, 0.8)
            .setOrigin(0.5)
            .setStrokeStyle(2, 0xFFFFFF, 0.3);

        this.elements.weaponText = this.scene.add.text(x, y, '突击步枪', {
            fontSize: '18px',
            fontFamily: 'Arial',
            color: '#FFFFFF',
            fontStyle: 'bold'
        }).setOrigin(0.5);

        this.scene.add.text(x, y - 30, '当前武器', {
            fontSize: '14px',
            fontFamily: 'Arial',
            color: '#AAAAAA'
        }).setOrigin(0.5);

        // Weapon slots indicator
        this.createWeaponSlots();
    }

    createWeaponSlots() {
        const startX = this.screenWidth - 280;
        const y = this.screenHeight - 25;

        this.elements.weaponSlots = [];

        for (let i = 0; i < 4; i++) {
            const x = startX + i * 35;

            const slot = this.scene.add.rectangle(x, y, 28, 28, this.colors.darkBg, 0.8)
                .setOrigin(0.5)
                .setStrokeStyle(1, i === 0 ? 0xFFD700 : 0x888888, i === 0 ? 1 : 0.5);

            const number = this.scene.add.text(x, y, `${i + 1}`, {
                fontSize: '14px',
                fontFamily: 'Arial',
                color: i === 0 ? '#FFD700' : '#AAAAAA'
            }).setOrigin(0.5);

            this.elements.weaponSlots.push({ slot, number, active: i === 0 });
        }
    }

    createGrenadeDisplay() {
        const y = this.screenHeight - 160;

        // Grenade label
        this.scene.add.text(50, y, '投掷物', {
            fontSize: '14px',
            fontFamily: 'Arial',
            color: '#AAAAAA'
        }).setOrigin(0.5);

        // 3 grenade slots
        this.elements.grenadeSlots = [];

        for (let i = 0; i < 3; i++) {
            const x = 50 + i * 40;
            const slot = this.scene.add.rectangle(x, y + 30, 32, 32, this.colors.darkBg, 0.8)
                .setOrigin(0.5)
                .setStrokeStyle(2, 0x888888, 0.5);

            // Grenade type icons (circles with colors)
            const types = ['smoke', 'flash', 'frag'];
            const grenadeColors = {
                smoke: 0xAAAAAA,
                flash: 0xFFFF00,
                frag: 0xFF6600
            };

            const grenadeIcon = this.scene.add.circle(x, y + 30, 10, grenadeColors[types[i]], 0.9);

            this.elements.grenadeSlots.push({
                slot,
                icon: grenadeIcon,
                type: types[i],
                active: i === 0
            });
        }
    }

    createMedkitDisplay() {
        const y = this.screenHeight - 100;

        // Medkit label
        this.scene.add.text(50, y, '药品', {
            fontSize: '14px',
            fontFamily: 'Arial',
            color: '#AAAAAA'
        }).setOrigin(0.5);

        // 3 medkit slots
        this.elements.medkitSlots = [];

        const medkitTypes = ['firstAidKit', 'bandage', 'energyDrink'];
        const medkitLabels = ['急救包', '绷带', '饮料'];

        for (let i = 0; i < 3; i++) {
            const x = 50 + i * 40;
            const slot = this.scene.add.rectangle(x, y + 30, 32, 32, this.colors.darkBg, 0.8)
                .setOrigin(0.5)
                .setStrokeStyle(2, 0x27AE60, 0.5);

            const label = this.scene.add.text(x, y + 30, `${i + 7}`, {
                fontSize: '12px',
                fontFamily: 'Arial',
                color: '#27AE60'
            }).setOrigin(0.5);

            this.elements.medkitSlots.push({
                slot,
                label,
                type: medkitTypes[i],
                displayName: medkitLabels[i]
            });
        }
    }

    createKillFeed() {
        // Kill feed on right side
        this.elements.killFeedContainer = this.scene.add.container(this.screenWidth - 20, 100);

        this.elements.killFeedText = [];

        // Background for kill feed
        const feedBg = this.scene.add.rectangle(0, 0, 200, 150, this.colors.darkBg, 0.6)
            .setOrigin(1, 0)
            .setAlpha(0.7);
        this.elements.killFeedContainer.add(feedBg);
    }

    createMiniMap() {
        const mapSize = 150;
        const x = this.screenWidth - mapSize / 2 - 20;
        const y = this.screenHeight - mapSize / 2 - 20;

        // Mini-map background
        this.elements.miniMapBg = this.scene.add.rectangle(x, y, mapSize, mapSize, this.colors.darkBg, 0.8)
            .setOrigin(0.5)
            .setStrokeStyle(2, 0xFFFFFF, 0.5);

        this.elements.miniMap = this.scene.add.graphics();
        this.elements.miniMap.setPosition(x - mapSize / 2, y - mapSize / 2);

        this.elements.miniMap = this.scene.add.graphics();
        this.elements.miniMapBg.add(this.elements.miniMap);

        // Map border
        this.scene.add.rectangle(x, y, mapSize, mapSize, 0x000000, 0)
            .setOrigin(0.5)
            .setStrokeStyle(2, 0xFFFFFF, 0.5);

        // Label
        this.scene.add.text(x, y - mapSize / 2 - 10, '小地图', {
            fontSize: '12px',
            fontFamily: 'Arial',
            color: '#AAAAAA'
        }).setOrigin(0.5);
    }

    update() {
        if (!this.localPlayer) {
            // Find local player if not set
            this.localPlayer = this.scene.players.find(p => p.team === 'red' && !p.isBot);
        }

        if (!this.localPlayer) return;

        this.updateHealth();
        this.updateAmmo();
        this.updateWeapon();
        this.updateGrenade();
        this.updateMedkit();
        this.updateMiniMap();
    }

    updateHealth() {
        if (!this.localPlayer) return;

        const health = this.localPlayer.health;
        const maxHealth = this.localPlayer.maxHealth;
        const percentage = health / maxHealth;

        // Update health bar fill
        this.elements.healthFill.setSize(210 * percentage, 22);

        // Change color based on health
        if (percentage > 0.6) {
            this.elements.healthFill.setFillStyle(this.colors.healthGreen);
        } else if (percentage > 0.3) {
            this.elements.healthFill.setFillStyle(this.colors.healthYellow);
        } else {
            this.elements.healthFill.setFillStyle(this.colors.healthRed);
        }

        // Update text
        this.elements.healthText.setText(`${Math.max(0, Math.floor(health))}/${maxHealth}`);
    }

    updateAmmo() {
        if (!this.localPlayer) return;

        const weapon = this.localPlayer.weaponInventory.getCurrentWeapon();
        if (weapon) {
            const magAmmo = weapon.getMagAmmo();
            const reserveAmmo = weapon.getReserveAmmo();
            this.elements.ammoText.setText(`${magAmmo} / ${reserveAmmo}`);
        }
    }

    updateWeapon() {
        if (!this.localPlayer) return;

        const weapon = this.localPlayer.weaponInventory.getCurrentWeapon();
        if (weapon && weapon.config) {
            this.elements.weaponText.setText(weapon.config.name || '武器');
        }

        // Update weapon slot highlights
        const currentSlot = this.localPlayer.weaponInventory.currentSlot;
        if (this.elements.weaponSlots) {
            this.elements.weaponSlots.forEach((slotInfo, index) => {
                if (index === currentSlot) {
                    slotInfo.slot.setStrokeStyle(2, 0xFFD700, 1);
                    slotInfo.number.setColor('#FFD700');
                } else {
                    slotInfo.slot.setStrokeStyle(1, 0x888888, 0.5);
                    slotInfo.number.setColor('#AAAAAA');
                }
            });
        }
    }

    updateGrenade() {
        if (!this.localPlayer) return;

        // Highlight current grenade type
        const currentIndex = this.localPlayer.currentGrenadeIndex;

        if (this.elements.grenadeSlots) {
            this.elements.grenadeSlots.forEach((slotInfo, index) => {
                if (index === currentIndex) {
                    slotInfo.slot.setStrokeStyle(2, 0xFFD700, 1);
                } else {
                    slotInfo.slot.setStrokeStyle(2, 0x888888, 0.5);
                }
            });
        }
    }

    updateMedkit() {
        // Update medkit cooldown states if needed
        // This can be enhanced based on medkit system
    }

    updateMiniMap() {
        if (!this.elements.miniMap) return;

        const mapGraphics = this.elements.miniMap;
        mapGraphics.clear();

        const mapSize = 150;
        const scale = mapSize / this.scene.sys.game.config.width;

        // Draw player positions
        this.scene.players.forEach(player => {
            if (!player.isAlive) return;

            const mapX = (player.x - 50) * scale;
            const mapY = (player.y - 50) * scale;

            const color = player.team === 'red' ? this.colors.red : this.colors.blue;
            mapGraphics.fillStyle(color, 1);
            mapGraphics.fillCircle(mapX, mapY, player.isBot ? 3 : 5);
        });
    }

    updateScore(redScore, blueScore) {
        this.elements.redScoreText.setText(`红队 ${redScore}`);
        this.elements.blueScoreText.setText(`蓝队 ${blueScore}`);
    }

    updateRound(round) {
        this.elements.roundText.setText(`第${round}回合/${this.maxRounds || 7}`);
    }

    updateKillFeed(killFeed) {
        // Clear existing kill feed text
        this.elements.killFeedContainer.each(child => {
            if (child !== this.elements.killFeedContainer.list[0]) {
                child.destroy();
            }
        });

        // Add new kill feed entries
        killFeed.slice(0, 5).forEach((entry, index) => {
            const y = index * 25 + 10;
            const text = this.scene.add.text(0, y, `${entry.killer} 击杀了 ${entry.victim}`, {
                fontSize: '14px',
                fontFamily: 'Arial',
                color: '#FFFFFF'
            }).setOrigin(1, 0);

            this.elements.killFeedContainer.add(text);
        });
    }

    showSuddenDeath() {
        // Flash sudden death warning
        const centerX = this.screenWidth / 2;
        const centerY = this.screenHeight / 2;

        const warningBg = this.scene.add.rectangle(centerX, centerY, 400, 100, 0x000000, 0.8)
            .setOrigin(0.5)
            .setStrokeStyle(3, 0xFF0000, 1);

        const warningText = this.scene.add.text(centerX, centerY, '决胜局 - 生死局', {
            fontSize: '32px',
            fontFamily: 'Arial',
            color: '#FF0000',
            fontStyle: 'bold'
        }).setOrigin(0.5);

        // Animate and remove
        this.scene.tweens.add({
            targets: [warningBg, warningText],
            alpha: 0,
            duration: 2000,
            onComplete: () => {
                warningBg.destroy();
                warningText.destroy();
            }
        });
    }

    showRoundEnd(winner, scores) {
        const centerX = this.screenWidth / 2;
        const centerY = this.screenHeight / 2;

        const winnerName = winner ? (winner === 'red' ? '红队' : '蓝队') : '平局';

        const endBg = this.scene.add.rectangle(centerX, centerY, 350, 120, 0x000000, 0.85)
            .setOrigin(0.5)
            .setStrokeStyle(2, winner === 'red' ? this.colors.red : (winner === 'blue' ? this.colors.blue : 0xFFFFFF), 1);

        const endText = this.scene.add.text(centerX, centerY - 20, `${winnerName} 获胜!`, {
            fontSize: '28px',
            fontFamily: 'Arial',
            color: '#FFFFFF',
            fontStyle: 'bold'
        }).setOrigin(0.5);

        const scoreText = this.scene.add.text(centerX, centerY + 20, `比分: ${scores.red} - ${scores.blue}`, {
            fontSize: '20px',
            fontFamily: 'Arial',
            color: '#CCCCCC'
        }).setOrigin(0.5);

        // Animate and remove
        this.scene.tweens.add({
            targets: [endBg, endText, scoreText],
            alpha: 0,
            delay: 2000,
            duration: 1000,
            onComplete: () => {
                endBg.destroy();
                endText.destroy();
                scoreText.destroy();
            }
        });
    }

    showGameOver(winner, scores) {
        const centerX = this.screenWidth / 2;
        const centerY = this.screenHeight / 2;

        const winnerName = winner === 'red' ? '红队' : '蓝队';
        const winnerColor = winner === 'red' ? this.colors.red : this.colors.blue;

        // Game over overlay
        const overlay = this.scene.add.rectangle(centerX, centerY, this.screenWidth, this.screenHeight, 0x000000, 0.8)
            .setOrigin(0.5);

        // Winner announcement
        const winnerBg = this.scene.add.rectangle(centerX, centerY - 60, 400, 80, winnerColor, 0.9)
            .setOrigin(0.5)
            .setStrokeStyle(3, 0xFFFFFF, 1);

        const winnerText = this.scene.add.text(centerX, centerY - 60, `${winnerName} 胜利!`, {
            fontSize: '36px',
            fontFamily: 'Arial',
            color: '#FFFFFF',
            fontStyle: 'bold'
        }).setOrigin(0.5);

        // Final score
        const finalScore = this.scene.add.text(centerX, centerY + 20, `最终比分: ${scores.red} - ${scores.blue}`, {
            fontSize: '24px',
            fontFamily: 'Arial',
            color: '#FFFFFF'
        }).setOrigin(0.5);

        // Restart hint
        const restartHint = this.scene.add.text(centerX, centerY + 80, '按 R 键重新开始', {
            fontSize: '18px',
            fontFamily: 'Arial',
            color: '#AAAAAA'
        }).setOrigin(0.5);

        this.elements.gameOverElements = [overlay, winnerBg, winnerText, finalScore, restartHint];
    }

    hideGameOver() {
        if (this.elements.gameOverElements) {
            this.elements.gameOverElements.forEach(el => el.destroy());
            this.elements.gameOverElements = [];
        }
    }

    destroy() {
        // Clean up all HUD elements
        // Most elements will be destroyed with scene, but custom containers need cleanup
    }
}

export default HUDManager;