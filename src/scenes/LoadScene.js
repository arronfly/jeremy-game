export default class LoadScene extends Phaser.Scene {
    constructor() {
        super({ key: 'LoadScene' });
    }

    create() {
        const { width, height } = this.sys.game.config;

        this.add.text(width/2, height/2 - 50, '加载资源中...', {
            fontSize: '36px',
            color: '#fff'
        }).setOrigin(0.5);

        const progress = this.add.graphics();
        const barWidth = 400;
        const barHeight = 30;
        const barX = width/2 - barWidth/2;
        const barY = height/2 + 20;

        this.load.on('progress', (value) => {
            progress.clear();
            progress.fillStyle(0x27AE60, 1);
            progress.fillRect(barX, barY, barWidth * value, barHeight);
            progress.lineStyle(2, 0xffffff);
            progress.strokeRect(barX, barY, barWidth, barHeight);
        });

        this.load.on('complete', () => {
            // Show "loading complete" briefly before starting
            this.add.text(width/2, height/2 + 60, '加载完成！', {
                fontSize: '24px',
                color: '#27AE60'
            }).setOrigin(0.5);

            // Delay 1.5 seconds so user can see the loading screen
            this.time.delayedCall(1500, () => {
                this.scene.start('GameScene');
            });
        });

        // Start loading
        this.generateTextures();
        this.load.start();

        // Even though nothing is in the queue, fire progress to show loading bar
        this.time.delayedCall(100, () => {
            progress.clear();
            progress.fillStyle(0x27AE60, 1);
            progress.fillRect(barX, barY, barWidth, barHeight);
            progress.lineStyle(2, 0xffffff);
            progress.strokeRect(barX, barY, barWidth, barHeight);
        });
    }

    generateTextures() {
        // Generate player textures for both teams
        this.generatePlayerTexture('player_red', 0xE74C3C);
        this.generatePlayerTexture('player_blue', 0x3498DB);
        this.generateBulletTexture();
        this.generateGrenadeTextures();
        this.generateMedkitTextures();
    }

    generatePlayerTexture(key, color) {
        const graphics = this.make.graphics({ x: 0, y: 0 });

        // Shadow
        graphics.fillStyle(0x000000, 0.3);
        graphics.fillEllipse(24, 44, 32, 12);

        // Body (torso)
        graphics.fillStyle(color);
        graphics.fillEllipse(24, 28, 16, 20);

        // Head
        graphics.fillStyle(0xFFDBB4); // Skin color
        graphics.fillCircle(24, 12, 10);
        graphics.fillStyle(color, 0.8);
        graphics.fillEllipse(24, 8, 12, 6); // Helmet top

        // Arms
        graphics.lineStyle(4, color);
        graphics.lineBetween(12, 22, 4, 32);
        graphics.lineBetween(36, 22, 44, 32);

        // Legs
        graphics.lineBetween(18, 38, 14, 48);
        graphics.lineBetween(30, 38, 34, 48);

        // Weapon
        graphics.lineStyle(3, 0x333333);
        graphics.lineBetween(24, 28, 48, 28);

        graphics.generateTexture(key, 48, 48);
        graphics.destroy();
    }

    generateBulletTexture() {
        const graphics = this.make.graphics({ x: 0, y: 0 });
        graphics.fillStyle(0xFFFF00);
        graphics.fillCircle(4, 4, 4);
        graphics.generateTexture('bullet', 8, 8);
        graphics.destroy();
    }

    generateGrenadeTextures() {
        // Smoke grenade - gray
        let graphics = this.make.graphics({ x: 0, y: 0 });
        graphics.fillStyle(0x888888);
        graphics.fillCircle(12, 12, 10);
        graphics.lineStyle(2, 0x555555);
        graphics.lineBetween(12, 2, 12, 6);
        graphics.generateTexture('smoke_grenade', 24, 24);
        graphics.destroy();

        // Flash grenade - white/yellow
        graphics = this.make.graphics({ x: 0, y: 0 });
        graphics.fillStyle(0xFFFF00);
        graphics.fillCircle(12, 12, 10);
        graphics.lineStyle(2, 0xCCCC00);
        graphics.lineBetween(12, 2, 12, 6);
        graphics.generateTexture('flash_grenade', 24, 24);
        graphics.destroy();

        // Frag grenade - dark green
        graphics = this.make.graphics({ x: 0, y: 0 });
        graphics.fillStyle(0x2E8B57);
        graphics.fillCircle(12, 12, 10);
        graphics.lineStyle(2, 0x1a5234);
        graphics.lineBetween(12, 2, 12, 6);
        graphics.generateTexture('frag_grenade', 24, 24);
        graphics.destroy();
    }

    generateMedkitTextures() {
        // First aid kit - red/white
        let graphics = this.make.graphics({ x: 0, y: 0 });
        graphics.fillStyle(0xFFFFFF);
        graphics.fillRect(4, 8, 16, 16);
        graphics.fillStyle(0xE74C3C);
        graphics.fillRect(8, 12, 8, 8);
        graphics.generateTexture('first_aid_kit', 24, 32);
        graphics.destroy();

        // Bandage - white roll
        graphics = this.make.graphics({ x: 0, y: 0 });
        graphics.fillStyle(0xFFFFFF);
        graphics.fillEllipse(12, 12, 14, 10);
        graphics.lineStyle(2, 0xDDDDDD);
        graphics.strokeEllipse(12, 12, 14, 10);
        graphics.generateTexture('bandage', 24, 24);
        graphics.destroy();

        // Energy drink - blue can
        graphics = this.make.graphics({ x: 0, y: 0 });
        graphics.fillStyle(0x3498DB);
        graphics.fillRect(6, 4, 12, 20);
        graphics.fillStyle(0x2980B9);
        graphics.fillRect(8, 6, 8, 16);
        graphics.generateTexture('energy_drink', 24, 28);
        graphics.destroy();
    }
}