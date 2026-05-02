export const GAME_CONFIG = {
    width: 1600,
    height: 1200,
    pixelArt: false,
    backgroundColor: '#2d2d44',
    physics: {
        default: 'arcade',
        arcade: { debug: false }
    }
};

export const TEAM_CONFIG = {
    red: { color: 0xE74C3C, name: '红队', spawnX: 150 },
    blue: { color: 0x3498DB, name: '蓝队', spawnX: 1450 }
};

export const RULES = {
    winScore: 4,
    maxRounds: 7,
    respawnTime: 3000,
    playerSpeed: 150,
    playerHealth: 100
};