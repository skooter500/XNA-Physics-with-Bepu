using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Steering
{
    class Lazer:GameEntity
    {

        public override void LoadContent()
        {
        }
        public override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime)
        {
            float width = XNAGame.Instance.Ground.width;
            float height = XNAGame.Instance.Ground.height;
            float speed = 50.0f;
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if ((Position.X < - (width / 2)) || (Position.X > width / 2) || (Position.Z < - (height / 2)) || (Position.Z > height / 2) || (Position.Y < 0) || (Position.Y > 100))
            {
                Alive = false;
            }
            Position += Look * speed * timeDelta;
        }

        public override void Draw(GameTime gameTime)
        {
            Line.DrawLine(Position, Position + Look * 5, Color.Green);
        }

    }
}
