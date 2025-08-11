using System;
using System.Collections.Generic;

namespace PixelManGame
{
    public abstract class Scene
    {
        public string Key { get; protected set; }
        public SceneManager SceneManager { get; set; }
        public LoaderPlugin Load { get; set; }
        public GameObjectFactory Add { get; set; }
        public InputPlugin Input { get; set; }

        protected Scene(string key)
        {
            Key = key;
            Load = new LoaderPlugin();
            Add = new GameObjectFactory();
            Input = new InputPlugin();
        }

        public virtual void Init() { }
        public virtual void Preload() { }
        public virtual void Create() { }
        public virtual void Update(float time = 0, float delta = 16) { }
    }

    public class SceneManager
    {
        public void Start(string sceneKey)
        {
            Console.WriteLine($"Starting scene: {sceneKey}");
        }
    }

    public class LoaderPlugin
    {
        public Dictionary<string, Action<float>> Events { get; set; } = new Dictionary<string, Action<float>>();

        public void SetPath(string path)
        {
            Console.WriteLine($"Setting asset path: {path}");
        }

        public void Image(string key, string url)
        {
            Console.WriteLine($"Loading image: {key} from {url}");
        }

        public void On(string eventName, Action<float> callback)
        {
            Events[eventName] = callback;
        }

        public void EmitProgress(float progress)
        {
            if (Events.ContainsKey("progress"))
            {
                Events["progress"](progress);
            }
        }
    }

    public class GameObjectFactory
    {
        public GameObject Image(float x, float y, string texture)
        {
            return new GameObject(x, y, texture);
        }

        public GameObject Rectangle(float x, float y, float width, float height)
        {
            return new GameObject(x, y, "rectangle") { Width = width, Height = height };
        }

        public GameObject Text(float x, float y, string text)
        {
            return new GameObject(x, y, "text") { Text = text };
        }

        public void Existing(GameObject gameObject)
        {
            Console.WriteLine($"Adding existing game object at ({gameObject.X}, {gameObject.Y})");
        }
    }

    public class InputPlugin
    {
        public KeyboardPlugin Keyboard { get; set; } = new KeyboardPlugin();
    }

    public class KeyboardPlugin
    {
        public CursorKeys CreateCursorKeys()
        {
            return new CursorKeys();
        }
    }

    public class CursorKeys
    {
        public Key Left { get; set; } = new Key();
        public Key Right { get; set; } = new Key();
        public Key Up { get; set; } = new Key();
        public Key Down { get; set; } = new Key();
    }

    public class Key
    {
        public bool IsDown { get; set; } = false;
    }
}
