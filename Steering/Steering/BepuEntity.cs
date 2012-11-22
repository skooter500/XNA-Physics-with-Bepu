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
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Collidables.Events;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Collidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;

namespace Steering
{
    public class BepuEntity:GameEntity
    {
        SoundEffect soundEffect;
        SoundEffectInstance soundEffectInstance;
        public BEPUphysics.Entities.Entity body;
        AudioEmitter emitter = new AudioEmitter();
        AudioListener listener = new AudioListener();

        public override void LoadContent()
        {

            soundEffect = XNAGame.Instance.Content.Load<SoundEffect>("Hit" + (XNAGame.Instance.Random.Next(6) + 1));
            soundEffectInstance = soundEffect.CreateInstance();
            base.LoadContent();
        }
        
        public override void Update(GameTime gameTime)
        {
            worldTransform = body.WorldTransform;
        }

        void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            if (soundEffect != null)
            {
                emitter.Position = body.Position / 5.0f;
                listener.Position = XNAGame.Instance.Camera.Position / 5.0f;
                soundEffectInstance.Apply3D(listener, emitter);
                soundEffectInstance.Play();
                
            }
        }

        public void configureEvents()
        {
            body.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
        }
    }
}
