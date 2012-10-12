using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Steering
{
    public class Ground : GameEntity
    {
        const float vertShiftSpeed = 0.17f / 1000;
        const float horzShiftSpeed = 0.13f / 1000;

        GraphicsDeviceManager graphics;
        VertexPositionTexture[] vertices;
        BasicEffect basicEffect;

        public float width = 500;
        public float height = 500;
        Plane worldPlane;

        public override void UnloadContent()
        {

        }

        public bool rayIntersects(Ray ray, out Vector3 point)
        {
	        // Calculate t
	        float t;
	        	        
	        t = - worldPlane.D - Vector3.Dot(worldPlane.Normal, ray.Position) / (Vector3.Dot(worldPlane.Normal, ray.Direction));
	        if (t > 0)
	        {
		        point = ray.Position + (ray.Direction * t);
		        return true;
	        }
            point = Vector3.Zero;
	        return false;
        }

        public Ground()
        {
            graphics = XNAGame.Instance.GraphicsDeviceManager;
            worldPlane = new Plane(0, -1, 0, 0);
            //Content.RootDirectory = "Content";
            //TargetElapsedTime = TimeSpan.FromSeconds(1 / 30.0);
        }

        public override void LoadContent()
        {
            

            float twidth = 10;
            float theight = 10;

            vertices = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(-width, 0, height), new Vector2(0, theight)),
                new VertexPositionTexture(new Vector3(-width, 0, -height), new Vector2(0,0)),
                new VertexPositionTexture(new Vector3(width, 0, height), new Vector2(twidth, theight)),

                new VertexPositionTexture(new Vector3(width, 0, height), new Vector2(twidth, theight)),
                new VertexPositionTexture(new Vector3(-width, 0, -height), new Vector2(0, 0)),
                
                new VertexPositionTexture(new Vector3(width, 0, -height), new Vector2(twidth, 0))
            };

            Texture2D portrait = XNAGame.Instance.Content.Load<Texture2D>("Ground");
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = portrait;
            
            
        }

        public override void Update(GameTime gameTime)
        {

            basicEffect.World = Matrix.Identity;
            basicEffect.Projection = XNAGame.Instance.Camera.projection;
            basicEffect.View = XNAGame.Instance.Camera.view;
                /*View = Matrix.CreateLookAt
                                (new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up),
                Projection = Matrix.CreatePerspectiveFieldOfView
                                (MathHelper.PiOver4, aspectRatio, 0.1f, 10)
                 */
           
        }

        public override void Draw(GameTime gameTime)
        {

            basicEffect.FogColor = Color.Black.ToVector3();
            basicEffect.FogEnabled = true;
            basicEffect.FogStart = 100.0f;
            basicEffect.FogEnd = 150.0f;

            EffectPass effectPass = basicEffect.CurrentTechnique.Passes[0];
            effectPass.Apply();
            SamplerState state = new SamplerState();
            state.AddressU = TextureAddressMode.Wrap;
            state.AddressV = TextureAddressMode.Wrap;
            state.AddressW = TextureAddressMode.Wrap;
            state.Filter = TextureFilter.Anisotropic;
            graphics.GraphicsDevice.SamplerStates[0] = state;
            graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, 2);
        }
    }
}




/*using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace Steering
{
    class Ground : Entity
    {
        Texture2D groundTexture;
        VertexPositionTexture[] pointList;

        BasicEffect basicEffect;
        int points = 6;

        public Ground()
        {
            worldTransform = Matrix.Identity;
            pos = new Vector3(0, 0, 0);
            look = new Vector3(0, 1, 0);
            right = new Vector3(1, 0, 0);
            up = new Vector3(0, 0, 1);
            globalUp = new Vector3(0, 0, 1);

            pointList = new VertexPositionTexture[points];
            basicEffect = new BasicEffect(XNAGame.Instance().GraphicsDevice);
            basicEffect.VertexColorEnabled = true;

            pointList = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1))
            };

            /*pointList[0] = new VertexPositionTexture(new Vector3(-10, -10, 0), new Vector2(0, 1));
            pointList[1] = new VertexPositionTexture(new Vector3(-10, 10, 0), new Vector2(0, 0));
            pointList[2] = new VertexPositionTexture(new Vector3(10, -10, 0), new Vector2(1, 1));
            pointList[3] = new VertexPositionTexture(new Vector3(10, -10, 0), new Vector2(1, 1));
            pointList[4] = new VertexPositionTexture(new Vector3(-10, 10, 0), new Vector2(0, 0));
            pointList[5] = new VertexPositionTexture(new Vector3(10, 10, 0), new Vector2(1, 0));


        }

        public override void LoadContent()
        {
            groundTexture = XNAGame.Instance().Content.Load<Texture2D>("ground");
        }

        public override void UnloadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            basicEffect.World = Matrix.Identity;
            basicEffect.View = XNAGame.Instance().Camera.getView();
            basicEffect.Projection = XNAGame.Instance().Camera.getProjection();
            basicEffect.Texture = groundTexture;
            basicEffect.TextureEnabled = true;
            XNAGame.Instance().GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            EffectPass effectPass = basicEffect.CurrentTechnique.Passes[0];
            effectPass.Apply();
            XNAGame.Instance().GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, pointList, 0, pointList.Length - 2);
            
            
        }
    }
}*/
