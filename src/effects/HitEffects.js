/**
 * HitEffects.js - Particle effects for the game
 * Uses Phaser particle emitters for performant effects
 */

export class HitEffects {
    constructor(scene) {
        this.scene = scene;
        this.emitters = new Map();
        this.createParticles();
    }

    createParticles() {
        // Create particle textures programmatically
        this.createBloodParticle();
        this.createSparkParticle();
        this.createMuzzleFlashParticle();
        this.createShellParticle();
        this.createSmokeParticle();
    }

    createBloodParticle() {
        // Create a small red circle texture for blood
        const graphics = this.scene.add.graphics();
        graphics.fillStyle(0xCC0000, 1);
        graphics.fillCircle(4, 4, 4);
        graphics.generateTexture('bloodParticle', 8, 8);
        graphics.destroy();
    }

    createSparkParticle() {
        // Create a small yellow/orange particle for sparks
        const graphics = this.scene.add.graphics();
        graphics.fillStyle(0xFFAA00, 1);
        graphics.fillCircle(3, 3, 3);
        graphics.generateTexture('sparkParticle', 6, 6);
        graphics.destroy();
    }

    createMuzzleFlashParticle() {
        // Create a bright flash particle
        const graphics = this.scene.add.graphics();
        graphics.fillStyle(0xFFFF00, 1);
        graphics.fillCircle(6, 6, 6);
        graphics.generateTexture('muzzleFlashParticle', 12, 12);
        graphics.destroy();
    }

    createShellParticle() {
        // Create a small brass-colored rectangle for shell casings
        const graphics = this.scene.add.graphics();
        graphics.fillStyle(0xCC9900, 1);
        graphics.fillRect(0, 0, 4, 3);
        graphics.generateTexture('shellParticle', 4, 3);
        graphics.destroy();
    }

    createSmokeParticle() {
        // Create a gray smoke puff particle
        const graphics = this.scene.add.graphics();
        graphics.fillStyle(0x888888, 0.6);
        graphics.fillCircle(20, 20, 20);
        graphics.generateTexture('smokeParticle', 40, 40);
        graphics.destroy();
    }

    /**
     * Blood splatter effect when player is hit
     * @param {number} x - X position of impact
     * @param {number} y - Y position of impact
     * @param {number} angle - Direction of impact (from bullet)
     */
    createBloodEffect(x, y, angle = 0) {
        // Blood emitter - red particles spreading on impact
        const bloodEmitter = this.scene.add.particles(x, y, 'bloodParticle', {
            speed: { min: 50, max: 150 },
            angle: { min: angle - 60, max: angle + 60 },
            scale: { start: 1, end: 0.3 },
            alpha: { start: 0.8, end: 0 },
            lifespan: 400,
            quantity: 8,
            blendMode: 'ADD',
            emitting: false
        });

        // Burst all particles at once
        bloodEmitter.explode(8);

        // Clean up after animation
        this.scene.time.delayedCall(500, () => {
            bloodEmitter.destroy();
        });
    }

    /**
     * Spark effect when bullet hits wall/cover
     * @param {number} x - X position of impact
     * @param {number} y - Y position of impact
     * @param {number} angle - Direction of bullet impact
     */
    createSparkEffect(x, y, angle = 0) {
        // Spark emitter - yellow/orange small fast particles
        const sparkEmitter = this.scene.add.particles(x, y, 'sparkParticle', {
            speed: { min: 100, max: 250 },
            angle: { min: angle - 45, max: angle + 45 },
            scale: { start: 0.8, end: 0.1 },
            alpha: { start: 1, end: 0 },
            lifespan: 200,
            quantity: 5,
            blendMode: 'ADD',
            emitting: false
        });

        sparkEmitter.explode(5);

        this.scene.time.delayedCall(300, () => {
            sparkEmitter.destroy();
        });
    }

