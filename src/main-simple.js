const Phaser = window.Phaser;

// Direct game creation bypassing modules
const config = {
    type: Phaser.AUTO,
    width: 1600,
    height: 1200,
    backgroundColor: '#2d2d44',
    parent: 'game-container'
};

// Create a minimal scene that immediately starts the game
class StartScene extends Phaser.Scene {
    constructor() {
        super({ key: 'Start' });
    }
    create() {
        this.add.text(800, 400, '游戏加载中...', {
            fontSize: '48px',
            color: '#ffffff'
        }).setOrigin(0.5);

        // Immediately start MenuScene
        this.time.delayedCall(500, () => {
            this.scene.start('MenuScene');
        });
    }
}

class MenuScene extends Phaser.Scene {
    constructor() {
        super({ key: 'MenuScene' });
    }
    create() {
        const { width, height } = this.sys.game.config;

        this.add.rectangle(width/2, height/2, width, height, 0x1a1a2e);
        this.add.text(width/2, 200, '歼灭团竞2', {
            fontSize: '72px',
            color: '#fff'
        }).setOrigin(0.5);

        const startBtn = this.add.rectangle(width/2, 450, 300, 80, 0x27AE60).setInteractive();
        this.add.text(width/2, 450, '开始游戏', {
            fontSize: '36px',
            color: '#fff'
        }).setOrigin(0.5);

        startBtn.on('pointerdown', () => {
            this.scene.start('LoadScene');
        });

        this.add.text(width/2, 600, 'WASD移动 | 鼠标瞄准 | 左键射击 | R换弹', {
            fontSize: '24px',
            color: '#888'
        }).setOrigin(0.5);
    }
}

class LoadScene extends Phaser.Scene {
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

        progress.fillStyle(0x27AE60, 1);
        progress.fillRect(barX, barY, barWidth, barHeight);
        progress.lineStyle(2, 0xffffff);
        progress.strokeRect(barX, barY, barWidth, barHeight);

        this.add.text(width/2, height/2 + 60, '加载完成！', {
            fontSize: '24px',
            color: '#27AE60'
        }).setOrigin(0.5);

        this.time.delayedCall(2000, () => {
            this.scene.start('GameScene');
        });
    }
}

class GameScene extends Phaser.Scene {
    constructor() {
        super({ key: 'GameScene' });
    }
    create() {
        const { width, height } = this.sys.game.config;

        this.add.rectangle(width/2, height/2, width, height, 0x2d2d44);
        this.add.text(width/2, height/2, '游戏场景 - 按 WASD 移动', {
            fontSize: '32px',
            color: '#ffffff'
        }).setOrigin(0.5);

        // Show instructions
        this.add.text(width/2, 100, '歼灭团竞2 - 游戏进行中', {
            fontSize: '28px',
            color: '#ffff00'
        }).setOrigin(0.5);
    }
}

config.scene = [StartScene, MenuScene, LoadScene, GameScene];

const game = new Phaser.Game(config);
window.game = game;