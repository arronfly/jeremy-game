import Player from './Player.js';

export default class Bot extends Player {
    constructor(scene, x, y, team) {
        super(scene, x, y, team);

        this.isBot = true;
        this.state = 'idle'; // idle, patrol, chase, attack
        this.target = null;
        this.patrolPoints = [];
        this.currentPatrolIndex = 0;
        this.lastSeenEnemy = null;
        this.lastAttackTime = 0;
        this.attackCooldown = 500; // ms between attacks

        // AI decision making
        this.decisionInterval = null;
        this.startAI();
    }

    startAI() {
        // AI makes decisions every 100ms
        this.decisionInterval = this.scene.time.addEvent({
            delay: 100,
            callback: this.makeDecision,
            callbackScope: this,
            loop: true
        });
    }

    stopAI() {
        if (this.decisionInterval) {
            this.decisionInterval.destroy();
            this.decisionInterval = null;
        }
    }

    setPatrolPoints(points) {
        this.patrolPoints = points;
        this.currentPatrolIndex = 0;
    }

    makeDecision() {
        if (!this.isAlive) return;

        // Find nearest enemy
        this.findNearestEnemy();

        switch (this.state) {
            case 'idle':
                this.doIdle();
                break;
            case 'patrol':
                this.doPatrol();
                break;
            case 'chase':
                this.doChase();
                break;
            case 'attack':
                this.doAttack();
                break;
        }
    }

    findNearestEnemy() {
        const enemies = this.scene.getEnemies ? this.scene.getEnemies(this.team) : [];
        let nearest = null;
        let nearestDist = Infinity;

        for (const enemy of enemies) {
            if (!enemy.isAlive) continue;
            const dist = Phaser.Math.Distance.Between(this.x, this.y, enemy.x, enemy.y);
            if (dist < nearestDist) {
                nearestDist = dist;
                nearest = enemy;
            }
        }

        this.target = nearest;

        // Update state based on target distance
        if (nearest) {
            if (nearestDist < 200) {
                this.state = 'attack';
            } else if (nearestDist < 400) {
                this.state = 'chase';
            } else {
                this.state = 'patrol';
            }
        } else {
            this.state = 'patrol';
        }
    }

    doIdle() {
        // Stand still, maybe look around
        this.playAnimation('idle');
    }

    doPatrol() {
        if (this.patrolPoints.length === 0) {
            // Default patrol - move back and forth
            this.patrolPoints = [
                { x: 150, y: 600 },
                { x: 1450, y: 600 }
            ];
        }

        const target = this.patrolPoints[this.currentPatrolIndex];
        const dist = Phaser.Math.Distance.Between(this.x, this.y, target.x, target.y);

        if (dist < 30) {
            // Reached current patrol point, go to next
            this.currentPatrolIndex = (this.currentPatrolIndex + 1) % this.patrolPoints.length;
        } else {
            // Move towards patrol point
            this.moveTowards(target.x, target.y);
        }

        this.playAnimation('walk');
    }

    doChase() {
        if (!this.target) return;

        this.moveTowards(this.target.x, this.target.y);
        this.playAnimation('run');
    }

    doAttack() {
        if (!this.target) return;

        const dist = Phaser.Math.Distance.Between(this.x, this.y, this.target.x, this.target.y);
        const angle = Phaser.Math.Angle.Between(this.x, this.y, this.target.x, this.target.y);

        // Aim at target using body container
        this.bodyContainer.setRotation(angle);

        // Move to optimal distance if too far
        if (dist > 200) {
            this.moveTowards(this.target.x, this.target.y);
            this.playAnimation('run');
        } else if (dist < 80) {
            // Too close, back up
            this.moveAwayFrom(this.target.x, this.target.y);
            this.playAnimation('run');
        } else {
            // Attack if cooldown is ready
            const now = this.scene.time.now;
            if (now - this.lastAttackTime > this.attackCooldown) {
                this.shoot(this.target.x, this.target.y);
                this.lastAttackTime = now;
            }
            this.playAnimation('shoot');
        }
    }

    moveTowards(targetX, targetY) {
        const angle = Phaser.Math.Angle.Between(this.x, this.y, targetX, targetY);
        const speed = this.state === 'chase' ? this.speed * 1.2 : this.speed;

        this.x += Math.cos(angle) * speed * 0.1;
        this.y += Math.sin(angle) * speed * 0.1;

        // Keep within bounds
        this.x = Phaser.Math.Clamp(this.x, 50, 1550);
        this.y = Phaser.Math.Clamp(this.y, 50, 1150);
    }

    moveAwayFrom(targetX, targetY) {
        const angle = Phaser.Math.Angle.Between(targetX, targetY, this.x, this.y);
        const speed = this.speed * 0.8;

        this.x += Math.cos(angle) * speed * 0.1;
        this.y += Math.sin(angle) * speed * 0.1;

        // Keep within bounds
        this.x = Phaser.Math.Clamp(this.x, 50, 1550);
        this.y = Phaser.Math.Clamp(this.y, 50, 1150);
    }

    shoot(targetX, targetY) {
        if (!this.isAlive || this.isReloading) return;

        const weapon = this.weaponInventory.getCurrentWeapon();
        if (!weapon) return;

        if (!weapon.canFire()) {
            if (weapon.getMagAmmo() <= 0 && weapon.getReserveAmmo() > 0) {
                this.reload();
            }
            return;
        }

        const fired = weapon.fire();
        if (!fired) return;

        this.isShooting = true;
        this.playAnimation('shoot');

        // Use proper weapon system
        const angle = Phaser.Math.Angle.Between(this.x, this.y, targetX, targetY);
        const weaponConfig = weapon.config;

        this.bulletManager.fireBullet(
            this.x + Math.cos(angle) * 24,
            this.y + Math.sin(angle) * 24,
            angle,
            weaponConfig,
            true
        );

        this.scene.time.delayedCall(Math.min(weaponConfig.fireRate, 200), () => {
            this.isShooting = false;
        });
    }

    checkBulletHit(bullet, targetX, targetY) {
        if (!bullet || !bullet.active) return;

        const dist = Phaser.Math.Distance.Between(bullet.x, bullet.y, targetX, targetY);
        if (dist < 30 && this.target && this.target.isAlive) {
            this.target.takeDamage(bullet.damage || 25);
        }
    }

    die() {
        super.die();
        this.stopAI();
    }

    respawn(x, y) {
        super.respawn(x, y);
        this.startAI();
        this.state = 'idle';
    }

    update(pointer) {
        // Bot doesn't use pointer for aiming, AI controls rotation
        if (!this.isAlive) return;

        // If attacking, face the target using body container
        if (this.target && this.state === 'attack') {
            const angle = Phaser.Math.Angle.Between(this.x, this.y, this.target.x, this.target.y);
            this.bodyContainer.setRotation(angle);
        }

        // Update bullet manager trails
        this.bulletManager.update();
    }
}