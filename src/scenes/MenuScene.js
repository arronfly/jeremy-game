export default class MenuScene extends Phaser.Scene {
    constructor() {
        super({ key: 'MenuScene' });
    }

    create() {
        this.createUI();
    }

    createUI() {
        const { width, height } = this.sys.game.config;

        // Background
        this.add.rectangle(width/2, height/2, width, height, 0x1a1a2e);

        // Title
        this.add.text(width/2, 200, '歼灭团竞2', {
            fontSize: '72px',
            color: '#fff',
            fontFamily: 'Arial'
        }).setOrigin(0.5);

        // Subtitle
        this.add.text(width/2, 280, '写实风格团队竞技', {
            fontSize: '28px',
            color: '#aaa'
        }).setOrigin(0.5);

        // Start button
        const startBtn = this.add.rectangle(width/2, 450, 300, 80, 0x27AE60)
            .setInteractive({ useHandCursor: true });

        const startText = this.add.text(width/2, 450, '开始游戏', {
            fontSize: '36px',
            color: '#fff'
        }).setOrigin(0.5);

        startBtn.on('pointerover', () => startBtn.setFillStyle(0x2ECC71));
        startBtn.on('pointerout', () => startBtn.setFillStyle(0x27AE60));
        startBtn.on('pointerdown', () => {
            this.scene.start('LoadScene');
        });

        // Controls info
        const controls = [
            'WASD - 移动',
            '鼠标 - 瞄准',
            '左键 - 射击',
            'R - 换弹',
            '1-4 - 切换武器',
            'G - 投掷物',
            '7/8/9 - 药品'
        ];

        this.add.text(width/2, 600, controls.join('\n'), {
            fontSize: '24px',
            color: '#888',
            lineSpacing: 10
        }).setOrigin(0.5);

        // Version
        this.add.text(width/2, height - 50, 'v1.0.0', {
            fontSize: '18px',
            color: '#555'
        }).setOrigin(0.5);
    }
}