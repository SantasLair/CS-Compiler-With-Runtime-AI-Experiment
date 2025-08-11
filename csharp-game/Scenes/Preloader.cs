using System;

namespace PixelManGame.Scenes
{
    public class Preloader : Scene
    {
        public Preloader() : base("Preloader")
        {
        }

        public override void Init()
        {
            // We loaded this image in our Boot Scene, so we can display it here
            Add.Image(512, 384, "background");

            // A simple progress bar. This is the outline of the bar.
            Add.Rectangle(512, 384, 468, 32).SetStrokeStyle(1, 0xffffff);

            // This is the progress bar itself. It will increase in size from the left based on the % of progress.
            var bar = Add.Rectangle(512 - 230, 384, 4, 28);

            // Use the 'progress' event emitted by the LoaderPlugin to update the loading bar
            Load.On("progress", (progress) =>
            {
                // Update the progress bar (our bar is 464px wide, so 100% = 464px)
                bar.Width = 4 + (460 * progress);
            });
        }

        public override void Preload()
        {
            // Load the assets for the game - Replace with your own assets
            Load.SetPath("assets");
            Load.Image("logo", "logo.png");
            Load.Image("pixel-man", "pixel-man.png");
        }

        public override void Create()
        {
            // When all the assets have loaded, it's often worth creating global objects here that the rest of the game can use.
            // For example, you can define global animations here, so we can use them in other scenes.

            // Move to the MainScene. You could also swap this for a Scene Transition, such as a camera fade.
            SceneManager.Start("MainScene");
        }
    }
}
