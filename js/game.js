class Game {
    constructor() {
        this.canvas = document.getElementById('gameCanvas');
        this.ctx = this.canvas.getContext('2d');
        this.canvas.width = 1200;
        this.canvas.height = 800;

        this.running = false;
        this.gameOver = false;
        this.winScore = 25;

        this.redScore = 0;
        this.blueScore = 0;
        this.startTime = 0;
        this.elapsedTime = 0;

        this.redTeam = [];
        this.blueTeam = [];
        this.player = null;
        this.kills = [];

        this.keys = {};
        this.mousePos = { x: 0, y: 0 };
        this.mouseDown = false;

        this.obstacles = [
            { x: 300, y: 200, w: 80, h: 80, color: '#3d3d5c' },
            { x: 600, y: 350, w: 120, h: 100, color: '#3d3d5c' },
            { x: 900, y: 200, w: 80, h: 80, color: '#3d3d5c' },
            { x: 200, y: 500, w: 100, h: 60, color: '#3d3d5c' },
            { x: 500, y: 600, w: 80, h: 80, color: '#3d3d5c' },
            { x: 800, y: 550, w: 100, h: 80, color: '#3d3d5c' },
            { x: 1050, y: 400, w: 60, h: 120, color: '#3d3d5c' }
        ];

        this.setupEventListeners();
        this.init();
    }

    setupEventListeners() {
        document.addEventListener('keydown', e => {
            this.keys[e.key] = true;
            if (e.key === 'r' || e.key === 'R') {
                if (this.player && this.player.alive) {
                    this.player.weapon.startReload(performance.now());
                }
            }
        });

        document.addEventListener('keyup', e => {
            this.keys[e.key] = false;
        });

        this.canvas.addEventListener('mousemove', e => {
            const rect = this.canvas.getBoundingClientRect();
            this.mousePos.x = e.clientX - rect.left;
            this.mousePos.y = e.clientY - rect.top;
        });

        this.canvas.addEventListener('mousedown', e => {
            if (e.button === 0) this.mouseDown = true;
        });

        this.canvas.addEventListener('mouseup', e => {
            if (e.button === 0) this.mouseDown = false;
        });

        document.getElementById('start-btn').addEventListener('click', () => this.start());
        document.getElementById('restart-btn').addEventListener('click', () => this.restart());
    }

    init() {
        // Create red team (spawn on left)
        this.redTeam = [];
        for (let i = 0; i < 3; i++) {
            const bot = new Bot(
                80 + Math.random() * 100,
                200 + i * 200,
                'red'
            );
            bot.spawnX = bot.x;
            bot.spawnY = bot.y;
            this.redTeam.push(bot);
        }

        // Create blue team (spawn on right)
        this.blueTeam = [];
        for (let i = 0; i < 3; i++) {
            const bot = new Bot(
                1020 + Math.random() * 100,
                200 + i * 200,
                'blue'
            );
            bot.spawnX = bot.x;
            bot.spawnY = bot.y;
            this.blueTeam.push(bot);
        }

        // Create human player on red team
        this.player = new Player(150, 400, 'red', true);
        this.player.spawnX = 150;
        this.player.spawnY = 400;
        this.redTeam.unshift(this.player);

        this.redScore = 0;
        this.blueScore = 0;
        this.updateScoreDisplay();
    }

    start() {
        document.getElementById('start-screen').classList.add('hidden');
        this.running = true;
        this.startTime = performance.now();
        this.gameLoop();
    }

    restart() {
        document.getElementById('game-over').classList.add('hidden');
        this.init();
        this.running = true;
        this.gameOver = false;
        this.startTime = performance.now();
        this.gameLoop();
    }

    addKill(killer, victim) {
        const entry = {
            killer: killer.name,
            killerTeam: killer.team,
            victim: victim.name,
            victimTeam: victim.team,
            time: performance.now()
        };
        this.kills.unshift(entry);
        if (this.kills.length > 5) this.kills.pop();

        this.addKillFeedEntry(entry);

        if (killer.team === 'red') {
            this.redScore++;
        } else {
            this.blueScore++;
        }
        this.updateScoreDisplay();
        this.checkWinCondition();
    }

    addKillFeedEntry(entry) {
        const feed = document.getElementById('kill-feed');
        const div = document.createElement('div');
        div.className = `kill-entry ${entry.killerTeam}-kill`;
        div.innerHTML = `<span style="color:${entry.killerTeam === 'red' ? '#ff6b6b' : '#4ecdc4'}">${entry.killer}</span> 击杀了 <span style="color:${entry.victimTeam === 'red' ? '#ff6b6b' : '#4ecdc4'}">${entry.victim}</span>`;
        feed.appendChild(div);
        setTimeout(() => div.remove(), 5000);
    }

    updateScoreDisplay() {
        document.getElementById('red-score').textContent = this.redScore;
        document.getElementById('blue-score').textContent = this.blueScore;
    }

    checkWinCondition() {
        if (this.redScore >= this.winScore) {
            this.endGame('red');
        } else if (this.blueScore >= this.winScore) {
            this.endGame('blue');
        }
    }

    endGame(winner) {
        this.running = false;
        this.gameOver = true;

        const winnerText = document.getElementById('winner-text');
        const teamName = winner === 'red' ? '红队' : '蓝队';
        winnerText.textContent = `${teamName}胜利!`;
        winnerText.className = `${winner}-winner`;

        document.getElementById('final-red').textContent = this.redScore;
        document.getElementById('final-blue').textContent = this.blueScore;
        document.getElementById('game-over').classList.remove('hidden');
    }

    update(time) {
        // Update player
        this.player.update(time, this.keys, this.mousePos, this.mouseDown);

        // Update red bots
        this.redTeam.forEach((p, i) => {
            if (!p.isHuman) {
                p.update(time, this.blueTeam, this.obstacles);
            }
        });

        // Update blue bots
        this.blueTeam.forEach(p => {
            p.update(time, this.redTeam, this.obstacles);
        });

        // Check bullet collisions
        // Red team bullets hitting blue team
        this.redTeam.forEach(attacker => {
            if (!attacker.alive) return;
            attacker.bullets = attacker.bullets.filter(bullet => {
                for (const target of this.blueTeam) {
                    if (!target.alive) continue;
                    const dx = bullet.x - target.x;
                    const dy = bullet.y - target.y;
                    const dist = Math.sqrt(dx * dx + dy * dy);
                    if (dist < target.radius + 4) {
                        const result = target.takeDamage(bullet.damage, attacker);
                        if (result.killed) {
                            this.addKill(attacker, target);
                        }
                        return false;
                    }
                }
                return true;
            });
        });

        // Blue team bullets hitting red team
        this.blueTeam.forEach(attacker => {
            if (!attacker.alive) return;
            attacker.bullets = attacker.bullets.filter(bullet => {
                for (const target of this.redTeam) {
                    if (!target.alive) continue;
                    const dx = bullet.x - target.x;
                    const dy = bullet.y - target.y;
                    const dist = Math.sqrt(dx * dx + dy * dy);
                    if (dist < target.radius + 4) {
                        const result = target.takeDamage(bullet.damage, attacker);
                        if (result.killed) {
                            this.addKill(attacker, target);
                        }
                        return false;
                    }
                }
                return true;
            });
        });

        // Update UI
        this.updateTimer();
        this.updatePlayerStats();
    }

    updateTimer() {
        const elapsed = Math.floor((performance.now() - this.startTime) / 1000);
        const mins = Math.floor(elapsed / 60).toString().padStart(2, '0');
        const secs = (elapsed % 60).toString().padStart(2, '0');
        document.getElementById('timer').textContent = `${mins}:${secs}`;
    }

    updatePlayerStats() {
        if (this.player) {
            const healthPct = (this.player.health / this.player.maxHealth) * 100;
            document.getElementById('health-fill').style.width = `${healthPct}%`;
            document.getElementById('health-text').textContent = Math.max(0, Math.floor(this.player.health));
            document.getElementById('ammo-count').textContent = this.player.weapon.currentAmmo;
            document.getElementById('ammo-reserve').textContent = this.player.weapon.reserveAmmo;
            document.getElementById('weapon-name').textContent = this.player.weapon.name;
        }
    }

    draw(time) {
        // Clear
        this.ctx.fillStyle = '#2d2d44';
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

        // Grid pattern
        this.ctx.strokeStyle = '#3d3d5c';
        this.ctx.lineWidth = 1;
        for (let x = 0; x < this.canvas.width; x += 50) {
            this.ctx.beginPath();
            this.ctx.moveTo(x, 0);
            this.ctx.lineTo(x, this.canvas.height);
            this.ctx.stroke();
        }
        for (let y = 0; y < this.canvas.height; y += 50) {
            this.ctx.beginPath();
            this.ctx.moveTo(0, y);
            this.ctx.lineTo(this.canvas.width, y);
            this.ctx.stroke();
        }

        // Spawn zones
        this.ctx.fillStyle = 'rgba(255, 107, 107, 0.1)';
        this.ctx.fillRect(0, 0, 200, 800);
        this.ctx.fillStyle = 'rgba(78, 205, 196, 0.1)';
        this.ctx.fillRect(1000, 0, 200, 800);

        // Obstacles
        this.obstacles.forEach(obs => {
            this.ctx.fillStyle = obs.color;
            this.ctx.fillRect(obs.x - obs.w/2, obs.y - obs.h/2, obs.w, obs.h);
            this.ctx.strokeStyle = '#555';
            this.ctx.lineWidth = 2;
            this.ctx.strokeRect(obs.x - obs.w/2, obs.y - obs.h/2, obs.w, obs.h);
        });

        // Draw all players
        [...this.redTeam, ...this.blueTeam].forEach(p => p.draw(this.ctx, time));

        // Center line
        this.ctx.strokeStyle = 'rgba(255, 255, 255, 0.2)';
        this.ctx.setLineDash([10, 10]);
        this.ctx.beginPath();
        this.ctx.moveTo(600, 0);
        this.ctx.lineTo(600, 800);
        this.ctx.stroke();
        this.ctx.setLineDash([]);
    }

    gameLoop() {
        if (!this.running) return;

        const time = performance.now();
        this.update(time);
        this.draw(time);

        requestAnimationFrame(() => this.gameLoop());
    }
}

// Start game when page loads
window.addEventListener('load', () => {
    window.game = new Game();
});
