using System;

namespace PixelManGame
{
    public class GameObject
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string Texture { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string Text { get; set; }
        public float ScaleX { get; set; } = 1.0f;
        public float ScaleY { get; set; } = 1.0f;
        public Scene Scene { get; set; }

        public GameObject(float x, float y, string texture = "")
        {
            X = x;
            Y = y;
            Texture = texture;
        }

        public GameObject SetScale(float scale)
        {
            ScaleX = scale;
            ScaleY = scale;
            return this;
        }

        public GameObject SetScale(float scaleX, float scaleY)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
            return this;
        }

        public GameObject SetStrokeStyle(float lineWidth, uint color)
        {
            Console.WriteLine($"Setting stroke style: width={lineWidth}, color=0x{color:X}");
            return this;
        }

        public virtual void Update(float time = 0, float delta = 16)
        {
            // Base update logic
        }
    }

    public class Image : GameObject
    {
        public static readonly string DEFAULT_TEXTURE = "pixel-man";

        public Image(Scene scene, float x, float y, string texture = null) : base(x, y, texture ?? DEFAULT_TEXTURE)
        {
            Scene = scene;
        }
    }
}
