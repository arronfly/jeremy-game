// Medical item configurations
export const MEDICALS = {
    firstAidKit: {
        id: 'firstAidKit',
        name: '急救包',
        cooldown: 10000,
        healAmount: 50,
        useTime: 3000, // 3秒使用时间
        type: 'heal'
    },
    bandage: {
        id: 'bandage',
        name: '绷带',
        cooldown: 5000,
        healAmount: 10,
        useTime: 1000, // 1秒使用时间
        type: 'heal'
    },
    energyDrink: {
        id: 'energyDrink',
        name: '能量饮料',
        cooldown: 8000,
        healAmount: 25,
        speedBoost: 0.2, // 移动速度+20%
        duration: 10000, // 持续10秒
        type: 'buff'
    }
};

export default MEDICALS;