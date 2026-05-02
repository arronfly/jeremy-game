import { WeaponInventory } from '../weapons/Weapon.js';
import { BulletManager } from '../weapons/Bullet.js';
import { WEAPONS } from '../config/weapons.js';
import { GRENADES } from '../config/grenades.js';
import { MEDICALS } from '../config/medicals.js';
import { Grenade } from '../items/Grenade.js';
import Medkit, { EnergyDrinkEffect } from '../items/Medkit.js';
import { PLAYER_CONFIG } from '../config/controls.js';
import { HitEffects } from '../effects/HitEffects.js';

const Phaser = window.Phaser;

export default class Player extends Phaser.GameObjects.Container {
    constructor(scene, x, y, team, hitEffects = null) {
        super(scene, x, y);

        this.scene = scene;
        this.team = team;
        this.health = PLAYER_CONFIG.health;
        this.maxHealth = PLAYER_CONFIG.health;
        this.speed = PLAYER_CONFIG.speed;
        this.radius = PLAYER_CONFIG.radius;
        this.isAlive = true;
        this.isBot = false;
        this.currentAnimation = 'idle';
        this.isShooting = false;
        this.isReloading = false;
        this.hitEffects = hitEffects;

        // Weapon system
        this.weaponInventory = new WeaponInventory(scene);
        this.bulletManager = new BulletManager(scene);

        // Grenade system
        this.grenadeTypes = ['smoke', 'flash', 'frag'];
        this.currentGrenadeIndex = 0;
        this.grenades = [];
        this.grenadeCooldownEnd = 0;
        this.isThrowing = false;
        this.throwHoldStart = 0;

        // Medkit system
        this.medicTypes = ['firstAidKit', 'bandage', 'energyDrink'];
        this.medkits = {
            firstAidKit: new Medkit(scene, 'firstAidKit', this),
            bandage: new Medkit(scene, 'bandage', this),
            energyDrink: new Medkit(scene, 'energyDrink', this)
        };
        this.energyDrinkEffect = null;

        // Status effects
        this.isBlinded = false;
        this.blindEndTime = 0;

        // Team color
        this.teamColor = team === 'red' ? 0xE74C3C : 0x3498DB;
        this.teamName = team === 'red' ? '红队' : '蓝队';

        // Body container for rotation (so shadow stays fixed)
        this.bodyContainer = this.scene.add.container(0, 0);

        // Create shadow
        this.createShadow();

        // Create body parts
        this.createBody();

        // Add to scene
        scene.add.existing(this);

        // Create animations
        this.createAnimations();
    }

    createShadow() {
        // Shadow under player (ellipse)
        this.shadow = this.scene.add.ellipse(0, 16, 32, 12, 0x000000, 0.3);
        this.add(this.shadow);
    }

    createBody() {
        const color = this.teamColor;
        const skinColor = 0xFFDBB4;

        // Torso (ellipse) - positioned at center
        this.torso = this.scene.add.ellipse(0, 4, 16, 20, color);
        this.bodyContainer.add(this.torso);

        // Head (circle with highlight)
        this.head = this.scene.add.circle(0, -12, 10, skinColor);
        this.bodyContainer.add(this.head);

        // Helmet top (ellipse with team color)
        this.helmet = this.scene.add.ellipse(0, -16, 12, 6, color, 0.8);
        this.bodyContainer.add(this.helmet);

        // Highlight on head (small circle for 3D effect)
        this.headHighlight = this.scene.add.circle(-3, -15, 3, 0xFFFFFF, 0.3);
        this.bodyContainer.add(this.headHighlight);

        // Arms (lines)
        this.leftArm = this.scene.add.line(0, 0, -12, -2, -20, 8, color, 4);
        this.rightArm = this.scene.add.line(0, 0, 12, -2, 20, 8, color, 4);
        this.bodyContainer.add(this.leftArm);
        this.bodyContainer.add(this.rightArm);

        // Legs (lines)
        this.leftLeg = this.scene.add.line(0, 0, -6, 14, -10, 24, color, 4);
        this.rightLeg = this.scene.add.line(0, 0, 6, 14, 10, 24, color, 4);
        this.bodyContainer.add(this.leftLeg);
        this.bodyContainer.add(this.rightLeg);

        // Weapon (line)
        this.weapon = this.scene.add.line(0, 0, 0, 4, 24, 4, 0x333333, 3);
        this.bodyContainer.add(this.weapon);

        this.add(this.bodyContainer);
    }

    createAnimations() {
        // Animation frames are drawn programmatically
        // This is a simplified version - real sprites would have multiple frames
        this.animations = {
            idle: { frame: 0, duration: 100 },
            walk: { frames: [0, 1, 2], duration: 150 },
            run: { frames: [0, 1, 2, 3, 4, 5], duration: 100 },
            shoot: { frames: [0, 1, 2, 3], duration: 100 },
            reload: { frames: [0, 1, 2, 3, 4, 5, 6, 7], duration: 150 },
            hurt: { frames: [0, 1, 2], duration: 100 },
            death: { frames: [0, 1, 2, 3, 4], duration: 200 }
        };
    }

