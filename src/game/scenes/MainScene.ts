import { Scene, GameObjects } from 'phaser';
import { Player } from '../gameObjects/Player';

export class MainScene extends Scene
{
    background: GameObjects.Image;
    logo: GameObjects.Image;
    title: GameObjects.Text;
    player: Player;

    constructor ()
    {
        super('MainScene');
    }

    create ()
    {
        // Create the player using the Player game object
        this.player = new Player(this, 640/2, 420/2);
    }

    update ()
    {
        if (this.player)
        {
            this.player.update();
        }
    }
}
