using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using System.IO;
using BEPUphysics.Entities.Prefabs;

namespace Steering
{
    public class Person:GameEntity
    {
        KinectSensor sensor;

        Skeleton[] skeletonData = null;

        public KinectStatus LastStatus { get; private set; }

        float footHeight;

        Dictionary<string, BepuEntity> bones = new Dictionary<string, BepuEntity>();
        public KinectSensor Sensor
        {
            get { return sensor; }
            set { sensor = value; }
        }

        private void DiscoverSensor()
        {
            // Grab any available sensor
            this.Sensor = KinectSensor.KinectSensors.FirstOrDefault();

            if (this.Sensor != null)
            {
                LastStatus = this.Sensor.Status;

                // If this sensor is connected, then enable it
                if (this.LastStatus == KinectStatus.Connected)
                {
                    sensor.SkeletonStream.Enable(new TransformSmoothParameters()
                    {
                        Smoothing = 0.5f,
                        Correction = 0.5f,
                        Prediction = 0.5f,
                        JitterRadius = 0.05f,
                        MaxDeviationRadius = 0.04f
                    });

                    try
                    {
                        this.Sensor.Start();
                    }
                    catch (IOException)
                    {
                        // sensor is in use by another application
                        // will treat as disconnected for display purposes
                        this.Sensor = null;
                    }
                }
            }
            else
            {
                this.LastStatus = KinectStatus.Disconnected;
            }
        }

        public Person()
        {
            KinectSensor.KinectSensors.StatusChanged += this.KinectSensors_StatusChanged;
            this.DiscoverSensor();
        }

        public override void LoadContent()
        {
        }

        private void DrawFace(JointCollection joints, JointType joint, float size)
        {
            SkeletonPoint sP;
            sP = joints[joint].Position;

            float scale = 30.0f;
            Vector3 pos = new Vector3(sP.X, sP.Y - footHeight, -sP.Z);

            pos *= scale;
            string key = "" + joint;
            BepuEntity bepuEntity;
            if (!bones.ContainsKey(key))
            {
                bepuEntity = XNAGame.Instance.createBox(pos, size, size, 1);
                bepuEntity.diffuse = new Vector3(0.5f, 0.5f, 0.5f);
                bepuEntity.body.BecomeKinematic();
                bones.Add(key, bepuEntity);
            }
            else
            {
                bepuEntity = bones[key];
                bepuEntity.body.Position = pos;
            }
        }

        private void DrawJoint(JointCollection joints, JointType joint, float size)
        {
            SkeletonPoint sP;
            sP = joints[joint].Position;

            float scale = 30.0f;
            Vector3 pos = new Vector3(sP.X, sP.Y - footHeight, -sP.Z);

            pos *= scale;
            string key = "" + joint;
            BepuEntity bepuEntity;
            if (!bones.ContainsKey(key))
            {
                bepuEntity = XNAGame.Instance.createBall(pos, size);
                bepuEntity.diffuse = new Vector3(1.0f, 0.0f, 0.0f);
                bepuEntity.body.BecomeKinematic();
                bones.Add(key, bepuEntity);
            }
            else
            {
                bepuEntity = bones[key];
                bepuEntity.body.Position = pos;
            }
        }

        private void DrawBone(JointCollection joints, JointType startJoint, JointType endJoint)
        {
            SkeletonPoint sP, eP;
            sP = joints[startJoint].Position;
            eP = joints[endJoint].Position;
            float scale = 30.0f;
            Vector3 start = new Vector3(sP.X, sP.Y - footHeight, -sP.Z);
            Vector3 end = new Vector3(eP.X, eP.Y - footHeight, -eP.Z);
            start *= scale;
            end *= scale;


            Vector3 boneVector = end - start;
            float boneLength = boneVector.Length();
            Vector3 centrePos = start + ((boneVector) / 2.0f);
            boneVector.Normalize();
            Vector3 axis = Vector3.Cross(Vector3.Up, boneVector);
            axis.Normalize();
            float theta = (float) Math.Acos(Vector3.Dot(Vector3.Up, boneVector));
            Quaternion q = Quaternion.CreateFromAxisAngle(axis, theta);
            BepuEntity cylEntity;
            String key = "" + startJoint + endJoint;
            if (!bones.ContainsKey(key))
            {
                cylEntity = XNAGame.Instance.createWheel(centrePos, boneLength, 0.5f, q);
                cylEntity.diffuse = new Vector3(0, 0, 1);
                cylEntity.body.BecomeKinematic();
                bones.Add(key, cylEntity);
            }
            else
            {
                cylEntity = bones[key];
                cylEntity.body.Position = centrePos;
                cylEntity.localTransform = Matrix.CreateScale(0.5f, boneLength, 0.5f);
                cylEntity.body.Orientation = q;
            }
            
            //Line.DrawLine(start, end, Color.White);
        }

