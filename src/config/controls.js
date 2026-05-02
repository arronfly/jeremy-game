// Control mapping for the game
export const CONTROLS = {
    // Movement
    MOVE_UP: 'W',
    MOVE_DOWN: 'S',
    MOVE_LEFT: 'A',
    MOVE_RIGHT: 'D',

    // Combat
    SHOOT: 'leftbutton',
    RELOAD: 'R',

    // Weapon selection
    WEAPON_1: 'ONE',
    WEAPON_2: 'TWO',
    WEAPON_3: 'THREE',
    WEAPON_4: 'FOUR',

    // Grenades
    GRENADE: 'G',
    GRENADE_THROW: 'rightbutton',

    // Medical items
    USE_MEDKIT: 'SEVEN',
    USE_BANDAGE: 'EIGHT',
    USE_ENERGY_DRINK: 'NINE'
};

export const PLAYER_CONFIG = {
    speed: 150,
    health: 100,
    radius: 20
};