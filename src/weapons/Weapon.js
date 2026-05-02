import { WEAPONS } from '../config/weapons.js';

export default class Weapon {
    constructor(scene, weaponId) {
        this.scene = scene;
        this.config = WEAPONS[weaponId];
        this.id = weaponId;

        this.magAmmo = this.config.magSize;
        this.reserveAmmo = this.config.maxReserve;

        this.lastFiredTime = 0;
        this.isReloading = false;
        this.reloadTimer = null;
    }

    canFire() {
        if (this.isReloading) return false;
        if (this.magAmmo <= 0) return false;

        const currentTime = this.scene.time.now;
        if (currentTime - this.lastFiredTime < this.config.fireRate) {
            return false;
        }

        return true;
    }

    fire() {
        if (!this.canFire()) return false;

        this.magAmmo--;
        this.lastFiredTime = this.scene.time.now;

        return true;
    }

    reload() {
        if (this.isReloading) return false;
        if (this.magAmmo >= this.config.magSize) return false;
        if (this.reserveAmmo <= 0) return false;

        this.isReloading = true;

        const reloadTime = this.id === 'sniper' ? 2500 : 1500;

        this.reloadTimer = this.scene.time.addEvent({
            delay: reloadTime,
            callback: this.completeReload,
            callbackScope: this
        });

        return true;
    }

    completeReload() {
        const needed = this.config.magSize - this.magAmmo;
        const available = Math.min(needed, this.reserveAmmo);

        this.magAmmo += available;
        this.reserveAmmo -= available;

        this.isReloading = false;
        this.reloadTimer = null;
    }

    cancelReload() {
        if (this.reloadTimer) {
            this.reloadTimer.remove(false);
            this.reloadTimer = null;
        }
        this.isReloading = false;
    }

    switchTo(newWeaponId) {
        this.cancelReload();
        this.isReloading = false;
        this.config = WEAPONS[newWeaponId];
        this.id = newWeaponId;
        this.magAmmo = this.config.magSize;
        this.reserveAmmo = this.config.maxReserve;
    }

    getMagAmmo() {
        return this.magAmmo;
    }

    getReserveAmmo() {
        return this.reserveAmmo;
    }

    getWeaponInfo() {
        return {
            name: this.config.name,
            magAmmo: this.magAmmo,
            magSize: this.config.magSize,
            reserveAmmo: this.reserveAmmo,
            isReloading: this.isReloading
        };
    }
}

export class WeaponInventory {
    constructor(scene) {
        this.scene = scene;
        this.weapons = {
            1: new Weapon(scene, 'assaultRifle'),
            2: new Weapon(scene, 'sniper'),
            3: new Weapon(scene, 'smg'),
            4: new Weapon(scene, 'shotgun')
        };
        this.currentSlot = 1;
    }

    getCurrentWeapon() {
        return this.weapons[this.currentSlot];
    }

    switchToSlot(slot) {
        if (slot >= 1 && slot <= 4 && this.weapons[slot]) {
            this.currentSlot = slot;
            return this.weapons[slot];
        }
        return null;
    }

    reload() {
        const weapon = this.getCurrentWeapon();
        if (weapon) {
            return weapon.reload();
        }
        return false;
    }

    update(time) {
        // Called every frame to update weapon state
    }
}