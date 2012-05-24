using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Soccerlight.Core.Model
{
    public abstract class Discoid
    {
        #region attributes
        protected double mass = 1;
        protected double radius = 27;
        protected bool isFixed = false;
        protected float friction = 0.015F;
        bool isStill = true;
        string id;
        protected Vector2D position = new Vector2D(0, 0);
        Vector2D size = new Vector2D(0, 0);
        Vector2D initPosition = new Vector2D(0, 0);
        Vector2D lastPosition = new Vector2D(0, 0);
        //double x = 0;
        //double y = 0;
        double width;
        double height;
        double rad = 0;
        Vector2D translateVelocity = new Vector2D(0, 0);
        Vector2D vSpinVelocity = new Vector2D(0, 0);
        Vector2D hSpinVelocity = new Vector2D(0, 0);
        int points;
        bool isBallInGoal = false;
        static StreamReader reader;
        int alphaValue = 255;
        List<Vector2D> lights = new List<Vector2D>();
        Vector2D drawPosition = new Vector2D(0, 0);
        #endregion attributes

        #region constructor

        public Discoid(double radius, double mass, bool isFixed)
        {
            this.size = size;
            this.id = id;
            this.radius = radius;
            this.mass = mass;
            this.isFixed = isFixed;
            width = 32;
            height = 32;
            initPosition = new Vector2D(position.X, position.Y);
            lastPosition = new Vector2D(X, Y);
            position = new Vector2D(0, 0);
        }

        #endregion constructor

        #region properties

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public bool IsBallInGoal
        {
            get { return isBallInGoal; }
            set
            {
                if (isBallInGoal == false && value)
                {
                    if (value && id == "01")
                    {
                        isBallInGoal = false;
                        position.X = initPosition.X;
                        position.Y = initPosition.Y;
                        return;
                    }
                }

                isBallInGoal = value;
                if (!isBallInGoal)
                {
                    alphaValue = 255;
                }
            }
        }

        public int Points
        {
            get { return points; }
        }

        public double X
        {
            get { return position.X; }
            set
            {
                this.lastPosition.X = position.X;
                position.X = value;
                isStill = false;
            }
        }

        public double Y
        {
            get { return position.Y; }
            set
            {
                this.lastPosition.Y = position.Y;
                position.Y = value;
                isStill = false;
            }
        }

        public double LastX
        {
            get { return lastPosition.X; }
            //set { lastX = value; }
        }

        public double LastY
        {
            get { return lastPosition.Y; }
            //set { lastY = value; }
        }

        //public Vector2D LastPosition
        //{
        //    get { return lastPosition; }
        //    set { lastPosition = value; }
        //}

        public double Width
        {
            get { return width; }
            set { width = value; }
        }

        public double Height
        {
            get { return height; }
            set { height = value; }
        }

        public double Rad
        {
            get { return rad; }
            set { rad = value; }
        }

        private Vector2D Velocity
        {
            get { return translateVelocity; }
        }

        public Vector2D TranslateVelocity
        {
            get { return translateVelocity; }
            set { translateVelocity = value; }
        }

        public Vector2D VSpinVelocity
        {
            get { return vSpinVelocity; }
            set { vSpinVelocity = value; }
        }

        public Vector2D HSpinVelocity
        {
            get { return hSpinVelocity; }
            set { hSpinVelocity = value; }
        }

        public bool IsStill
        {
            get { return isStill; }
            set { isStill = value; }
        }

        public void ResetPosition()
        {
            translateVelocity = new Vector2D(0, 0);
            isBallInGoal = false;
            position.X = initPosition.X;
            position.Y = initPosition.Y;
            lastPosition.X = position.X;
            lastPosition.Y = position.Y;
        }

        public Vector2D Position
        {
            get
            {
                return position;
            }
            set
            {
                this.lastPosition.X = position.X;
                this.lastPosition.Y = position.Y;

                position.X = value.X;
                position.Y = value.Y;
                isStill = false;
            }
        }

        public Vector2D InitPosition { get { return initPosition; } set { initPosition = value; } }

        public int AlphaValue { get { return alphaValue; } set { alphaValue = value; } }

        //public Vector2D DrawPosition { get { return drawPosition; } set { drawPosition = value; } }

        #endregion properties

        #region functions

        public void ResetPositionAt(float x, float y)
        {
            translateVelocity = new Vector2D(0, 0);
            isBallInGoal = false;
            this.X = x;
            this.Y = y;
            lastPosition.X = x;
            lastPosition.Y = y;
            alphaValue = 255;
        }

        public void SetPosition(float x, float y)
        {
            this.X = x;
            this.Y = y;

            //lastX = x;
            //LastY = y;
            isStill = false;
        }

        public bool Colliding(Discoid otherDiscoid)
        {
            //if (!otherDiscoid.IsBallInGoal && !IsBallInGoal)
            //{
            float xd = (float)(position.X - otherDiscoid.Position.X);
            float yd = (float)(position.Y - otherDiscoid.Position.Y);

                //float sumRadius = (float)((this.Radius + 1.0) + (otherDiscoid.Radius + 1.0));
                float sumRadius = (float)((this.Radius + 1.0) + (otherDiscoid.Radius + 1.0));
                float sqrRadius = sumRadius * sumRadius;

                float distSqr = (xd * xd) + (yd * yd);

                if (Math.Round(distSqr) < Math.Round(sqrRadius))
                {
                    return true;
                }
            //}

            return false;
        }

        public void ResolveCollision(Discoid otherDiscoid)
        {
            // get the mtd
            Vector2D delta = (position.Subtract(otherDiscoid.position));
            float d = delta.Length();
            // minimum translation distance to push balls apart after intersecting
            Vector2D mtd = delta.Multiply((float)(((this.Radius + 1.0 + otherDiscoid.Radius + 1.0) - d) / d));

            // resolve intersection --
            // inverse mass quantities
            double im1 = 1f / this.Mass;
            double im2 = 1f / otherDiscoid.Mass;

            // push-pull them apart based off their mass

            if (!this.IsFixed)
                position = position.Add((mtd.Multiply(im1 / (im1 + im2))));

            if (!otherDiscoid.IsFixed)
                otherDiscoid.position = otherDiscoid.position.Subtract(mtd.Multiply(im2 / (im1 + im2)));

            // impact speed
            Vector2D v = (this.translateVelocity.Subtract(otherDiscoid.translateVelocity));
            Vector2D mtdNormalize = new Vector2D(mtd.X, mtd.Y);
            mtdNormalize.Normalize();
            float vn = v.Dot(mtd.Normalize());

            // sphere intersecting but moving away from each other already
            if (vn > 0.0f)
                return;

            // collision impulse
            float i = Math.Abs((float)((-(1.0f + 0.1) * vn) / (im1 + im2)));
            
            Vector2D impulse = mtd.Multiply(0.5);
            //Vector2D impulse = mtd.Multiply(1.0);

            //double i = (float)((-(0.1f + 0.1) * vn)) / (im1 + im2);
            //Vector2D impulse = mtd.Multiply(i);

            int hitSoundIntensity = (int)(Math.Abs(impulse.X) + Math.Abs(impulse.Y));

            if (hitSoundIntensity > 5)
                hitSoundIntensity = 5;

            if (hitSoundIntensity < 1)
                hitSoundIntensity = 1;

            //GameSound sound = GameSound.None;
            //switch (hitSoundIntensity)
            //{
            //    case 1:
            //        sound = GameSound.Hit01;
            //        break;
            //    case 2:
            //        sound = GameSound.Hit02;
            //        break;
            //    case 3:
            //        sound = GameSound.Hit03;
            //        break;
            //    case 4:
            //        sound = GameSound.Hit04;
            //        break;
            //    case 5:
            //        sound = GameSound.Hit05;
            //        break;
            //    case 6:
            //        sound = GameSound.Hit06;
            //        break;
            //}

            //observer.Hit(sound);

            // change in momentum
            if (!isFixed)
                this.translateVelocity = this.translateVelocity.Add(impulse.Multiply(im1));
            if (!otherDiscoid.IsFixed)
                otherDiscoid.translateVelocity = otherDiscoid.translateVelocity.Subtract(impulse.Multiply(im2));
        }

        private void wavDiscoid_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //((System.Media.SoundPlayer)sender).Play();
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", (int)position.X, (int)position.Y);
        }

        //public override bool Equals(object obj)
        //{
        //    return ((Discoid)obj).id.Equals(id);
        //}

        public double Radius
        {
            get { return radius; }
        }

        public double Mass
        {
            get { return mass; }
        }

        public bool IsFixed
        {
            get { return isFixed; }
        }

        public float Friction
        {
            get { return friction; }
        }

        #endregion functions
    }
}