    playAnimation(name) {
        if (this.currentAnimation === name) return;
        this.currentAnimation = name;
        // Animation playback logic would go here
    }

    moveUp() {
        if (!this.isAlive) return;
        this.y -= this.speed * 0.016; // Assuming 60fps
        this.playAnimation('walk');
    }

    moveDown() {
        if (!this.isAlive) return;
        this.y += this.speed * 0.016;
        this.playAnimation('walk');
    }

    moveLeft() {
        if (!this.isAlive) return;
        this.x -= this.speed * 0.016;
        this.playAnimation('walk');
    }

    moveRight() {
        if (!this.isAlive) return;
        this.x += this.speed * 0.016;
        this.playAnimation('walk');
    }

    aimAt(pointer) {
        // Rotate body container to face the pointer (not the shadow)
        const angle = Phaser.Math.Angle.Between(this.x, this.y, pointer.x, pointer.y);
        this.bodyContainer.setRotation(angle);
    }

    shoot(targetX, targetY) {
        if (!this.isAlive || this.isReloading) return;

        const weapon = this.weaponInventory.getCurrentWeapon();
        if (!weapon) return;

        if (!weapon.canFire()) {
            // Auto reload if magazine empty
            if (weapon.getMagAmmo() <= 0 && weapon.getReserveAmmo() > 0) {
                this.reload();
            }
            return;
        }

        const fired = weapon.fire();
        if (!fired) return;

        this.isShooting = true;
        this.playAnimation('shoot');

        // Create bullet(s) with proper weapon config
        const angle = Phaser.Math.Angle.Between(this.x, this.y, targetX, targetY);
        const weaponConfig = weapon.config;

        // Muzzle flash effect
        const muzzleX = this.x + Math.cos(angle) * 24;
        const muzzleY = this.y + Math.sin(angle) * 24;
        if (this.hitEffects) {
            this.hitEffects.createMuzzleFlashEffect(muzzleX, muzzleY, Phaser.Math.RadToDeg(angle));
            this.hitEffects.createShellCasingEffect(this.x, this.y, Phaser.Math.RadToDeg(angle));
        }

        this.bulletManager.fireBullet(
            muzzleX,
            muzzleY,
            angle,
            weaponConfig,
            false
        );

        // Reset shooting state based on fire rate
        this.scene.time.delayedCall(Math.min(weaponConfig.fireRate, 200), () => {
            this.isShooting = false;
        });
    }

    reload() {
        if (!this.isAlive || this.isReloading) return;

        const weapon = this.weaponInventory.getCurrentWeapon();
        if (!weapon) return false;

        if (weapon.getMagAmmo() >= weapon.config.magSize) return false;
        if (weapon.getReserveAmmo() <= 0) return false;

        this.isReloading = true;
        this.playAnimation('reload');

        return weapon.reload();
    }

    switchWeapon(slot) {
        if (slot >= 1 && slot <= 4) {
            const weapon = this.weaponInventory.switchToSlot(slot);
            if (weapon) {
                this.scene.events.emit('weaponSwitched', weapon.getWeaponInfo());
                return true;
            }
        }
        return false;
    }

    getWeaponInfo() {
        const weapon = this.weaponInventory.getCurrentWeapon();
        return weapon ? weapon.getWeaponInfo() : null;
    }

    getBullets() {
        return this.bulletManager.getBullets();
    }

    updateBulletCollisions(enemies) {
        const bullets = this.bulletManager.getBullets();
        if (!bullets) return;

        bullets.getChildren().forEach(bullet => {
            if (!bullet.active) return;

            enemies.forEach(enemy => {
                if (!enemy.isAlive || !enemy.sprite) return;

                const distance = Phaser.Math.Distance.Between(
                    bullet.x, bullet.y,
                    enemy.x, enemy.y
                );

                if (distance < enemy.radius + 4) {
                    enemy.takeDamage(bullet.damage);
                    bullet.destroy();
                }
            });
        });
    }

    takeDamage(amount) {
        if (!this.isAlive) return;

        this.health -= amount;
        this.playAnimation('hurt');

        // Show damage number
        this.showDamageNumber(amount);

        // Trigger blood effect if hit effects is available
        if (this.hitEffects) {
            this.hitEffects.createBloodEffect(this.x, this.y, Math.random() * 360);
        }

        if (this.health <= 0) {
            this.die();
        }
    }

    showDamageNumber(amount) {
        const text = this.scene.add.text(this.x, this.y - 30, `-${amount}`, {
            fontSize: '16px',
            color: '#FF6B6B',
            fontFamily: 'Arial'
        }).setOrigin(0.5);

        // Animate the text
        this.scene.tweens.add({
            targets: text,
            y: text.y - 40,
            alpha: 0,
            duration: 800,
            onComplete: () => text.destroy()
        });
    }

