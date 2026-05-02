import { GAME_CONFIG } from './config/game.js';
import BootScene from './scenes/BootScene.js';
import MenuScene from './scenes/MenuScene.js';
import LoadScene from './scenes/LoadScene.js';
import GameScene from './scenes/GameScene.js';
import EndScene from './scenes/EndScene.js';

const config = {
    ...GAME_CONFIG,
    scene: [BootScene, MenuScene, LoadScene, GameScene, EndScene]
};

const game = new Phaser.Game(config);
window.game = game;