import Phaser from 'phaser';

export default class Bullet extends Phaser.Physics.Arcade.Sprite {
    constructor(scene, x, y, texture, damage, velocity, isEnemy = false) {
        super(scene, x, y, texture);

        scene.add.existing(this);
        scene.physics.add.existing(this);

        this.damage = damage;
        this.isEnemy = isEnemy;
        this.trailGraphics = null;

        this.setVelocity(velocity.x, velocity.y);
        this.setCircle(4);
        this.setCollideWorldBounds(true);
        this.setBounce(0);

        this.createTrail();

        // Destroy bullet after 3 seconds to prevent memory leaks
        this.lifespan = scene.time.addEvent({
            delay: 3000,
            callback: () => this.destroy(),
            callbackScope: this
        });
    }

    createTrail() {
        this.trailPositions = [];
        this.maxTrailLength = 5;
    }

    updateTrail() {
        this.trailPositions.unshift({ x: this.x, y: this.y });
        if (this.trailPositions.length > this.maxTrailLength) {
            this.trailPositions.pop();
        }
    }

    drawTrail(graphics) {
        if (!this.trailPositions || this.trailPositions.length < 2) return;

        const alphaStep = 0.8 / this.trailPositions.length;
        let alpha = 0.8;

        for (let i = 1; i < this.trailPositions.length; i++) {
            const pos = this.trailPositions[i];
            const prevPos = this.trailPositions[i - 1];
            graphics.lineStyle(2, 0xFFFF00, alpha);
            graphics.lineBetween(prevPos.x, prevPos.y, pos.x, pos.y);
            alpha -= alphaStep;
        }
    }

    destroy() {
        if (this.lifespan) {
            this.lifespan.remove(false);
        }
        if (this.trailGraphics) {
            this.trailGraphics.destroy();
        }
        super.destroy();
    }
}

export class BulletManager {
    constructor(scene) {
        this.scene = scene;
        this.bullets = scene.add.group();
        this.trailGraphics = scene.add.graphics();
    }

    fireBullet(x, y, angle, weaponConfig, isEnemy = false) {
        const bulletCount = weaponConfig.pellets || 1;

        for (let i = 0; i < bulletCount; i++) {
            const spreadAngle = angle + (Math.random() - 0.5) * weaponConfig.spread;
            const velocity = {
                x: Math.cos(spreadAngle) * weaponConfig.bulletSpeed,
                y: Math.sin(spreadAngle) * weaponConfig.bulletSpeed
            };

            const bullet = new Bullet(
                this.scene,
                x, y,
                'bullet',
                weaponConfig.damage,
                velocity,
                isEnemy
            );

            this.bullets.add(bullet);
        }
    }

    update() {
        this.trailGraphics.clear();
        this.bullets.getChildren().forEach(bullet => {
            if (bullet.active) {
                bullet.updateTrail();
                bullet.drawTrail(this.trailGraphics);
            }
        });
    }

    getBullets() {
        return this.bullets;
    }

    destroyAll() {
        this.trailGraphics.destroy();
        this.bullets.destroy(true);
    }
}