    /**
     * Muzzle flash effect when shooting
     * @param {number} x - X position (gun barrel)
     * @param {number} y - Y position (gun barrel)
     * @param {number} angle - Direction of shot
     */
    createMuzzleFlashEffect(x, y, angle) {
        // Bright flash at muzzle position
        const flashEmitter = this.scene.add.particles(x, y, 'muzzleFlashParticle', {
            speed: { min: 10, max: 30 },
            angle: { min: angle - 20, max: angle + 20 },
            scale: { start: 1.2, end: 0.2 },
            alpha: { start: 1, end: 0 },
            lifespan: 80,
            quantity: 3,
            blendMode: 'ADD',
            emitting: false
        });

        flashEmitter.explode(3);

        // Also create a quick bright circle for the flash
        const flashCircle = this.scene.add.graphics();
        flashCircle.fillStyle(0xFFFFAA, 0.9);
        flashCircle.fillCircle(x, y, 12);
        flashCircle.setBlendMode(Phaser.BlendModes.ADD);

        this.scene.tweens.add({
            targets: flashCircle,
            alpha: 0,
            scale: 1.5,
            duration: 50,
            onComplete: () => flashCircle.destroy()
        });

        this.scene.time.delayedCall(100, () => {
            flashEmitter.destroy();
        });
    }

    /**
     * Shell casing particles ejecting from weapon
     * @param {number} x - X position (weapon location)
     * @param {number} y - Y position (weapon location)
     * @param {number} angle - Direction player is facing
     */
    createShellCasingEffect(x, y, angle) {
        // Calculate ejection angle (perpendicular to firing direction)
        const ejectAngle = angle + (Math.random() > 0.5 ? 90 : -90);
        const ejectRad = Phaser.Math.DegToRad(ejectAngle);

        // Shell casing
        const shell = this.scene.add.particles(x, y, 'shellParticle', {
            speed: { min: 80, max: 120 },
            angle: { min: ejectAngle - 15, max: ejectAngle + 15 },
            scale: { start: 1, end: 0.8 },
            alpha: { start: 1, end: 0.5 },
            lifespan: 600,
            quantity: 1,
            gravityY: 300,
            rotate: { min: 0, max: 360 },
            emitting: false
        });

        shell.explode(1);

        this.scene.time.delayedCall(700, () => {
            shell.destroy();
        });
    }

    /**
     * Smoke grenade effect - creates smoke cover area
     * @param {number} x - X position
     * @param {number} y - Y position
     * @param {number} radius - Smoke radius
     * @param {number} duration - Duration in ms
     */
    createSmokeEffect(x, y, radius, duration = 5000) {
        // Create multiple smoke emitters that expand and fade
        const smokeEmitter = this.scene.add.particles(x, y, 'smokeParticle', {
            speed: { min: 10, max: 30 },
            angle: { min: 0, max: 360 },
            scale: { start: 0.5, end: 1.5 },
            alpha: { start: 0.5, end: 0 },
            lifespan: duration,
            quantity: 0,
            frequency: 50,
            blendMode: 'NORMAL',
            emitting: true
        });

        // Stop emitting after duration
        this.scene.time.delayedCall(duration - 500, () => {
            smokeEmitter.stop();
        });

        // Clean up
        this.scene.time.delayedCall(duration + 100, () => {
            smokeEmitter.destroy();
        });

        // Create a semi-transparent overlay for smoke cover
        const smokeCover = this.scene.add.graphics();
        smokeCover.fillStyle(0x555555, 0.35);
        smokeCover.fillCircle(x, y, radius);
        smokeCover.setDepth(4);

        // Animate smoke expansion
        let elapsed = 0;
        const expandInterval = 100;
        let lastExpand = 0;

        const expandTimer = this.scene.time.addEvent({
            delay: 16,
            callback: () => {
                elapsed += 16;

                if (elapsed - lastExpand > expandInterval) {
                    lastExpand = elapsed;
                    // Add new smoke puff with variation
                    const puffX = x + (Math.random() - 0.5) * radius * 1.5;
                    const puffY = y + (Math.random() - 0.5) * radius * 1.5;
                    const puffRadius = 20 + Math.random() * 30;

                    smokeCover.fillStyle(0x666666, 0.3 + Math.random() * 0.2);
                    smokeCover.fillCircle(puffX, puffY, puffRadius);
                }
            }
        });

        // Fade out smoke cover
        this.scene.tweens.add({
            targets: smokeCover,
            alpha: 0,
            duration: 500,
            delay: duration - 500,
            onComplete: () => {
                expandTimer.remove();
                smokeCover.destroy();
            }
        });

        return smokeEmitter;
    }

    /**
     * Destroy all emitters and clean up
     */
    destroy() {
        for (const emitter of this.emitters.values()) {
            emitter.destroy();
        }
        this.emitters.clear();
    }
}

export default HitEffects;
