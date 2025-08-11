using System;

namespace PixelManGame
{
    public class Player : Image
    {
        public static readonly string DEFAULT_TEXTURE = "pixel-man";
        private float speed = 6.0f;
        private float tileSize = 64.0f;
        private bool moving = false;
        private float targetX = 0.0f;
        private float targetY = 0.0f;
        private MoveDirection moveDir = new MoveDirection { dx = 0, dy = 0 };
        private int keyPressDelayFrames = 4;
        private int keyPressFrameCount = 0;

        public CursorKeys Cursors { get; set; }
        private string debugInfo = "";

        private float startX = 0.0f;
        private float startY = 0.0f;
        private float moveProgress = 0.0f;

        public Player(Scene scene, float x, float y, string texture = null) 
            : base(scene, x, y, texture ?? DEFAULT_TEXTURE)
        {
            SnapToGrid();
            scene.Add.Existing(this);
            SetScale(1.0f);
            
            if (scene.Input.Keyboard != null)
            {
                Cursors = scene.Input.Keyboard.CreateCursorKeys();
            }

            Console.WriteLine("Player created with debug info enabled");
        }

        public void SetSpeed(float value)
        {
            speed = value;
        }

        public float GetSpeed()
        {
            return speed;
        }

        private void UpdateDebugInfo()
        {
            debugInfo = $"x: {X:F2}\n" +
                       $"y: {Y:F2}\n" +
                       $"moving: {moving}\n" +
                       $"targetX: {targetX}\n" +
                       $"targetY: {targetY}\n" +
                       $"moveDir: dx={moveDir.dx}, dy={moveDir.dy}\n" +
                       $"left: {Cursors?.Left?.IsDown ?? false}\n" +
                       $"right: {Cursors?.Right?.IsDown ?? false}\n" +
                       $"up: {Cursors?.Up?.IsDown ?? false}\n" +
                       $"down: {Cursors?.Down?.IsDown ?? false}";
            
            Console.WriteLine($"Player Debug:\n{debugInfo}");
        }

        public void SnapToGrid()
        {
            X = (float)(Math.Round(X / tileSize) * tileSize);
            Y = (float)(Math.Round(Y / tileSize) * tileSize);
        }

        public MoveDirection GetMoveDirection()
        {
            int dx = 0, dy = 0;
            
            if (Cursors?.Left?.IsDown == true) dx += -1;
            if (Cursors?.Right?.IsDown == true) dx += 1;
            if (Cursors?.Up?.IsDown == true) dy += -1;
            if (Cursors?.Down?.IsDown == true) dy += 1;

            if (dx != 0 || dy != 0)
            {
                keyPressFrameCount += 1;
            }
            else
            {
                keyPressFrameCount = 0;
            }

            if (keyPressFrameCount >= keyPressDelayFrames)
            {
                return new MoveDirection { dx = dx, dy = dy };
            }

            return new MoveDirection { dx = 0, dy = 0 };
        }

        public bool CanChangeDirection()
        {
            return !moving || IsNearSnapPosition();
        }

        public bool IsNearSnapPosition()
        {
            float snapX = (float)(Math.Round(X / tileSize) * tileSize);
            float snapY = (float)(Math.Round(Y / tileSize) * tileSize);
            float dist = (float)Math.Sqrt((X - snapX) * (X - snapX) + (Y - snapY) * (Y - snapY));
            return dist < speed;
        }

        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public override void Update(float time = 0, float delta = 16)
        {
            if (Cursors == null) return;

            // Only process a move if not already moving
            if (!moving)
            {
                var moveDirection = GetMoveDirection();

                // Only move if a direction is pressed
                if (moveDirection.dx != 0 || moveDirection.dy != 0)
                {
                    moveDir = moveDirection;
                    startX = (float)(Math.Round(X / tileSize) * tileSize);
                    startY = (float)(Math.Round(Y / tileSize) * tileSize);
                    targetX = startX + moveDirection.dx * tileSize;
                    targetY = startY + moveDirection.dy * tileSize;
                    moveProgress = 0;
                    moving = true;
                }
            }

            // Move toward target if moving
            if (moving)
            {
                // Calculate progress based on speed and delta
                float moveSteps = tileSize;
                if (moveDir.dx != 0 && moveDir.dy != 0)
                {
                    // add extra steps for diagonals
                    moveSteps += moveSteps * 0.7071f; // Math.SQRT1_2 equivalent
                }
                moveProgress += (speed / moveSteps) * (delta / 16);
                
                if (moveProgress >= 1)
                {
                    X = targetX;
                    Y = targetY;
                    moving = false;
                }
                else
                {
                    X = Lerp(startX, targetX, moveProgress);
                    Y = Lerp(startY, targetY, moveProgress);
                }
            }

            // At end of update, show debug info
            UpdateDebugInfo();
        }
    }

    public struct MoveDirection
    {
        public int dx;
        public int dy;
    }
}
