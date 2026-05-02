class Player {
    constructor(x, y, team, isHuman = false) {
        this.x = x;
        this.y = y;
        this.team = team;
        this.isHuman = isHuman;

        this.speed = 4;
        this.radius = 18;
        this.health = 100;
        this.maxHealth = 100;

        this.angle = team === 'red' ? 0 : Math.PI;
        this.weapons = [
            new Weapon('assault_rifle'),
            new Weapon('sniper')
        ];
        this.currentWeaponIndex = 0;

        this.color = team === 'red' ? '#ff6b6b' : '#4ecdc4';
        this.name = isHuman ? 'Jeremy' : `Bot_${Math.floor(Math.random() * 100)}`;

        this.alive = true;
        this.respawnTime = 0;
        this.spawnX = x;
        this.spawnY = y;

        this.bullets = [];
        this.hitbox = { x: 0, y: 0, w: 30, h: 30 };
    }

    get weapon() {
        return this.weapons[this.currentWeaponIndex];
    }

    switchWeapon() {
        this.currentWeaponIndex = (this.currentWeaponIndex + 1) % this.weapons.length;
    }

    takeDamage(damage, attacker) {
        if (!this.alive) return;

        this.health -= damage;

        if (this.health <= 0) {
            this.health = 0;
            this.alive = false;
            this.respawnTime = performance.now() + 3000;
            return { killed: true, killer: attacker };
        }
        return { killed: false };
    }

    respawn(time) {
        if (this.alive) return;
        if (time < this.respawnTime) return;

        this.x = this.spawnX + (Math.random() - 0.5) * 100;
        this.y = this.spawnY + (Math.random() - 0.5) * 100;
        this.health = this.maxHealth;
        this.alive = true;
        this.weapons.forEach(w => {
            w.currentAmmo = w.magazineSize;
            w.reserveAmmo = w.magazineSize * 3;
            w.isReloading = false;
        });
    }

    update(time, keys = {}, mousePos = {}, mouseDown = false) {
        if (!this.alive) {
            this.respawn(time);
            return;
        }

        // Weapon update
        this.weapon.update(time);

        // Movement
        let dx = 0, dy = 0;
        if (keys['w'] || keys['W'] || keys['ArrowUp']) dy -= 1;
        if (keys['s'] || keys['S'] || keys['ArrowDown']) dy += 1;
        if (keys['a'] || keys['A'] || keys['ArrowLeft']) dx -= 1;
        if (keys['d'] || keys['D'] || keys['ArrowRight']) dx += 1;

        if (dx !== 0 || dy !== 0) {
            const len = Math.sqrt(dx * dx + dy * dy);
            dx /= len;
            dy /= len;
            this.x += dx * this.speed;
            this.y += dy * this.speed;
        }

        // Aim at mouse (for human player)
        if (this.isHuman && mousePos.x !== undefined) {
            this.angle = Math.atan2(mousePos.y - this.y, mousePos.x - this.x);
        }

        // Bounds
        this.x = Math.max(this.radius, Math.min(1200 - this.radius, this.x));
        this.y = Math.max(this.radius, Math.min(800 - this.radius, this.y));

        // Fire weapon
        if (this.isHuman && mouseDown) {
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
    }

    draw(ctx, time) {
        if (!this.alive) {
            // Draw respawn timer
            const remaining = Math.ceil((this.respawnTime - time) / 1000);
            ctx.fillStyle = 'rgba(0,0,0,0.5)';
            ctx.beginPath();
            ctx.arc(this.x, this.y, 25, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = '#fff';
            ctx.font = 'bold 16px sans-serif';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText(remaining > 0 ? remaining : '!', this.x, this.y);
            return;
        }

        ctx.save();
        ctx.translate(this.x, this.y);

        // Body
        ctx.fillStyle = this.color;
        ctx.beginPath();
        ctx.arc(0, 0, this.radius, 0, Math.PI * 2);
        ctx.fill();

        // Team indicator ring
        ctx.strokeStyle = this.team === 'red' ? '#ff4444' : '#44dddd';
        ctx.lineWidth = 3;
        ctx.stroke();

        // Direction indicator
        ctx.rotate(this.angle);
        ctx.fillStyle = '#fff';
        ctx.fillRect(10, -4, 15, 8);

        ctx.restore();

        // Draw weapon
        this.weapon.draw(ctx, this.x, this.y, this.angle);

        // Draw bullets
        this.bullets.forEach(b => {
            // Trail
            ctx.strokeStyle = this.team === 'red' ? 'rgba(255,107,107,0.5)' : 'rgba(78,205,196,0.5)';
            ctx.lineWidth = 2;
            ctx.beginPath();
            b.trail.forEach((p, i) => {
                if (i === 0) ctx.moveTo(p.x, p.y);
                else ctx.lineTo(p.x, p.y);
            });
            ctx.stroke();

            // Bullet
            ctx.fillStyle = '#ffff00';
            ctx.beginPath();
            ctx.arc(b.x, b.y, 4, 0, Math.PI * 2);
            ctx.fill();
        });

        // Health bar
        if (this.health < this.maxHealth) {
            const barWidth = 40;
            const barHeight = 5;
            ctx.fillStyle = '#333';
            ctx.fillRect(this.x - barWidth/2, this.y - this.radius - 15, barWidth, barHeight);
            ctx.fillStyle = this.health > 30 ? '#4ecdc4' : '#ff4444';
            ctx.fillRect(this.x - barWidth/2, this.y - this.radius - 15, barWidth * (this.health / this.maxHealth), barHeight);
        }

        // Name tag
        ctx.fillStyle = this.isHuman ? '#ffff00' : '#aaa';
        ctx.font = `${this.isHuman ? 'bold ' : ''}11px sans-serif`;
        ctx.textAlign = 'center';
        ctx.fillText(this.name, this.x, this.y - this.radius - 20);
    }

    checkBulletHit(target) {
        if (!target.alive) return false;

        for (let i = this.bullets.length - 1; i >= 0; i--) {
            const b = this.bullets[i];
            const dx = b.x - target.x;
            const dy = b.y - target.y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist < target.radius + 4) {
                this.bullets.splice(i, 1);
                return { bullet: b, damage: b.damage };
            }
        }
        return false;
    }
}
