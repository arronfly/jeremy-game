import { GRENADES } from '../config/grenades.js';
import { HitEffects } from '../effects/HitEffects.js';

const Phaser = window.Phaser;

export class Grenade {
    constructor(scene, type, owner) {
        this.scene = scene;
        this.type = type;
        this.owner = owner;
        this.config = GRENADES[type];

        this.x = owner.x;
        this.y = owner.y;
        this.z = 0; // Height for parabolic trajectory

        this.isActive = true;
        this.activatedTime = 0;

        // Create sprite
        const textureKey = `${type}_grenade`;
        this.sprite = scene.add.sprite(this.x, this.y, textureKey);
        this.sprite.setDepth(5);

        // Physics
        this.velocityX = 0;
        this.velocityY = 0;
        this.velocityZ = 0;
        this.gravity = 500;

        // Throw power based on hold time
        this.minPower = 200;
        this.maxPower = 600;
        this.holdTime = 0;
    }

    startThrow(angle, holdTime) {
        this.holdTime = Math.min(holdTime, 1000); // Max 1 second hold
        const power = this.minPower + (this.maxPower - this.minPower) * (this.holdTime / 1000);

        // Convert angle to velocity components (parabolic trajectory)
        this.velocityX = Math.cos(angle) * power;
        this.velocityY = Math.sin(angle) * power;
        this.velocityZ = power * 0.7; // Initial upward velocity for arc

        // Rotate sprite in throw direction
        this.sprite.setRotation(angle);
    }

    update(delta) {
        if (!this.isActive) return;

        // Apply gravity to Z axis
        this.velocityZ -= this.gravity * delta;

        // Update position
        this.x += this.velocityX * delta;
        this.y += this.velocityY * delta;
        this.z += this.velocityZ * delta;

        // Check if grenade has landed (z <= 0)
        if (this.z <= 0) {
            this.z = 0;
            this.activate();
        }

        // Update sprite position (add z offset for height)
        this.sprite.setPosition(this.x, this.y - this.z * 0.5);
    }

    activate() {
        this.isActive = false;
        this.sprite.setVisible(false);

        switch (this.config.type) {
            case 'smoke':
                this.createSmokeEffect();
                break;
            case 'flash':
                this.createFlashEffect();
                break;
            case 'frag':
                this.createFragEffect();
                break;
        }

        // Destroy sprite after effect
        this.sprite.destroy();
    }

    createSmokeEffect() {
        const duration = this.config.duration;
        const radius = this.config.radius;

        // Use HitEffects for better smoke rendering if available
        if (this.scene.hitEffects) {
            this.scene.hitEffects.createSmokeEffect(this.x, this.y, radius, duration);
            return;
        }

        // Fallback: original smoke implementation
        // Create smoke particles
        const smokeGraphics = this.scene.add.graphics();
        smokeGraphics.setDepth(3);

        // Animated smoke cloud
        let elapsed = 0;
        const particleInterval = 50;
        let lastParticle = 0;

        const smokeTimer = this.scene.time.addEvent({
            delay: 16,
            callback: () => {
                elapsed += 16;

                // Generate particles
                if (elapsed - lastParticle > particleInterval) {
                    lastParticle = elapsed;

                    // Add new smoke puff
                    const offsetX = (Math.random() - 0.5) * radius;
                    const offsetY = (Math.random() - 0.5) * radius;
                    smokeGraphics.fillStyle(0x888888, 0.6);
                    smokeGraphics.fillCircle(this.x + offsetX, this.y + offsetY, 30 + Math.random() * 20);
                }

                // Fade out smoke
                if (elapsed >= duration) {
                    smokeTimer.remove();
                    smokeGraphics.destroy();
                }
            }
        });

        // Create smoke cover effect (blocks vision)
        const smokeCover = this.scene.add.graphics();
        smokeCover.fillStyle(0x555555, 0.4);
        smokeCover.fillCircle(this.x, this.y, radius);
        smokeCover.setBlendMode(Phaser.BlendModes.NORMAL);
        smokeCover.setDepth(4);

        this.scene.time.delayedCall(duration, () => {
            smokeCover.destroy();
        });
    }

    createFlashEffect() {
        const blindDuration = this.config.blindDuration;
        const radius = this.config.radius;

        // Visual flash effect
        const flash = this.scene.add.graphics();
        flash.fillStyle(0xFFFFFF, 0.8);
        flash.fillCircle(this.x, this.y, 50);
        flash.setDepth(10);

        this.scene.time.delayedCall(100, () => flash.destroy());

        // Check for enemies in range and apply blind
        const enemies = this.scene.getEnemiesForTeam(this.owner.team);
        enemies.forEach(enemy => {
            const dx = enemy.x - this.x;
            const dy = enemy.y - this.y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist <= radius + (enemy.radius || 20)) {
                enemy.applyBlind(blindDuration);
            }
        });

        // Flash ring effect
        const ring = this.scene.add.graphics();
        ring.lineStyle(3, 0xFFFF00, 1);
        ring.strokeCircle(this.x, this.y, 20);

        let ringRadius = 20;
        const expandTimer = this.scene.time.addEvent({
            delay: 16,
            callback: () => {
                ringRadius += 8;
                ring.clear();
                ring.lineStyle(3, 0xFFFF00, Math.max(0, 1 - ringRadius / radius));
                ring.strokeCircle(this.x, this.y, ringRadius);

                if (ringRadius >= radius) {
                    expandTimer.remove();
                    ring.destroy();
                }
            }
        });
    }

    createFragEffect() {
        const damage = this.config.damage;
        const radius = this.config.radius;

        // Explosion visual effect
        const explosion = this.scene.add.graphics();
        explosion.setDepth(10);

        let explosionRadius = 10;
        const expandTimer = this.scene.time.addEvent({
            delay: 16,
            callback: () => {
                explosionRadius += 15;
                explosion.clear();

                // Outer ring
                explosion.fillStyle(0xFF6600, Math.max(0, 0.8 - explosionRadius / 100));
                explosion.fillCircle(this.x, this.y, explosionRadius);

                // Inner glow
                explosion.fillStyle(0xFFFF00, Math.max(0, 0.5 - explosionRadius / 150));
                explosion.fillCircle(this.x, this.y, explosionRadius * 0.5);

                if (explosionRadius >= radius) {
                    expandTimer.remove();
                    explosion.destroy();
                }
            }
        });

        // Screen shake effect
        this.scene.cameras.main.shake(200, 0.01);

        // Apply damage to enemies
        const enemies = this.scene.getEnemiesForTeam(this.owner.team);
        enemies.forEach(enemy => {
            const dx = enemy.x - this.x;
            const dy = enemy.y - this.y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist <= radius + (enemy.radius || 20)) {
                // Damage falloff based on distance
                const falloff = 1 - (dist / radius);
                const finalDamage = Math.floor(damage * falloff);
                enemy.takeDamage(finalDamage, this.owner);
            }
        });
    }

    destroy() {
        if (this.sprite && !this.sprite.destroyed) {
            this.sprite.destroy();
        }
    }
}