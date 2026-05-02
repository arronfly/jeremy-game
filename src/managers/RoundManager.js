import { RULES } from '../config/game.js';

/**
 * RoundManager - Handles round-based gameplay logic
 * 7 rounds total, first to 4 wins
 * When one team is fully eliminated, the other scores
 * At 3:3, final round "sudden death"
 * After each round, 3 second respawn time
 */
export class RoundState {
    static WAITING = 'waiting';      // Waiting for round to start
    static ACTIVE = 'active';        // Round in progress
    static ENDED = 'ended';          // Round ended, scoring
    static RESPAWNING = 'respawning'; // Respawn countdown
    static GAME_OVER = 'game_over';  // Game finished
}

export class RoundManager {
    constructor(scene) {
        this.scene = scene;

        // Round tracking
        this.currentRound = 1;
        this.maxRounds = RULES.maxRounds; // 7 rounds
        this.winScore = RULES.winScore;   // First to 4 wins

        // Scores
        this.scores = {
            red: 0,
            blue: 0
        };

        // Round state
        this.state = RoundState.WAITING;
        this.isSuddenDeath = false;

        // Respawn time
        this.respawnTime = RULES.respawnTime; // 3 seconds

        // Event callbacks
        this.onRoundStart = null;
        this.onRoundEnd = null;
        this.onScoreUpdate = null;
        this.onGameOver = null;
        this.onSuddenDeath = null;

        // Track kills for kill feed
        this.killFeed = [];
        this.maxKillFeedSize = 5;

        // Listeners
        this.setupListeners();
    }

    setupListeners() {
        // Listen for player death to detect team elimination
        this.scene.events.on('playerDied', this.handlePlayerDeath, this);
    }

    startRound() {
        this.state = RoundState.ACTIVE;
        this.isSuddenDeath = this.scores.red === 3 && this.scores.blue === 3;

        if (this.isSuddenDeath && this.onSuddenDeath) {
            this.onSuddenDeath();
        }

        if (this.onRoundStart) {
            this.onRoundStart(this.currentRound, this.isSuddenDeath);
        }
    }

    endRound(winningTeam) {
        if (this.state !== RoundState.ACTIVE) return;

        this.state = RoundState.ENDED;

        if (winningTeam) {
            this.scores[winningTeam]++;
        }

        if (this.onScoreUpdate) {
            this.onScoreUpdate(this.scores.red, this.scores.blue);
        }

        // Check win condition (first to 4)
        if (this.scores.red >= this.winScore) {
            this.handleGameOver('red');
            return;
        }

        if (this.scores.blue >= this.winScore) {
            this.handleGameOver('blue');
            return;
        }

        // Round ended - schedule next round
        this.scheduleNextRound();
    }

    scheduleNextRound() {
        this.state = RoundState.RESPAWNING;

        this.scene.time.delayedCall(this.respawnTime, () => {
            if (this.state === RoundState.RESPAWNING) {
                this.currentRound++;
                if (this.currentRound <= this.maxRounds) {
                    this.startRound();
                }
            }
        });

        if (this.onRoundEnd) {
            this.onRoundEnd(winningTeam, this.scores);
        }
    }

    handleGameOver(winner) {
        this.state = RoundState.GAME_OVER;

        if (this.onGameOver) {
            this.onGameOver(winner, this.scores);
        }
    }

    handlePlayerDeath(player, killer) {
        if (this.state !== RoundState.ACTIVE) return;

        // Add to kill feed
        this.addToKillFeed(player, killer);

        // Check if team is eliminated
        this.checkTeamElimination();
    }

    checkTeamElimination() {
        const redAlive = this.getAlivePlayers('red');
        const blueAlive = this.getAlivePlayers('blue');

        const redTeam = this.scene.players.filter(p => p.team === 'red');
        const blueTeam = this.scene.players.filter(p => p.team === 'blue');

        const allRedDead = redTeam.every(p => !p.isAlive);
        const allBlueDead = blueTeam.every(p => !p.isAlive);

        if (allRedDead && allBlueDead) {
            // Both teams eliminated simultaneously - no score
            this.endRound(null);
        } else if (allRedDead) {
            // Blue team wins round
            this.endRound('blue');
        } else if (allBlueDead) {
            // Red team wins round
            this.endRound('red');
        }
    }

    getAlivePlayers(team) {
        return this.scene.players.filter(p => p.team === team && p.isAlive);
    }

    getAllPlayersDead(team) {
        const teamPlayers = this.scene.players.filter(p => p.team === team);
        return teamPlayers.length > 0 && teamPlayers.every(p => !p.isAlive);
    }

    addToKillFeed(player, killer) {
        const entry = {
            killer: killer ? killer.teamName : 'Unknown',
            victim: player ? player.teamName : 'Unknown',
            timestamp: Date.now()
        };

        this.killFeed.unshift(entry);

        // Keep only last N entries
        if (this.killFeed.length > this.maxKillFeedSize) {
            this.killFeed.pop();
        }
    }

    getKillFeed() {
        return this.killFeed;
    }

    // Get current round info
    getRoundInfo() {
        return {
            currentRound: this.currentRound,
            maxRounds: this.maxRounds,
            scores: { ...this.scores },
            state: this.state,
            isSuddenDeath: this.isSuddenDeath
        };
    }

    // Check if game should end (7 rounds completed without winner - shouldn't happen with first to 4)
    isGameEnded() {
        return this.state === RoundState.GAME_OVER;
    }

    getWinner() {
        if (this.state !== RoundState.GAME_OVER) return null;
        return this.scores.red >= this.winScore ? 'red' : 'blue';
    }

    // Reset for new game
    reset() {
        this.currentRound = 1;
        this.scores = { red: 0, blue: 0 };
        this.state = RoundState.WAITING;
        this.isSuddenDeath = false;
        this.killFeed = [];
    }

    destroy() {
        this.scene.events.off('playerDied', this.handlePlayerDeath, this);
    }
}

export default RoundManager;