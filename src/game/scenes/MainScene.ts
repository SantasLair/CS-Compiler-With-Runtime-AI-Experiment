import { Scene, GameObjects } from 'phaser';

export class MainScene extends Scene
{
    background: GameObjects.Image;
    logo: GameObjects.Image;
    title: GameObjects.Text;
    player: GameObjects.Image;
    cursors: Phaser.Types.Input.Keyboard.CursorKeys;
    playerSpeed: number = 4;

    constructor ()
    {
        super('MainScene');
    }

    create ()
    {
    // Create the player using pixel-man.png
    this.player = this.add.image(640/2, 420/2, 'pixel-man');
    this.player.setScale(1); // Optional: scale up the player

        // Enable keyboard input for arrow keys
        if (this.input.keyboard) {
            this.cursors = this.input.keyboard.createCursorKeys();
        }
    }

    update () {
        if (!this.player || !this.cursors) return;

        let dx = 0;
        let dy = 0;
        if (this.cursors.left.isDown) {
            dx -= 1;
        }
        if (this.cursors.right.isDown) {
            dx += 1;
        }
        if (this.cursors.up.isDown) {
            dy -= 1;
        }
        if (this.cursors.down.isDown) {
            dy += 1;
        }

        // Normalize diagonal movement
        if (dx !== 0 && dy !== 0) {
            dx *= Math.SQRT1_2;
            dy *= Math.SQRT1_2;
        }

        this.player.x += dx * this.playerSpeed;
        this.player.y += dy * this.playerSpeed;
    }
}
