using System;

namespace PixelManGame.Scenes
{
    public class MainScene : Scene
    {
        public GameObject Background { get; set; }
        public GameObject Logo { get; set; }
        public GameObject Title { get; set; }
        public Player Player { get; set; }

        public MainScene() : base("MainScene")
        {
        }

        public override void Create()
        {
            // Create the player using the Player game object
            Player = new Player(this, 448 / 2, 600);
        }

        public override void Update(float time = 0, float delta = 16)
        {
            if (Player != null)
            {
                Player.Update(time, delta);
            }
        }
    }
}
