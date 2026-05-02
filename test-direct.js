const Phaser = window.Phaser;

const config = {
    type: Phaser.AUTO,
    width: 800,
    height: 600,
    parent: 'game-container',
    backgroundColor: '#2d2d44',
    scene: [Phaser.Scene]
};

class TestScene extends Phaser.Scene {
    constructor() {
        super({ key: 'Test' });
    }
    create() {
        this.add.text(400, 300, 'Direct Phaser Works!', {
            fontSize: '32px',
            color: '#00ff00'
        }).setOrigin(0.5);
    }
}

config.scene = [TestScene];
const game = new Phaser.Game(config);
window.game = game;
console.log('Direct Phaser created successfully');