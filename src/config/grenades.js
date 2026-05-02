// Grenade configurations
export const GRENADES = {
    smoke: {
        id: 'smoke',
        name: '烟雾弹',
        cooldown: 3000,
        duration: 5000, // 烟雾区域持续5秒
        radius: 150,
        type: 'smoke'
    },
    flash: {
        id: 'flash',
        name: '闪光弹',
        cooldown: 3000,
        blindDuration: 2000, // 致盲2秒
        radius: 100,
        type: 'flash'
    },
    frag: {
        id: 'frag',
        name: '手雷',
        cooldown: 3000,
        damage: 50,
        radius: 120,
        type: 'frag'
    }
};

export default GRENADES;