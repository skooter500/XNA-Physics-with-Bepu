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
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Constraints.SolverGroups;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.DataStructures;

namespace Steering
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class XNAGame : Microsoft.Xna.Framework.Game
    {
        static XNAGame instance = null;
        BEPUphysics.Entities.Entity pickedUp = null;
        Box groundBox;
        GameEntity cylinder;
        Texture2D crosshairs;

        public static XNAGame Instance
        {
            get { return XNAGame.instance; }
            set { XNAGame.instance = value; }
        }
        GraphicsDeviceManager graphics;

        private Random random = new Random();

        public Random Random
        {
            get { return random; }
            set { random = value; }
        }
        Space space;
        Cylinder cameraCylindar;

        float lastFired = 1.0f;

        public GraphicsDeviceManager Graphics
        {
            get { return graphics; }
            set { graphics = value; }
        }
        SpriteBatch spriteBatch;

        public SpriteBatch SpriteBatch1
        {
            get { return spriteBatch; }
            set { spriteBatch = value; }
        }
        private Ground ground = null;

        public Ground Ground
        {
            get { return ground; }
            set { ground = value; }
        }

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
            set { spriteBatch = value; }
        }
        private Camera camera;
        List<GameEntity> children = new List<GameEntity>();

        public List<GameEntity> Children
        {
            get { return children; }
            set { children = value; }
        }
        
        public XNAGame()
        {
            instance = this;
            graphics = new GraphicsDeviceManager(this);

#if WINDOWS
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferMultiSampling = true;
            graphics.IsFullScreen = false;
            graphics.SynchronizeWithVerticalRetrace = true;
            
#elif WINDOWS_PHONE
            Graphics.PreferredBackBufferWidth = 240;
            Graphics.PreferredBackBufferHeight = 400;
#endif
            graphics.ApplyChanges();

            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            camera = new Camera();
            camera.Position = new Vector3(0, 30, 100);
            int midX = GraphicsDeviceManager.DefaultBackBufferHeight / 2;
            int midY = GraphicsDeviceManager.DefaultBackBufferWidth / 2;
            Mouse.SetPosition(midX, midY);

                                   
            children.Add(camera);
            ground = new Ground();                        
            children.Add(ground);

            base.Initialize();
        }

        BepuEntity createWheel(Vector3 position, float wheelWidth, float wheelRadius)
        {
            BepuEntity wheelEntity = new BepuEntity();
            wheelEntity.modelName = "cyl";
            wheelEntity.LoadContent();
            wheelEntity.body = new Cylinder(position, wheelWidth, wheelRadius, wheelRadius);
            wheelEntity.localTransform = Matrix.CreateScale(wheelRadius, wheelWidth, wheelRadius);
            wheelEntity.body.Orientation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), MathHelper.PiOver2);
            wheelEntity.diffuse = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            space.Add(wheelEntity.body);
            children.Add(wheelEntity);
            return wheelEntity;
   
        }

        BepuEntity createCog(Vector3 position, float radius, int gears)
        {
            BepuEntity wheel = createWheel(position, 1, radius);

            float angleDelta = (MathHelper.Pi * 2.0f) / (float) gears;

            float cogHeight = radius * 0.2f;
            for (int i = 0; i < gears; i++)
            {
                float angle = ((float) i) * angleDelta;
                float x = (radius + (cogHeight / 2.0f)) * (float) Math.Sin(angle);
                float y = (radius + (cogHeight / 2.0f)) * (float) Math.Cos(angle);
                Vector3 cogPos = new Vector3(x, y, 0) + position;
                BepuEntity cog = createBox(cogPos, cogHeight, cogHeight, 1);
                cog.LoadContent();
                cog.body.Orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), angle);
                WeldJoint weld = new WeldJoint(wheel.body, cog.body);
                space.Add(weld);
            }

            return wheel;
        }

        void createSteerableVehicle(Vector3 position)
        {
            float width = 15;
            float height = 2;
            float length = 5;
            float wheelWidth = 1;
            float wheelRadius = 2;

            BepuEntity chassis = createBox(position, width, height, length);
            chassis.LoadContent();
            chassis.body.Mass = 100;

            BepuEntity wheel;
            RevoluteJoint joint;
            SwivelHingeJoint steerJoint;
            Vector3 wheelPos = new Vector3(position.X - (width / 2) + wheelRadius, position.Y, position.Z - (length / 2));
            wheel = createWheel(wheelPos, wheelWidth, wheelRadius);
            wheelPos = new Vector3(position.X - (width / 2) + wheelRadius, position.Y, position.Z - (length / 2) - (wheelWidth * 2));

            joint = new RevoluteJoint(chassis.body, wheel.body, wheel.body.Position, new Vector3(0, 0, -1));
            space.Add(joint);

            wheel = createWheel(new Vector3(position.X - (width / 2) + wheelRadius, position.Y, position.Z + (length / 2) + wheelWidth), wheelWidth, wheelRadius);
            joint = new RevoluteJoint(chassis.body, wheel.body, wheel.body.Position, new Vector3(0, 0, -1));
            space.Add(joint);


            wheel = createWheel(new Vector3(position.X + (width / 2) - wheelRadius, position.Y, position.Z - (length / 2) - wheelWidth), wheelWidth, wheelRadius);
            joint = new RevoluteJoint(chassis.body, wheel.body, wheel.body.Position, new Vector3(0, 0, -1));
            space.Add(joint);
            
            wheel = createWheel(new Vector3(position.X + (width / 2) - wheelRadius, position.Y, position.Z + (length / 2) + wheelWidth), wheelWidth, wheelRadius);
            joint = new RevoluteJoint(chassis.body, wheel.body, wheel.body.Position, new Vector3(0, 0, -1));
            space.Add(joint);
             
        }

        void createVehicle(Vector3 position)
        {
            float width = 15;
            float height = 2;
            float length = 5;
            float wheelWidth = 1;
            float wheelRadius = 2;

            BepuEntity chassis = createBox(position, width, height, length);
            chassis.LoadContent();
            chassis.body.Mass = 100;

            BepuEntity wheel;
            RevoluteJoint joint;

            wheel = createWheel(new Vector3(position.X - (width / 2) + wheelRadius, position.Y, position.Z - (length / 2) - wheelWidth), wheelWidth, wheelRadius);
            joint = new RevoluteJoint(chassis.body, wheel.body, wheel.body.Position, new Vector3(0, 0, -1));
            space.Add(joint);
            
            wheel = createWheel(new Vector3(position.X + (width / 2) - wheelRadius, position.Y, position.Z - (length / 2) - wheelWidth), wheelWidth, wheelRadius);
            joint = new RevoluteJoint(chassis.body, wheel.body, wheel.body.Position, new Vector3(0, 0, -1));
            space.Add(joint);

            wheel = createWheel(new Vector3(position.X - (width / 2) + wheelRadius, position.Y, position.Z + (length / 2) + wheelWidth), wheelWidth, wheelRadius);
            joint = new RevoluteJoint(chassis.body, wheel.body, wheel.body.Position, new Vector3(0, 0, -1));
            space.Add(joint);

            wheel = createWheel(new Vector3(position.X + (width / 2) - wheelRadius, position.Y, position.Z + (length / 2) + wheelWidth), wheelWidth, wheelRadius);
            joint = new RevoluteJoint(chassis.body, wheel.body, wheel.body.Position, new Vector3(0, 0, -1));
            space.Add(joint);
        }


        void createTower()
        {
            for (float y = 200; y > 5; y -= 5)
            {
                createBox(new Vector3(0, y, 0), 20, 4.99f, 20);
            }
        }

        void createWall()
        {
            for (float z = -20; z < 20; z += 5)
            {
                for (float y = 60; y > 0; y -= 5)
                {
                    createBox(new Vector3(-20, y, z), 4, 4, 4);
                }
            }
        }


        void jointDemo()
        {
            BepuEntity e1;
            BepuEntity e2;
            Joint joint;

            // Ball & socket joint
            e1 = createBox(new Vector3(20, 5, -20), 1, 1, 5);
            e1.body.BecomeKinematic();
            e2 = createBox(new Vector3(20, 5, -10), 1, 1, 5);
            joint = new BallSocketJoint(e1.body, e2.body, new Vector3(20, 5, -15));
            space.Add(joint);
            
            // Hinge
            e1 = createBox(new Vector3(30, 5, -20), 1, 1, 5);
            e1.body.BecomeKinematic();
            e2 = createBox(new Vector3(30, 5, -10), 1, 1, 5);

            RevoluteJoint hinge = new RevoluteJoint(e1.body, e2.body, new Vector3(20, 5, -15), new Vector3(1, 0, 0));
            space.Add(hinge);

            // Universal
            e1 = createBox(new Vector3(40, 5, -20), 1, 1, 5);
            
            e2 = createBox(new Vector3(40, 5, -10), 1, 1, 5);

            UniversalJoint uni = new UniversalJoint(e1.body, e2.body, new Vector3(40, 5, -15));
            space.Add(uni);

            // Weld Joint
            e1 = createBox(new Vector3(50, 5, -20), 1, 1, 5);
            e2 = createBox(new Vector3(50, 5, -10), 1, 1, 5);

            WeldJoint weld = new WeldJoint(e1.body, e2.body);
            space.Add(weld);

            // PointOnLine Joint
            // create the line
            e1 = createBox(new Vector3(60, 5, -20), 1, 1, 5);
            e1.body.BecomeKinematic();
            e2 = createBox(new Vector3(60, 10, -10), 1, 1, 1);
            PointOnLineJoint pol = new PointOnLineJoint(e1.body, e2.body, new Vector3(60, 5, -20), new Vector3(0, 0, -1), new Vector3(60, 5, -10));
            space.Add(pol);
        }

        BepuEntity fireBall()
        {
            BepuEntity ball = new BepuEntity();
            ball.modelName = "sphere";
            float size = 1;
            ball.localTransform = Matrix.CreateScale(new Vector3(size, size, size));
            ball.body = new Sphere(Camera.Position + (Camera.Look * 8), size, size * 10);
            ball.diffuse = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            space.Add(ball.body);
            ball.LoadContent();
            ball.configureEvents();
            ball.body.ApplyImpulse(Camera.Position, Camera.Look * 500);
            children.Add(ball);
            return ball;

        }

        void resetScene()
        {
            for (int i = 0; i < children.Count(); i++)
            {
                if (children[i] is BepuEntity)
                {
                    children.Remove(children[i]);
                    i--;
                }
            }
            space = null;
            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -9.8f, 0);

            groundBox = new Box(Vector3.Zero, ground.width, 0.1f, ground.height);
            space.Add(groundBox);

            cameraCylindar = new Cylinder(Camera.Position, 5, 2);
            space.Add(cameraCylindar);

            createTower();
            createWall();
            jointDemo();
        }

        BepuEntity createFromMesh(Vector3 position, string mesh, float scale)
        {
            BepuEntity entity = new BepuEntity();
            entity.modelName = mesh;
            entity.LoadContent();
            Vector3[] vertices;
            int[] indices;
            TriangleMesh.GetVerticesAndIndicesFromModel(entity.model, out vertices, out indices);
            AffineTransform localTransform = new AffineTransform(new Vector3(scale, scale, scale), Quaternion.Identity, new Vector3(0, 0, 0));
            MobileMesh mobileMesh = new MobileMesh(vertices, indices, localTransform, BEPUphysics.CollisionShapes.MobileMeshSolidity.Counterclockwise, 1);
            entity.localTransform = Matrix.CreateScale(scale, scale, scale);
            entity.body = mobileMesh;
            entity.HasColor = true;
            entity.diffuse = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            entity.body.Position = position;
            space.Add(entity.body);
            children.Add(entity);
            return entity;
        }

        BepuEntity createBox(Vector3 position, float width, float height, float length)
        {
            BepuEntity theBox = new BepuEntity();
            theBox.modelName = "cube";
            theBox.LoadContent();
            theBox.localTransform = Matrix.CreateScale(new Vector3(width, height, length));
            theBox.body = new Box(position, width, height, length, 1);
            theBox.diffuse = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            theBox.configureEvents();
            space.Add(theBox.body);
            children.Add(theBox);
            return theBox;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>       
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            resetScene();
            
            

            crosshairs = Content.Load<Texture2D>("sprites_crosshairs");

            foreach (GameEntity child in children)
            {
                child.LoadContent();
            }

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            foreach (GameEntity child in children)
            {
                child.UnloadContent();
            }

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        bool didSpawn = false;
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            if (keyState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            if (keyState.IsKeyDown(Keys.F1))
            {
                if (!didSpawn)
                {
                    Ray ray;
                    ray.Position = Camera.Position;
                    ray.Direction = Camera.Look;
                    Vector3 point;
                    if (ground.rayIntersects(ray, out point))
                    {
                        point.Y = 10;
                        createFromMesh(point, "fish", 10);
                        didSpawn = true;
                    }
                }
            }
            else if (keyState.IsKeyDown(Keys.F2))
            {
                if (!didSpawn)
                {
                    Ray ray;
                    ray.Position = Camera.Position;
                    ray.Direction = Camera.Look;
                    Vector3 point;
                    if (ground.rayIntersects(ray, out point))
                    {
                        point.Y = 10;
                        createFromMesh(point, "tube", 1);
                        didSpawn = true;
                    }
                }
            }
            else if (keyState.IsKeyDown(Keys.F3))
            {
                if (!didSpawn)
                {
                    Ray ray;
                    ray.Position = Camera.Position;
                    ray.Direction = Camera.Look;
                    Vector3 point;
                    if (ground.rayIntersects(ray, out point))
                    {
                        point.Y = 10;
                        createVehicle(point);
                        didSpawn = true;
                    }
                }

            }
            else if (keyState.IsKeyDown(Keys.F4))
            {
                if (!didSpawn)
                {
                    Ray ray;
                    ray.Position = Camera.Position;
                    ray.Direction = Camera.Look;
                    Vector3 point;
                    if (ground.rayIntersects(ray, out point))
                    {
                        point.Y = 10;
                        createFromMesh(point, "guy", 10);
                        didSpawn = true;
                    }
                }
            }
            else if (keyState.IsKeyDown(Keys.F5))
            {
                if (!didSpawn)
                {
                    Ray ray;
                    ray.Position = Camera.Position;
                    ray.Direction = Camera.Look;
                    Vector3 point;
                    if (ground.rayIntersects(ray, out point))
                    {
                        point.Y = 25;
                        BepuEntity cog1 = createCog(point, 10, 20);
                        //BepuEntity cog2 = createCog(point, 10, 20);

                        didSpawn = true;
                    }
                }
            }
            else if (keyState.IsKeyDown(Keys.F12))
            {
                resetScene();
                didSpawn = true;
            }
            else
            {
                didSpawn = false;
            }




            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (pickedUp == null)
                {
                    Ray ray;
                    ray.Position = Camera.Position;
                    ray.Direction = Camera.Look;
                    RayCastResult result;
                    bool didDit = space.RayCast(ray, out result);
                    if (didDit)
                    {
                        pickedUp = ((EntityCollidable)result.HitObject).Entity;
                        if (pickedUp == groundBox)
                        {
                            pickedUp = null;
                        }
                    }
                }
                if (pickedUp != null)
                {
                    float fDistance = 15.0f;
                    float powerfactor = 50.0f; // Higher values causes the targets moving faster to the holding point.
                    float maxVel = 40.0f;      // Lower values prevent objects flying through walls.

                    // Calculate the hold point in front of the camera
                    Vector3 holdPos = Camera.Position + (Camera.Look * fDistance);

                    Vector3 v = holdPos - pickedUp.Position; // direction to move the Target
                    v *= powerfactor; // powerfactor of the GravityGun

                    if (v.Length() > maxVel)
                    {
                        // if the correction-velocity is bigger than maximum
                        v.Normalize();
                        v *= maxVel; // just set correction-velocity to the maximum
                    }
                    pickedUp.LinearVelocity = v;
                     
                }
            }
            else
            {
                pickedUp = null;
            }
            if (keyState.IsKeyDown(Keys.F) & pickedUp != null)
            {
                pickedUp.LinearVelocity = Vector3.Zero;
                pickedUp.ApplyImpulse(pickedUp.Position, Camera.Look * 100.0f);
                pickedUp = null; 
            }
                            
            if (keyState.IsKeyDown(Keys.B) & lastFired > 0.25f)
            {
                fireBall();
                lastFired = 0.0f;
            }
            
            lastFired += timeDelta;
            

            for (int i = 0; i < children.Count; i++)
            {
                children[i].Update(gameTime);
            }

            cameraCylindar.Position = camera.Position;
            space.Update();
            
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            foreach (GameEntity child in children)
            {
                DepthStencilState state = new DepthStencilState();
                state.DepthBufferEnable = true;                
                GraphicsDevice.DepthStencilState = state;
                child.Draw(gameTime);
            }
            // Draw any lines
            Line.Draw();

            // Draw the crosshairs
            Vector2 center = Vector2.Zero, origin = Vector2.Zero;
            center.X = graphics.PreferredBackBufferWidth / 2;
            center.Y = graphics.PreferredBackBufferHeight / 2;            
            Rectangle spriteRect = new Rectangle(76, 28, 15, 15);
            origin.X = spriteRect.Width / 2;
            origin.Y = spriteRect.Height / 2;
            spriteBatch.Draw(crosshairs, center, spriteRect, Color.Orange, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);

            spriteBatch.End();            
        }

        public Camera Camera
        {
            get
            {
                return camera;
            }
            set
            {
                camera = value;
            }
        }

        public GraphicsDeviceManager GraphicsDeviceManager
        {
            get
            {
                return graphics;
            }
        }
    }
}
