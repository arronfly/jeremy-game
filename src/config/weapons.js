export const WEAPONS = {
    assaultRifle: {
        id: 'assaultRifle',
        name: '突击步枪',
        damage: 25,
        fireRate: 400,
        magSize: 30,
        maxReserve: 120,
        bulletSpeed: 800,
        spread: 0.02,
        pellets: 1
    },
    sniper: {
        id: 'sniper',
        name: '狙击枪',
        damage: 80,
        fireRate: 1500,
        magSize: 5,
        maxReserve: 20,
        bulletSpeed: 1200,
        spread: 0,
        pellets: 1
    },
    smg: {
        id: 'smg',
        name: '冲锋枪',
        damage: 18,
        fireRate: 200,
        magSize: 35,
        maxReserve: 140,
        bulletSpeed: 700,
        spread: 0.06,
        pellets: 1
    },
    shotgun: {
        id: 'shotgun',
        name: '散弹枪',
        damage: 15,
        fireRate: 800,
        magSize: 8,
        maxReserve: 32,
        bulletSpeed: 600,
        spread: 0.15,
        pellets: 8
    }
};

export default WEAPONS;