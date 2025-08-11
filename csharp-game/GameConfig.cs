using System;
using System.Collections.Generic;

namespace PixelManGame
{
    public class GameConfig
    {
        public string Type { get; set; } = "AUTO";
        public int Width { get; set; } = 320;
        public int Height { get; set; } = 640;
        public ScaleConfig Scale { get; set; } = new ScaleConfig();
        public string Parent { get; set; } = "game-container";
        public string BackgroundColor { get; set; } = "#3d4042";
        public List<string> Scenes { get; set; } = new List<string>();
    }

    public class ScaleConfig
    {
        public string Mode { get; set; } = "EXPAND";
        public string AutoCenter { get; set; } = "CENTER_HORIZONTALLY";
    }

    public static class GameMain
    {
        public static GameConfig CreateConfig()
        {
            return new GameConfig
            {
                Type = "AUTO",
                Width = 320,
                Height = 640,
                Scale = new ScaleConfig
                {
                    Mode = "EXPAND",
                    AutoCenter = "CENTER_HORIZONTALLY"
                },
                Parent = "game-container",
                BackgroundColor = "#3d4042",
                Scenes = new List<string> { "Boot", "Preloader", "MainScene" }
            };
        }

        public static void StartGame(string parent)
        {
            var config = CreateConfig();
            config.Parent = parent;
            // This would initialize the game with the given config
            Console.WriteLine($"Starting game with parent: {parent}");
        }
    }
}
