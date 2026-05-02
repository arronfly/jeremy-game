const Phaser = window.Phaser;
export default class EndScene extends Phaser.Scene {
    constructor() {
        super({ key: 'EndScene' });
    }

    create() {
        const { width, height } = this.sys.game.config;

        this.add.rectangle(width/2, height/2, width, height, 0x1a1a2e);

        this.add.text(width/2, height/2 - 100, '比赛结束', {
            fontSize: '64px',
            color: '#fff'
        }).setOrigin(0.5);

        const restartBtn = this.add.rectangle(width/2, height/2 + 50, 200, 60, 0x27AE60)
            .setInteractive({ useHandCursor: true });

        this.add.text(width/2, height/2 + 50, '再来一局', {
            fontSize: '28px',
            color: '#fff'
        }).setOrigin(0.5);

        restartBtn.on('pointerdown', () => {
            this.scene.start('MenuScene');
        });
    }
}