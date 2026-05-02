/**
 * GarageMap - "假车库" style map for the Phaser.js game
 *
 * Map layout:
 * - Red garage (left side) and Blue garage (right side) - 200x200 size with roofs
 * - Windows in garages that allow shooting through
 * - Internal walls that provide cover
 * - Open center area with obstacles
 * - Spawn zones marked for each team
 */

const Phaser = window.Phaser;

export class GarageMap {
    constructor(scene) {
        this.scene = scene;
        this.mapWidth = 1600;
        this.mapHeight = 1200;
        this.tileSize = 64;

        // Map elements containers
        this.walls = null;      // Solid walls - block movement and bullets
        this.covers = null;     // Cover objects - block bullets but not movement
        this.spawnZones = null; // Team spawn areas
        this.windows = null;    // Semi-penetrable windows

        // Colors
        this.colors = {
            floor: 0x3d3d5c,
            floorAlt: 0x353550,
            red: 0xE74C3C,
            blue: 0x3498DB,
            wall: 0x4a4a6a,
            wallDark: 0x2a2a4a,
            roof: 0x2a2a3a,
            cover: 0x5a5a7a,
            window: 0x88ccff,
            windowFrame: 0x4a4a6a,
            spawnRed: 0xE74C3C,
            spawnBlue: 0x3498DB
        };

        // Collision types
        this.COLLISION_TYPES = {
            SOLID: 1,      // Blocks movement and bullets
            COVER: 2,      // Blocks bullets only
            WINDOW: 3,    // Semi-penetrable
            SPAWN: 4       // No collision, just visual marker
        };
    }

    /**
     * Create the entire map
     */
    create() {
        // Create texture atlas first
        this.createTextures();

        // Create layer containers
        this.createLayers();

        // Draw floor
        this.createFloor();

        // Create garages (left = red, right = blue)
        this.createRedGarage();
        this.createBlueGarage();

        // Create internal walls and cover positions
        this.createInternalWalls();

        // Create center area obstacles
        this.createCenterObstacles();

        // Create spawn zones
        this.createSpawnZones();

        // Setup collisions
        this.setupCollisions();
    }

    /**
     * Create programmatic textures for walls, floors, etc.
     */
    createTextures() {
        // Floor tile texture
        this.createFloorTexture();

        // Wall texture
        this.createWallTexture();

        // Cover texture (crates/barriers)
        this.createCoverTexture();

        // Roof texture for garages
        this.createRoofTexture();

        // Spawn zone texture
        this.createSpawnTexture();
    }

    createFloorTexture() {
        const graphics = this.scene.make.graphics({ x: 0, y: 0, add: false });

        // Main floor tile
        graphics.fillStyle(this.colors.floor);
        graphics.fillRect(0, 0, 64, 64);

        // Add some texture variation
        graphics.fillStyle(this.colors.floorAlt);
        graphics.fillRect(4, 4, 24, 24);
        graphics.fillRect(36, 36, 24, 24);

        // Add subtle grid lines
        graphics.lineStyle(1, 0x333344, 0.3);
        graphics.strokeRect(0, 0, 64, 64);

        graphics.generateTexture('floor_tile', 64, 64);
        graphics.destroy();
    }

    createWallTexture() {
        const graphics = this.scene.make.graphics({ x: 0, y: 0, add: false });

        // Main wall color
        graphics.fillStyle(this.colors.wall);
        graphics.fillRect(0, 0, 64, 64);

        // Add 3D effect - top highlight
        graphics.fillStyle(0x6a6a8a);
        graphics.fillRect(0, 0, 64, 4);

        // Add 3D effect - bottom shadow
        graphics.fillStyle(this.colors.wallDark);
        graphics.fillRect(0, 60, 64, 4);

        // Add brick-like pattern
        graphics.lineStyle(1, 0x3a3a5a, 0.5);
        graphics.strokeRect(0, 8, 64, 24);
        graphics.strokeRect(0, 40, 64, 24);

        graphics.generateTexture('wall_tile', 64, 64);
        graphics.destroy();
    }

    createCoverTexture() {
        const graphics = this.scene.make.graphics({ x: 0, y: 0, add: false });

        // Crate color
        graphics.fillStyle(this.colors.cover);
        graphics.fillRect(0, 0, 64, 64);

        // Add wood-like grain
        graphics.fillStyle(0x4a4a6a);
        graphics.fillRect(2, 2, 60, 4);
        graphics.fillRect(2, 14, 60, 4);
        graphics.fillRect(2, 26, 60, 4);
        graphics.fillRect(2, 38, 60, 4);
        graphics.fillRect(2, 50, 60, 4);

        // Add darker edges
        graphics.fillStyle(0x3a3a5a);
        graphics.fillRect(0, 0, 2, 64);
        graphics.fillRect(62, 0, 2, 64);

        graphics.generateTexture('cover_crate', 64, 64);
        graphics.destroy();
    }

