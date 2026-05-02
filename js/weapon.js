// Weapon definitions
const WEAPONS = {
    assault_rifle: {
        name: '突击步枪',
        damage: 15,
        fireRate: 100, // ms between shots
        magazineSize: 30,
        reserveAmmo: 90,
        bulletSpeed: 15,
        reloadTime: 1500,
        spread: 0.05
    },
    sniper: {
        name: '狙击枪',
        damage: 50,
        fireRate: 800,
        magazineSize: 5,
        reserveAmmo: 15,
        bulletSpeed: 25,
        reloadTime: 2500,
        spread: 0.01
    }
};

class Weapon {
    constructor(type) {
        const def = WEAPONS[type];
        this.type = type;
        this.name = def.name;
        this.damage = def.damage;
        this.fireRate = def.fireRate;
        this.magazineSize = def.magazineSize;
        this.reserveAmmo = def.reserveAmmo;
        this.bulletSpeed = def.bulletSpeed;
        this.reloadTime = def.reloadTime;
        this.spread = def.spread;

        this.currentAmmo = this.magazineSize;
        this.lastFireTime = 0;
        this.isReloading = false;
        this.reloadStartTime = 0;
    }

    canFire(time) {
        if (this.isReloading) return false;
        if (this.currentAmmo <= 0) return false;
        return time - this.lastFireTime >= this.fireRate;
    }

    fire(time, x, y, angle, team) {
        if (!this.canFire(time)) return null;

        this.currentAmmo--;
        this.lastFireTime = time;

        const spread = (Math.random() - 0.5) * this.spread;
        const finalAngle = angle + spread;

        return {
            x,
            y,
            vx: Math.cos(finalAngle) * this.bulletSpeed,
            vy: Math.sin(finalAngle) * this.bulletSpeed,
            damage: this.damage,
            team,
            trail: []
        };
    }

    startReload(time) {
        if (this.isReloading || this.currentAmmo === this.magazineSize || this.reserveAmmo <= 0) return;
        this.isReloading = true;
        this.reloadStartTime = time;
    }

    update(time) {
        if (this.isReloading && time - this.reloadStartTime >= this.reloadTime) {
            const needed = this.magazineSize - this.currentAmmo;
            const toReload = Math.min(needed, this.reserveAmmo);
            this.currentAmmo += toReload;
            this.reserveAmmo -= toReload;
            this.isReloading = false;
        }
    }

    draw(ctx, x, y, angle) {
        ctx.save();
        ctx.translate(x, y);
        ctx.rotate(angle);

        // Weapon body
        ctx.fillStyle = '#666';
        ctx.fillRect(-15, -4, 30, 8);

        // Muzzle
        ctx.fillStyle = '#444';
        ctx.fillRect(15, -3, 8, 6);

        ctx.restore();

        // Reload indicator
        if (this.isReloading) {
            const progress = (time => time - this.reloadStartTime) / this.reloadTime;
            ctx.fillStyle = 'rgba(255, 200, 0, 0.8)';
            ctx.font = '14px sans-serif';
            ctx.fillText('换弹中...', x - 30, y - 30);
        }
    }
}
