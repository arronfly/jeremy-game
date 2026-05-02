class Bot extends Player {
    constructor(x, y, team) {
        super(x, y, team, false);
        this.name = team === 'red' ? `红队Bot_${Math.floor(Math.random() * 99) + 1}`
                                   : `蓝队Bot_${Math.floor(Math.random() * 99) + 1}`;

        this.target = null;
        this.state = 'wander'; // wander, chase, attack, retreat
        this.stateChangeTime = 0;
        this.lastSawEnemy = 0;
        this.lastShotTime = 0;
        this.wanderTarget = { x: 0, y: 0 };
        this.pickNewWanderTarget();
    }

    pickNewWanderTarget() {
        this.wanderTarget = {
            x: 100 + Math.random() * 1000,
            y: 100 + Math.random() * 600
        };
    }

    findNearestEnemy(enemies) {
        let nearest = null;
        let nearestDist = Infinity;

        enemies.forEach(e => {
            if (!e.alive) return;
            const dx = e.x - this.x;
            const dy = e.y - this.y;
            const dist = Math.sqrt(dx * dx + dy * dy);
            if (dist < nearestDist) {
                nearestDist = dist;
                nearest = e;
            }
        });

        return { enemy: nearest, dist: nearestDist };
    }

    update(time, enemies, obstacles = []) {
        if (!this.alive) {
            this.respawn(time);
            return [];
        }

        // Weapon update
        this.weapons.forEach(w => w.update(time));

        const { enemy, dist } = this.findNearestEnemy(enemies);
        this.target = enemy;

        // State machine
        if (time > this.stateChangeTime) {
            if (!enemy || dist > 400) {
                this.state = 'wander';
                this.stateChangeTime = time + 2000 + Math.random() * 2000;
            } else if (dist < 150) {
                this.state = 'retreat';
                this.stateChangeTime = time + 1000 + Math.random() * 1000;
            } else {
                this.state = Math.random() > 0.3 ? 'chase' : 'attack';
                this.stateChangeTime = time + 500 + Math.random() * 1500;
            }
        }

        let targetX = this.wanderTarget.x;
        let targetY = this.wanderTarget.y;
        let shouldChase = false;
        let shouldAttack = false;

        if (enemy) {
            const enemyAngle = Math.atan2(enemy.y - this.y, enemy.x - this.x);

            if (this.state === 'chase') {
                targetX = enemy.x;
                targetY = enemy.y;
                shouldChase = true;
            } else if (this.state === 'attack') {
                targetX = enemy.x;
                targetY = enemy.y;
                shouldAttack = true;
                this.angle = enemyAngle;
            } else if (this.state === 'retreat') {
                targetX = this.x - Math.cos(enemyAngle) * 200;
                targetY = this.y - Math.sin(enemyAngle) * 200;
            } else {
                this.angle = enemyAngle;
            }
        }

        // Movement
        if (this.state !== 'attack' || !enemy) {
            const dx = targetX - this.x;
            const dy = targetY - this.y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist > 30) {
                const moveSpeed = this.state === 'retreat' ? this.speed * 1.2 : this.speed;
                this.x += (dx / dist) * moveSpeed;
                this.y += (dy / dist) * moveSpeed;
                this.angle = Math.atan2(dy, dx);
            } else if (this.state === 'wander') {
                this.pickNewWanderTarget();
            }
        }

        // Obstacle avoidance
        obstacles.forEach(obs => {
            const dx = this.x - obs.x;
            const dy = this.y - obs.y;
            const dist = Math.sqrt(dx * dx + dy * dy);
            const minDist = this.radius + obs.w/2;
            if (dist < minDist && dist > 0) {
                this.x += (dx / dist) * 3;
                this.y += (dy / dist) * 3;
            }
        });

        // Bounds
        this.x = Math.max(this.radius, Math.min(1200 - this.radius, this.x));
        this.y = Math.max(this.radius, Math.min(800 - this.radius, this.y));

        // Shooting
        if (enemy && shouldAttack && dist < 500) {
            this.angle = Math.atan2(enemy.y - this.y, enemy.x - this.x);
            const bullet = this.weapon.fire(time, this.x, this.y, this.angle, this.team);
            if (bullet) this.bullets.push(bullet);
        }

        // Update bullets
        this.bullets = this.bullets.filter(b => {
            b.x += b.vx;
            b.y += b.vy;
            b.trail.push({ x: b.x, y: b.y });
            if (b.trail.length > 5) b.trail.shift();
            return b.x > 0 && b.x < 1200 && b.y > 0 && b.y < 800;
        });

        // Random weapon switch
        if (Math.random() < 0.002) {
            this.switchWeapon();
        }

        // Reload when low ammo
        if (this.weapon.currentAmmo < 5 && !this.weapon.isReloading) {
            this.weapon.startReload(time);
        }

        return this.bullets;
    }
}