    createRoofTexture() {
        const graphics = this.scene.make.graphics({ x: 0, y: 0, add: false });

        // Main roof color
        graphics.fillStyle(this.colors.roof);
        graphics.fillRect(0, 0, 64, 64);

        // Add roof tile pattern
        graphics.fillStyle(0x1a1a2a);
        for (let row = 0; row < 4; row++) {
            for (let col = 0; col < 4; col++) {
                const offsetX = (row % 2) * 8;
                graphics.fillRect(col * 16 + offsetX, row * 16, 14, 2);
            }
        }

        graphics.generateTexture('roof_tile', 64, 64);
        graphics.destroy();
    }

    createSpawnTexture() {
        const graphics = this.scene.make.graphics({ x: 0, y: 0, add: false });

        // Semi-transparent team color
        graphics.fillStyle(0xffffff, 0.2);
        graphics.fillRect(0, 0, 64, 64);

        // Add border
        graphics.lineStyle(2, 0xffffff, 0.4);
        graphics.strokeRect(2, 2, 60, 60);

        // Add diagonal lines
        graphics.lineStyle(1, 0xffffff, 0.2);
        for (let i = 0; i < 8; i++) {
            graphics.lineBetween(i * 10, 0, i * 10 + 64, 64);
        }

        graphics.generateTexture('spawn_zone', 64, 64);
        graphics.destroy();
    }

    /**
     * Create layer containers
     */
    createLayers() {
        // Floor layer (bottom)
        this.floorLayer = this.scene.add.group();

        // Spawn zones layer
        this.spawnLayer = this.scene.add.group();

        // Covers layer (not blocking movement, only bullets)
        this.coversGroup = this.scene.add.group();

        // Walls layer (blocking both movement and bullets)
        this.wallsGroup = this.scene.add.group();

        // Roofs layer (on top of player)
        this.roofsGroup = this.scene.add.group();

        // Windows layer
        this.windowsGroup = this.scene.add.group();
    }

    /**
     * Create the floor with tiles
     */
    createFloor() {
        const tilesX = Math.ceil(this.mapWidth / 64);
        const tilesY = Math.ceil(this.mapHeight / 64);

        for (let x = 0; x < tilesX; x++) {
            for (let y = 0; y < tilesY; y++) {
                const tile = this.scene.add.image(
                    x * 64 + 32,
                    y * 64 + 32,
                    'floor_tile'
                );
                tile.setDepth(0);
                this.floorLayer.add(tile);
            }
        }
    }

    /**
     * Create the red garage on the left side
     * Garage: 200x200 pixels, positioned at top-left area
     */
    createRedGarage() {
        const x = 50;
        const y = 200;
        const width = 200;
        const height = 200;

        // Garage floor (darker inside)
        const floor = this.scene.add.rectangle(x + width/2, y + height/2, width, height, 0x2a2a3a);
        floor.setDepth(1);
        this.coversGroup.add(floor);

        // Outer walls
        this.createWallRect(x, y, 16, height); // Left wall
        this.createWallRect(x, y, width, 16); // Top wall
        this.createWallRect(x, y + height - 16, width, 16); // Bottom wall

        // Create windows (shootable openings in the walls)
        this.createWindow(x, y + 60, 80, 24);
        this.createWindow(x, y + 140, 80, 24);

        // Internal cover (crate in middle of garage)
        this.createCover(x + 70, y + 85, 64, 32);

        // Spawn indicator
        this.createSpawnIndicator(x + width/2, y + height - 50, 'red');

        // Store garage bounds for reference
        this.redGarageBounds = { x, y, width, height };
    }

    /**
     * Create the blue garage on the right side
     */
    createBlueGarage() {
        const x = this.mapWidth - 250;
        const y = 200;
        const width = 200;
        const height = 200;

        // Garage floor (darker inside)
        const floor = this.scene.add.rectangle(x + width/2, y + height/2, width, height, 0x2a2a3a);
        floor.setDepth(1);
        this.coversGroup.add(floor);

        // Outer walls
        this.createWallRect(x + width - 16, y, 16, height); // Right wall
        this.createWallRect(x, y, width, 16); // Top wall
        this.createWallRect(x, y + height - 16, width, 16); // Bottom wall

        // Create windows (shootable openings in the walls)
        this.createWindow(x + width - 80, y + 60, 80, 24);
        this.createWindow(x + width - 80, y + 140, 80, 24);

        // Internal cover (crate in middle of garage)
        this.createCover(x + 70, y + 85, 64, 32);

        // Spawn indicator
        this.createSpawnIndicator(x + width/2, y + height - 50, 'blue');

        // Store garage bounds for reference
        this.blueGarageBounds = { x, y, width, height };
    }

