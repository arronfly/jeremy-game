import { MEDICALS } from '../config/medicals.js';

export default class Medkit {
    constructor(scene, type, owner) {
        this.scene = scene;
        this.type = type;
        this.owner = owner;
        this.config = MEDICALS[type];

        this.isActive = false;
        this.isUsing = false;
        this.useStartTime = 0;
        this.lastUseTime = 0;

        // Cooldown tracking
        this.cooldownEndTime = 0;

        // Effect tracking
        this.effectEndTime = 0;
        this.originalSpeed = 0;
    }

    canUse() {
        const now = this.scene.time.now;
        return !this.isUsing &&
               now >= this.cooldownEndTime &&
               this.owner.alive &&
               this.owner.health < this.owner.maxHealth;
    }

    startUse() {
        if (!this.canUse()) return false;

        this.isUsing = true;
        this.useStartTime = this.scene.time.now;
        this.isActive = true;

        // Play use animation on owner
        if (this.owner.playUseAnimation) {
            this.owner.playUseAnimation(this.type);
        }

        return true;
    }

    update(delta) {
        if (!this.isUsing) return;

        const elapsed = this.scene.time.now - this.useStartTime;
        const useTime = this.config.useTime;

        // Update progress
        if (this.owner.updateUseProgress) {
            this.owner.updateUseProgress(this.type, elapsed / useTime);
        }

        // Check if use is complete
        if (elapsed >= useTime) {
            this.completeUse();
        }
    }

    completeUse() {
        const now = this.scene.time.now;

        // Apply healing
        if (this.type === 'firstAidKit' || this.type === 'bandage') {
            const healAmount = this.config.healAmount;
            this.owner.health = Math.min(this.owner.maxHealth, this.owner.health + healAmount);

            // Show heal effect
            this.showHealEffect(healAmount);
        }
        else if (this.type === 'energyDrink') {
            // Apply speed boost
            this.originalSpeed = this.owner.speed || 150;
            const boostAmount = this.originalSpeed * (1 + this.config.speedBoost);
            this.owner.speed = boostAmount;

            // Also heal over time
            const healAmount = this.config.healAmount;
            this.owner.health = Math.min(this.owner.maxHealth, this.owner.health + healAmount);

            // Set effect duration
            this.effectEndTime = now + this.config.duration;
            this.showBuffEffect();
        }

        this.isUsing = false;
        this.isActive = false;
        this.cooldownEndTime = now + this.config.cooldown;

        // Clear progress
        if (this.owner.clearUseProgress) {
            this.owner.clearUseProgress(this.type);
        }
    }

    showHealEffect(amount) {
        // Floating heal number
        const text = this.scene.add.text(
            this.owner.x,
            this.owner.y - 40,
            `+${amount}`,
            {
                fontSize: '20px',
                color: '#27AE60',
                stroke: '#fff',
                strokeThickness: 2
            }
        );
        text.setDepth(100);

        // Animate floating up and fading
        this.scene.tweens.add({
            targets: text,
            y: text.y - 30,
            alpha: 0,
            duration: 1000,
            onComplete: () => text.destroy()
        });

        // Green particles around player
        const particles = this.scene.add.particles(this.owner.x, this.owner.y, 'particle', {
            speed: { min: 50, max: 100 },
            scale: { start: 0.5, end: 0 },
            lifespan: 500,
            tint: 0x27AE60,
            quantity: 5,
            emitting: false
        });
        particles.explode();
        this.scene.time.delayedCall(500, () => particles.destroy());
    }

    showBuffEffect() {
        // Blue glow effect on player
        const glow = this.scene.add.graphics();
        glow.setDepth(2);

        let elapsed = 0;
        const duration = this.config.duration;
        const interval = 100;
        let lastPulse = 0;

        const pulseTimer = this.scene.time.addEvent({
            delay: 16,
            callback: () => {
                elapsed += 16;

                // Pulse effect
                if (elapsed - lastPulse > interval) {
                    lastPulse = elapsed;
                    glow.clear();
                    glow.lineStyle(2, 0x3498DB, 0.5);
                    glow.strokeCircle(this.owner.x, this.owner.y, 30 + Math.sin(elapsed / 100) * 5);
                }

                if (elapsed >= duration) {
                    pulseTimer.remove();
                    glow.destroy();
                }
            }
        });
    }

    // Cancel use (when taking damage or moving)
    cancelUse() {
        if (!this.isUsing) return;

        this.isUsing = false;
        this.isActive = false;

        if (this.owner.clearUseProgress) {
            this.owner.clearUseProgress(this.type);
        }
    }

    // Check if speed buff is still active
    isBuffActive() {
        return this.type === 'energyDrink' && this.scene.time.now < this.effectEndTime;
    }

    // Restore original speed when buff ends
    checkBuffEnd() {
        if (this.type === 'energyDrink' && this.scene.time.now >= this.effectEndTime) {
            if (this.originalSpeed > 0) {
                this.owner.speed = this.originalSpeed;
                this.originalSpeed = 0;
            }
        }
    }

    destroy() {
        this.cancelUse();
    }
}

// Helper class for energy drink continuous healing
export class EnergyDrinkEffect {
    constructor(scene, owner, config) {
        this.scene = scene;
        this.owner = owner;
        this.config = config;

        this.startTime = scene.time.now;
        this.healPerTick = config.healAmount / (config.duration / 1000); // Heal per second
        this.tickedAmount = 0;

        this.isActive = true;
    }

    update(delta) {
        if (!this.isActive || !this.owner.alive) {
            this.isActive = false;
            return;
        }

        // Check if duration has passed
        if (this.scene.time.now >= this.startTime + this.config.duration) {
            this.isActive = false;
            return;
        }

        // Continuous healing (tick every 500ms)
        const elapsed = this.scene.time.now - this.startTime;
        const expectedTicks = Math.floor(elapsed / 500);
        const currentTicks = Math.floor(this.tickedAmount);

        if (expectedTicks > currentTicks) {
            const healAmount = this.healPerTick * (expectedTicks - currentTicks) * 0.5;
            this.owner.health = Math.min(this.owner.maxHealth, this.owner.health + healAmount);
            this.tickedAmount = expectedTicks;
        }
    }

    destroy() {
        this.isActive = false;
    }
}