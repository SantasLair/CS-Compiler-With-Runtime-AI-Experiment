import { Scene, GameObjects } from 'phaser';

export class Player extends GameObjects.Image {
    static readonly DEFAULT_TEXTURE = 'pixel-man';
    private speed: number = 4;
    cursors: Phaser.Types.Input.Keyboard.CursorKeys;

    constructor(scene: Scene, x: number, y: number, texture: string = Player.DEFAULT_TEXTURE) {
        super(scene, x, y, texture);
        scene.add.existing(this);
        this.setScale(1);
        if (scene.input.keyboard) {
            this.cursors = scene.input.keyboard.createCursorKeys();
        }
    }

    setSpeed(value: number) {
        this.speed = value;
    }

    getSpeed(): number {
        return this.speed;
    }

    update() {
        if (!this.cursors) return;
        let dx = 0;
        let dy = 0;
        if (this.cursors.left.isDown) dx -= 1;
        if (this.cursors.right.isDown) dx += 1;
        if (this.cursors.up.isDown) dy -= 1;
        if (this.cursors.down.isDown) dy += 1;
        if (dx !== 0 && dy !== 0) {
            dx *= Math.SQRT1_2;
            dy *= Math.SQRT1_2;
        }
        this.x += dx * this.speed;
        this.y += dy * this.speed;
    }
}