    /**
     * Create a solid wall rectangle
     */
    createWallRect(x, y, width, height) {
        const wall = this.scene.add.rectangle(x + width/2, y + height/2, width, height, this.colors.wall);
        wall.setDepth(2);
        wall.isWall = true;
        this.wallsGroup.add(wall);
        return wall;
    }

    /**
     * Create a cover object (blocks bullets but not movement)
     */
    createCover(x, y, width = 64, height = 64) {
        const cover = this.scene.add.rectangle(x + width/2, y + height/2, width, height, this.colors.cover);
        cover.setDepth(2);
        cover.isCover = true;
        this.coversGroup.add(cover);
        return cover;
    }

    /**
     * Create a window (semi-penetrable)
     */
    createWindow(x, y, width, height) {
        // Window frame (solid)
        const frame = this.scene.add.rectangle(x + width/2, y + height/2, width, height, this.colors.windowFrame);
        frame.setDepth(2);
        this.wallsGroup.add(frame);

        // Window glass (semi-transparent)
        const glass = this.scene.add.rectangle(x + width/2, y + height/2, width - 8, height - 8, this.colors.window, 0.3);
        glass.setDepth(3);
        glass.isWindow = true;
        this.windowsGroup.add(glass);

        return { frame, glass };
    }

    /**
     * Create spawn zone indicator
     */
    createSpawnIndicator(x, y, team) {
        const color = team === 'red' ? this.colors.spawnRed : this.colors.spawnBlue;
        const indicator = this.scene.add.rectangle(x, y, 100, 100, color, 0.3);
        indicator.setDepth(1);
        indicator.setStrokeStyle(2, color, 0.6);
        this.spawnLayer.add(indicator);

        // Add text label
        const label = this.scene.add.text(x, y, team === 'red' ? '红队' : '蓝队', {
            fontSize: '16px',
            color: team === 'red' ? '#E74C3C' : '#3498DB',
            fontFamily: 'Arial'
        }).setOrigin(0.5);
        label.setDepth(2);
        this.spawnLayer.add(label);
    }

    /**
     * Create internal walls around the map
     */
    createInternalWalls() {
        // Top left cover wall
        this.createCover(200, 100, 128, 32);
        this.createWallRect(200, 132, 16, 80);

        // Top right cover wall
        this.createCover(this.mapWidth - 328, 100, 128, 32);
        this.createWallRect(this.mapWidth - 216, 132, 16, 80);

        // Bottom left cover wall
        this.createCover(200, this.mapHeight - 132, 128, 32);
        this.createWallRect(200, this.mapHeight - 212, 16, 80);

        // Bottom right cover wall
        this.createCover(this.mapWidth - 328, this.mapHeight - 132, 128, 32);
        this.createWallRect(this.mapWidth - 216, this.mapHeight - 212, 16, 80);
    }

    /**
     * Create center area obstacles
     */
    createCenterObstacles() {
        const centerX = this.mapWidth / 2;
        const centerY = this.mapHeight / 2;

        // Center cross cover
        this.createCover(centerX - 16, centerY - 100, 32, 200); // Vertical
        this.createCover(centerX - 100, centerY - 16, 200, 32); // Horizontal

        // Corner crates around center
        this.createCover(centerX - 180, centerY - 180, 64, 64);
        this.createCover(centerX + 116, centerY - 180, 64, 64);
        this.createCover(centerX - 180, centerY + 116, 64, 64);
        this.createCover(centerX + 116, centerY + 116, 64, 64);

        // Small obstacles
        this.createCover(centerX - 250, centerY, 48, 48);
        this.createCover(centerX + 202, centerY, 48, 48);
        this.createCover(centerX, centerY - 250, 48, 48);
        this.createCover(centerX, centerY + 202, 48, 48);

        // Additional cover positions near spawn areas
        this.createCover(350, 100, 64, 32);
        this.createCover(350, this.mapHeight - 132, 64, 32);
        this.createCover(this.mapWidth - 414, 100, 64, 32);
        this.createCover(this.mapWidth - 414, this.mapHeight - 132, 64, 32);
    }

