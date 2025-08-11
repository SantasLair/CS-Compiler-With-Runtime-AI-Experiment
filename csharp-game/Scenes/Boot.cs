using System;

namespace PixelManGame.Scenes
{
    public class Boot : Scene
    {
        public Boot() : base("Boot")
        {
        }

        public override void Preload()
        {
            // The Boot Scene is typically used to load in any assets you require for your Preloader, such as a game logo or background.
            // The smaller the file size of the assets, the better, as the Boot Scene itself has no preloader.
            Load.Image("background", "assets/bg.png");
        }

        public override void Create()
        {
            SceneManager.Start("Preloader");
        }
    }
}