        public override void Draw(GameTime gameTime)
        {
            if ((sensor != null) && (skeletonData != null)  && (LastStatus == KinectStatus.Connected ))
            {
                foreach (var skeleton in skeletonData)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        footHeight = Math.Min(skeleton.Joints[JointType.FootLeft].Position.Y, skeleton.Joints[JointType.FootRight].Position.Y);
                        // Draw Bones
                        DrawBone(skeleton.Joints, JointType.Head, JointType.ShoulderCenter);
                        DrawBone(skeleton.Joints, JointType.ShoulderCenter, JointType.ShoulderLeft);
                        DrawBone(skeleton.Joints, JointType.ShoulderCenter, JointType.ShoulderRight);
                        DrawBone(skeleton.Joints, JointType.ShoulderCenter, JointType.Spine);
                        DrawBone(skeleton.Joints, JointType.Spine, JointType.HipCenter);
                        DrawBone(skeleton.Joints, JointType.HipCenter, JointType.HipLeft);
                        DrawBone(skeleton.Joints, JointType.HipCenter, JointType.HipRight);

                        DrawBone(skeleton.Joints, JointType.ShoulderLeft, JointType.ElbowLeft);
                        DrawBone(skeleton.Joints, JointType.ElbowLeft, JointType.WristLeft);
                        DrawBone(skeleton.Joints, JointType.WristLeft, JointType.HandLeft);

                        DrawBone(skeleton.Joints, JointType.ShoulderRight, JointType.ElbowRight);
                        DrawBone(skeleton.Joints, JointType.ElbowRight, JointType.WristRight);
                        DrawBone(skeleton.Joints, JointType.WristRight, JointType.HandRight);

                        DrawBone(skeleton.Joints, JointType.HipLeft, JointType.KneeLeft);
                        DrawBone(skeleton.Joints, JointType.KneeLeft, JointType.AnkleLeft);
                        DrawBone(skeleton.Joints, JointType.AnkleLeft, JointType.FootLeft);

                        DrawBone(skeleton.Joints, JointType.HipRight, JointType.KneeRight);
                        DrawBone(skeleton.Joints, JointType.KneeRight, JointType.AnkleRight);
                        DrawBone(skeleton.Joints, JointType.AnkleRight, JointType.FootRight);

                        DrawFace(skeleton.Joints, JointType.Head, 8);
                        DrawJoint(skeleton.Joints, JointType.ShoulderCenter, 1);
                        DrawJoint(skeleton.Joints, JointType.ShoulderLeft, 1);
                        DrawJoint(skeleton.Joints, JointType.ShoulderRight, 1);
                        DrawJoint(skeleton.Joints, JointType.ElbowLeft, 1);
                        DrawJoint(skeleton.Joints, JointType.ElbowRight, 1);
                        DrawJoint(skeleton.Joints, JointType.WristLeft, 1);
                        DrawJoint(skeleton.Joints, JointType.WristRight, 1);

                        DrawJoint(skeleton.Joints, JointType.HipCenter, 1);
                        DrawJoint(skeleton.Joints, JointType.Spine, 1);
                        DrawJoint(skeleton.Joints, JointType.HipLeft, 1);
                        DrawJoint(skeleton.Joints, JointType.HipRight, 1);
                        DrawJoint(skeleton.Joints, JointType.KneeLeft, 1);
                        DrawJoint(skeleton.Joints, JointType.KneeRight, 1);
                        DrawJoint(skeleton.Joints, JointType.AnkleLeft, 1);
                        DrawJoint(skeleton.Joints, JointType.AnkleRight, 1);

                        DrawFace(skeleton.Joints, JointType.HandLeft, 8);
                        DrawFace(skeleton.Joints, JointType.HandRight, 8);

                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (sensor.SkeletonStream != null)
            {
                using (var skeletonFrame = sensor.SkeletonStream.OpenNextFrame(0))
                {
                    // Sometimes we get a null frame back if no data is ready
                    if (null == skeletonFrame)
                    {
                        return;
                    }

                    // Reallocate if necessary
                    if (null == skeletonData || skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                    {
                        skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(skeletonData);
                }
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // If the status is not connected, try to stop it
            if (e.Status != KinectStatus.Connected)
            {
                e.Sensor.Stop();
            }

            LastStatus = e.Status;
            DiscoverSensor();
        }
    }
}