    /**
     * Create spawn zones for each team
     */
    createSpawnZones() {
        // Red team spawn (left side, lower area)
        for (let i = 0; i < 3; i++) {
            for (let j = 0; j < 2; j++) {
                const spawn = this.scene.add.rectangle(
                    100 + i * 60,
                    this.mapHeight - 100 - j * 60,
                    50, 50,
                    this.colors.spawnRed,
                    0.2
                );
                spawn.setDepth(0);
                spawn.setStrokeStyle(1, this.colors.spawnRed, 0.4);
                this.spawnLayer.add(spawn);
            }
        }

        // Blue team spawn (right side, lower area)
        for (let i = 0; i < 3; i++) {
            for (let j = 0; j < 2; j++) {
                const spawn = this.scene.add.rectangle(
                    this.mapWidth - 100 - i * 60,
                    this.mapHeight - 100 - j * 60,
                    50, 50,
                    this.colors.spawnBlue,
                    0.2
                );
                spawn.setDepth(0);
                spawn.setStrokeStyle(1, this.colors.spawnBlue, 0.4);
                this.spawnLayer.add(spawn);
            }
        }
    }

    /**
     * Setup collision detection
     */
    setupCollisions() {
        // Store collision objects for reference
        this.collisionObjects = [];

        this.wallsGroup.getChildren().forEach(wall => {
            this.collisionObjects.push({
                x: wall.x - wall.width/2,
                y: wall.y - wall.height/2,
                width: wall.width,
                height: wall.height,
                type: wall.isWindow ? this.COLLISION_TYPES.WINDOW : this.COLLISION_TYPES.SOLID
            });
        });

        this.coversGroup.getChildren().forEach(cover => {
            this.collisionObjects.push({
                x: cover.x - cover.width/2,
                y: cover.y - cover.height/2,
                width: cover.width,
                height: cover.height,
                type: this.COLLISION_TYPES.COVER
            });
        });
    }

    /**
     * Check if a point collides with any wall
     */
    checkCollision(x, y, radius = 10) {
        for (const obj of this.collisionObjects) {
            if (obj.type === this.COLLISION_TYPES.SOLID || obj.type === this.COLLISION_TYPES.COVER) {
                // AABB collision with circle
                const closestX = Phaser.Math.Clamp(x, obj.x, obj.x + obj.width);
                const closestY = Phaser.Math.Clamp(y, obj.y, obj.y + obj.height);
                const distance = Phaser.Math.Distance.Between(x, y, closestX, closestY);

                if (distance < radius) {
                    return {
                        collided: true,
                        type: obj.type,
                        x: closestX,
                        y: closestY,
                        obj: obj
                    };
                }
            }
        }
        return { collided: false };
    }

    /**
     * Check if a bullet collides with any obstacle
     */
    checkBulletCollision(x, y) {
        for (const obj of this.collisionObjects) {
            if (obj.type === this.COLLISION_TYPES.SOLID) {
                if (x >= obj.x && x <= obj.x + obj.width &&
                    y >= obj.y && y <= obj.y + obj.height) {
                    return { hit: true, type: obj.type, obj: obj };
                }
            } else if (obj.type === this.COLLISION_TYPES.COVER) {
                // Cover blocks bullets too
                if (x >= obj.x && x <= obj.x + obj.width &&
                    y >= obj.y && y <= obj.y + obj.height) {
                    return { hit: true, type: obj.type, obj: obj };
                }
            }
            // Windows don't block bullets in this implementation
        }
        return { hit: false };
    }

    /**
     * Get spawn positions for a team
     */
    getSpawnPositions(team) {
        const positions = [];
        const isRed = team === 'red';

        // Spawn area (lower part of the map, toward their garage)
        const baseX = isRed ? 100 : this.mapWidth - 100;
        const startY = this.mapHeight - 100;

        for (let i = 0; i < 3; i++) {
            for (let j = 0; j < 2; j++) {
                positions.push({
                    x: baseX + (isRed ? 1 : -1) * i * 60,
                    y: startY - j * 60
                });
            }
        }

        return positions;
    }

    /**
     * Get the garage bounds for a team
     */
    getGarageBounds(team) {
        return team === 'red' ? this.redGarageBounds : this.blueGarageBounds;
    }

    /**
     * Get all collision objects for external use
     */
    getCollisionObjects() {
        return this.collisionObjects;
    }

    /**
     * Get walls group for rendering order
     */
    getWallsGroup() {
        return this.wallsGroup;
    }

    /**
     * Get covers group
     */
    getCoversGroup() {
        return this.coversGroup;
    }

    /**
     * Get roofs group (for depth sorting)
     */
    getRoofsGroup() {
        return this.roofsGroup;
    }
}

export default GarageMap;