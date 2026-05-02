export default class GameScene extends Phaser.Scene {
    constructor() {
        super({ key: 'GameScene' });
    }

    create() {
        const { width, height } = this.sys.game.config;

        // Background
        this.add.rectangle(width/2, height/2, width, height, 0x2d2d44);

        // TODO: Load map and game logic
        this.add.text(width/2, height/2, '游戏场景加载中...', {
            fontSize: '36px',
            color: '#fff'
        }).setOrigin(0.5);
    }
}