    die() {
        this.isAlive = false;
        this.playAnimation('death');

        // Fade out the player
        this.scene.tweens.add({
            targets: this,
            alpha: 0.5,
            duration: 500
        });
    }

    respawn(x, y) {
        this.x = x;
        this.y = y;
        this.health = this.maxHealth;
        this.isAlive = true;
        this.alpha = 1;
        this.playAnimation('idle');
    }

    update(pointer) {
        if (!this.isAlive) return;

        // Aim at mouse pointer (only for human players, bots use their own AI)
        if (pointer && !this.isBot) {
            this.aimAt(pointer);
        }

        // Update bullet manager trails
        this.bulletManager.update();

        // Update medkits
        for (const medkit of Object.values(this.medkits)) {
            medkit.update();
        }

        // Update energy drink effect
        if (this.energyDrinkEffect) {
            this.energyDrinkEffect.update();
            if (!this.energyDrinkEffect.isActive) {
                this.energyDrinkEffect = null;
            }
        }

        // Check blind effect end
        if (this.isBlinded && this.scene.time.now >= this.blindEndTime) {
            this.isBlinded = false;
        }
    }

    // ============= GRENADE SYSTEM =============
    getCurrentGrenadeType() {
        return this.grenadeTypes[this.currentGrenadeIndex];
    }

    getCurrentGrenade() {
        return GRENADES[this.getCurrentGrenadeType()];
    }

    switchGrenade() {
        this.currentGrenadeIndex = (this.currentGrenadeIndex + 1) % this.grenadeTypes.length;
        const grenadeType = this.getCurrentGrenadeType();
        this.scene.events.emit('grenadeSwitched', grenadeType, GRENADES[grenadeType]);
        return grenadeType;
    }

    canThrowGrenade() {
        return this.scene.time.now >= this.grenadeCooldownEnd && !this.isThrowing;
    }

    startGrenadeThrow() {
        if (!this.canThrowGrenade()) return false;
        this.isThrowing = true;
        this.throwHoldStart = this.scene.time.now;
        return true;
    }

    releaseGrenade(targetX, targetY) {
        if (!this.isThrowing) return null;

        const holdTime = this.scene.time.now - this.throwHoldStart;
        const grenadeType = this.getCurrentGrenadeType();
        const angle = Phaser.Math.Angle.Between(this.x, this.y, targetX, targetY);

        // Create and throw grenade
        const grenade = new Grenade(this.scene, grenadeType, this);
        grenade.startThrow(angle, holdTime);
        this.grenades.push(grenade);

        // Set cooldown
        this.grenadeCooldownEnd = this.scene.time.now + GRENADES[grenadeType].cooldown;

        this.isThrowing = false;
        this.scene.events.emit('grenadeThrown', grenadeType);

        return grenade;
    }

    cancelGrenadeThrow() {
        this.isThrowing = false;
    }

    updateGrenades(delta) {
        for (let i = this.grenades.length - 1; i >= 0; i--) {
            const grenade = this.grenades[i];
            grenade.update(delta);

            // Remove inactive grenades
            if (!grenade.isActive) {
                this.grenades.splice(i, 1);
            }
        }
    }

    // ============= MEDKIT SYSTEM =============
    useMedkit(type) {
        if (!this.medkits[type]) return false;
        return this.medkits[type].startUse();
    }

    cancelMedkitUse(type) {
        if (this.medkits[type]) {
            this.medkits[type].cancelUse();
        }
    }

    playUseAnimation(type) {
        this.playAnimation('idle');
    }

    updateUseProgress(type, progress) {
        // Progress bar implementation
    }

    clearUseProgress(type) {
        // Clear progress bar
    }

    applyBlind(duration) {
        this.isBlinded = true;
        this.blindEndTime = this.scene.time.now + duration;
    }

    // ============= GETTERS FOR UI =============
    getGrenadeInfo() {
        return {
            currentType: this.getCurrentGrenadeType(),
            config: this.getCurrentGrenade(),
            cooldownEnd: this.grenadeCooldownEnd,
            canThrow: this.canThrowGrenade()
        };
    }

    getMedkitInfo() {
        const info = {};
        for (const [type, medkit] of Object.entries(this.medkits)) {
            info[type] = {
                config: medkit.config,
                cooldownEnd: medkit.cooldownEndTime,
                isUsing: medkit.isUsing,
                canUse: medkit.canUse()
            };
        }
        return info;
    }

    destroy() {
        this.bulletManager.destroyAll();

        // Destroy all grenades
        for (const grenade of this.grenades) {
            grenade.destroy();
        }

        // Destroy all medkits
        for (const medkit of Object.values(this.medkits)) {
            medkit.destroy();
        }

        if (this.energyDrinkEffect) {
            this.energyDrinkEffect.destroy();
        }

        super.destroy();
    }
}