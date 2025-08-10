import { Scene, GameObjects } from 'phaser';

export class Player extends GameObjects.Image {
    static readonly DEFAULT_TEXTURE = 'pixel-man';
    private speed: number = 4;
    private tileSize: number = 64;
    private moving: boolean = false;
    private targetX: number = 0;
    private targetY: number = 0;
    private moveDir: { dx: number, dy: number } = { dx: 0, dy: 0 };
    private desiredMoveDir: { dx: number, dy: number } = { dx: 0, dy: 0 };  
    private hasMovementInput: boolean = false;
    private isSnapped: boolean = false;

    cursors: Phaser.Types.Input.Keyboard.CursorKeys;
    private debugDiv: HTMLDivElement;

    private startX: number = 0;
    private startY: number = 0;

    constructor(scene: Scene, x: number, y: number, texture: string = Player.DEFAULT_TEXTURE) {
        super(scene, x, y, texture);
        this.snapToGrid();
        scene.add.existing(this);
        this.setScale(1);
        if (scene.input.keyboard) {
            this.cursors = scene.input.keyboard.createCursorKeys();
        }
        
        // Create debug div
        this.debugDiv = document.createElement('div');
        this.debugDiv.style.position = 'absolute';
        this.debugDiv.style.top = '0';
        this.debugDiv.style.left = '0';
        this.debugDiv.style.background = 'rgba(0,0,0,0.7)';
        this.debugDiv.style.color = 'lime';
        this.debugDiv.style.fontFamily = 'monospace';
        this.debugDiv.style.zIndex = '1000';
        this.debugDiv.style.padding = '4px';
        this.debugDiv.id = 'player-debug';
        document.body.appendChild(this.debugDiv);
    }

    setSpeed(value: number) {
        this.speed = value;
    }

    getSpeed(): number {
        return this.speed;
    }

    private updateDebugInfo() {
        this.debugDiv.innerHTML =
            `x: ${this.x.toFixed(2)}<br>` +
            `y: ${this.y.toFixed(2)}<br>` +
            `moving: ${this.moving}<br>` +
            `targetX: ${this.targetX}<br>` +
            `targetY: ${this.targetY}<br>` +
            `moveDir: dx=${this.moveDir.dx}, dy=${this.moveDir.dy}<br>` +
            `left: ${this.cursors.left.isDown}<br>` +
            `right: ${this.cursors.right.isDown}<br>` +
            `up: ${this.cursors.up.isDown}<br>` +
            `down: ${this.cursors.down.isDown}`;
    }

    snapToGrid() {
        this.x = Math.round(this.x / this.tileSize) * this.tileSize;
        this.y = Math.round(this.y / this.tileSize) * this.tileSize;
        this.isSnapped = true;
    }

    getMoveDirection() {
        let dx = 0, dy = 0;
        if (this.cursors.left.isDown) dx += -1;
        if (this.cursors.right.isDown) dx += 1;
        if (this.cursors.up.isDown) dy += -1;
        if (this.cursors.down.isDown) dy += 1;

        // Normalize diagonal movement
        if (dx !== 0 && dy !== 0) {
            dx *= Math.SQRT1_2;
            dy *= Math.SQRT1_2;
        }

        this.hasMovementInput = (dx !== 0 || dy !== 0);
        return { dx, dy };
    }

    canChangeDirection(): boolean {
        return !this.moving || this.isNearSnapPosition();
    }

    isNearSnapPosition(): boolean {
        const snapX = Math.round(this.x / this.tileSize) * this.tileSize;
        const snapY = Math.round(this.y / this.tileSize) * this.tileSize;
        const dist = Math.sqrt((this.x - snapX) ** 2 + (this.y - snapY) ** 2);
        return dist < this.speed;
    }

    private lerp(a: number, b: number, t: number): number {
        return a + (b - a) * t;
    }

    private moveProgress: number = 0;

    update(time?: number, delta?: number) {
        if (!this.cursors) return;
        if (typeof delta !== 'number') delta = 16; // fallback for frame delta

        // Only process a move if not already moving
        if (!this.moving) {
            let dx = 0, dy = 0;
            if (this.cursors.left.isDown) dx -= 1;
            if (this.cursors.right.isDown) dx += 1;
            if (this.cursors.up.isDown) dy -= 1;
            if (this.cursors.down.isDown) dy += 1;

            // Remove normalization for diagonal movement
            // Each move (including diagonal) moves exactly one tile in each direction

            // Only move if a direction is pressed
            if (dx !== 0 || dy !== 0) {
                this.moveDir = { dx, dy };
                this.startX = Math.round(this.x / this.tileSize) * this.tileSize;
                this.startY = Math.round(this.y / this.tileSize) * this.tileSize;
                this.targetX = this.startX + dx * this.tileSize;
                this.targetY = this.startY + dy * this.tileSize;
                this.moveProgress = 0;
                this.moving = true;
            }
        }

        // Move toward target if moving
        if (this.moving) {
            // Calculate progress based on speed and delta
            let moveSteps = this.tileSize;
            if (this.moveDir.dx != 0 && this.moveDir.dy != 0) {
                // add extra steps for diagonals
                moveSteps += moveSteps *= Math.SQRT1_2;
            }
            this.moveProgress += (this.speed / moveSteps) * (delta / 16);
            if (this.moveProgress >= 1) {
                this.x = this.targetX;
                this.y = this.targetY;
                this.moving = false;
            } else {
                this.x = this.lerp(this.startX, this.targetX, this.moveProgress);
                this.y = this.lerp(this.startY, this.targetY, this.moveProgress);
            }
        }

        // At end of update, show debug info
        this.updateDebugInfo();
    }
}
