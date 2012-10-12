using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace My_First_XNA_Example
{
    class Ship: Thing
    {
        Rectangle rect;
        public Ship():base()
        {
            Pos = new Vector2(100, 100);

            Width = 90;
            Height = 144;
        }

        public override void LoadContent()
        {
            Texture = Game1.Instance.Content.Load<Texture2D>("vehicles");
            Vector2 tl, br;
            tl = new Vector2(550, 446);
            br = new Vector2(705, 544);
            Width = br.X - tl.X;
            Height = br.Y - tl.Y;
            rect = new Rectangle((int)tl.X, (int)tl.Y, (int) Width, (int) Height);

        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            float speed = 50.0f;
            if (keyState.IsKeyDown(Keys.A))
            {
                pos.X -= (float)gameTime.ElapsedGameTime.TotalSeconds * speed;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                pos.X += (float)gameTime.ElapsedGameTime.TotalSeconds * speed;
            }
            if (keyState.IsKeyDown(Keys.W))
            {
                pos.Y -= (float)gameTime.ElapsedGameTime.TotalSeconds * speed;
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                pos.Y += (float)gameTime.ElapsedGameTime.TotalSeconds * speed;
            }
            if (keyState.IsKeyDown(Keys.Left))
            {
                rotate (- (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (keyState.IsKeyDown(Keys.Right))
            {
                rotate ((float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            if (keyState.IsKeyDown(Keys.Up))
            {
                Walk((float)gameTime.ElapsedGameTime.TotalSeconds * speed);
            }

        }

        public override void Draw(GameTime gameTime)
        {
            //Game1.Instance.SpriteBatch.Draw(Texture, Pos, rect, Color.White);
            Vector2 centre = new Vector2(Width / 2, Height / 2);
            Game1.Instance.SpriteBatch.Draw(Texture, Pos, rect, Color.White, Rot - (MathHelper.Pi * 1.5f), centre, 1, SpriteEffects.None, 0); 
            
        }

        public override void UnloadContent()
        {
            Texture.Dispose();
        }
    }
}
