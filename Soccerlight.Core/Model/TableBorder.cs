using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Soccerlight.Core.Model
{
    public class TableBorder
    {
        #region attributes
        public static string message;
        //IBallObserver observer;
        double x;
        double y;
        double width;
        double height;
        Vector2D position;
        Vector2D size;
        #endregion attributes

        #region constructor
        public TableBorder(int x, int y, int width, int height)
        {
            //this.observer = observer;
            this.position = new Vector2D(x, y);
            this.size = new Vector2D(width, height);
        }
        #endregion constructor

        #region properties
        public double X
        {
            get { return x; }
            set { x = value; }
        }
        public double Y
        {
            get { return y; }
            set { y = value; }
        }
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
        #endregion properties

        #region functions
        public RectangleCollision Colliding(Discoid discoid)
        {
            RectangleCollision collision = RectangleCollision.None;

            double mediumX = (discoid.LastX + discoid.Position.X) / 2.0;
            double mediumY = (discoid.LastY + discoid.Position.Y) / 2.0;

            //if (!discoid.IsBallInGoal)
            //{
                //bool insideWidth = (discoid.X > position.X) && (discoid.X < position.X + size.X);
                //bool insideHeight = (discoid.Y > position.Y) && (discoid.Y < position.Y + size.Y);

            bool insideWidth = ((discoid.Position.X > position.X) && (discoid.Position.X < position.X + size.X));
            bool insideHeight = (discoid.Position.Y > position.Y) && (discoid.Position.Y < position.Y + size.Y);

                //if ((discoid.X < position.X) && (discoid.X + discoid.Radius > position.X) && (discoid.X - discoid.Radius < position.X + size.X) && insideHeight && (discoid.TranslateVelocity.X + discoid.VSpinVelocity.X > 0))
            if ((discoid.LastX < position.X) && (discoid.Position.X + discoid.Radius > position.X) && (discoid.Position.X - discoid.Radius < position.X + size.X) && insideHeight && (discoid.TranslateVelocity.X + discoid.VSpinVelocity.X > 0))
                {
                    collision = RectangleCollision.Left;
                }
                //else if ((discoid.X > (position.X + size.X)) && (discoid.X - discoid.Radius < position.X + size.X) && (discoid.X + discoid.Radius > position.X) && insideHeight && (discoid.TranslateVelocity.X + discoid.VSpinVelocity.X < 0))
            else if ((discoid.LastX + discoid.Radius > position.X + size.X) && (discoid.Position.X - discoid.Radius < position.X + size.X) && (discoid.Position.X + discoid.Radius > position.X) && insideHeight && (discoid.TranslateVelocity.X + discoid.VSpinVelocity.X < 0))
                {
                    collision = RectangleCollision.Right;
                }

                //if ((discoid.Y < position.Y) && (discoid.Y + discoid.Radius > position.Y) && (discoid.Y - discoid.Radius < position.Y + size.Y) && insideWidth && (discoid.TranslateVelocity.Y + discoid.VSpinVelocity.Y > 0))
            if ((discoid.LastY < position.Y) && (discoid.Position.Y + discoid.Radius > position.Y) && (discoid.Position.Y - discoid.Radius < position.Y + size.Y) && insideWidth && (discoid.TranslateVelocity.Y + discoid.VSpinVelocity.Y > 0))
                {
                    collision = RectangleCollision.Top;
                }
            else if ((discoid.Position.Y + discoid.Radius > position.Y + size.Y) && (discoid.Position.Y - discoid.Radius < position.Y + size.Y) && (discoid.Position.Y + discoid.Radius > position.Y) && insideWidth && (discoid.TranslateVelocity.Y + discoid.VSpinVelocity.Y < 0))
                //else if ((discoid.LastY > (position.Y + size.Y)) && (discoid.Y - discoid.Radius < position.Y + size.Y) && (discoid.Y + discoid.Radius > position.Y) && insideWidth && (discoid.TranslateVelocity.Y + discoid.VSpinVelocity.Y < 0))
                {
                    collision = RectangleCollision.Bottom;
                }
            //}

            return collision;
        }

        public void ResolveCollision(Discoid discoid, RectangleCollision collision)
        {
            double absorption = 0.9f;

            switch (collision)
            {
                case RectangleCollision.Right:
                case RectangleCollision.Left:
                    //double dX = 0;
                    //if (collision == RectangleCollision.Left)
                    //{
                    //    dX = (discoid.X + discoid.Radius) - this.position.X;
                    //}
                    //else
                    //{
                    //    dX = (this.position.X + this.size.X ) - (discoid.X + discoid.Radius);
                    //}
                    //discoid.Position.X = this.position.X - dX;
                    if (Math.Sign(discoid.TranslateVelocity.X) == Math.Sign(discoid.VSpinVelocity.X) && discoid.VSpinVelocity.X > 0.0)
                    {
                        discoid.TranslateVelocity = discoid.TranslateVelocity.Add(new Vector2D(discoid.VSpinVelocity.X, 0));
                        discoid.VSpinVelocity = discoid.VSpinVelocity.Add(new Vector2D(0, discoid.VSpinVelocity.Y));
                    }
                    discoid.TranslateVelocity.X = discoid.TranslateVelocity.X * (-1.0f * absorption);
                    break;
                case RectangleCollision.Bottom:
                case RectangleCollision.Top:
                    //double dY = 0;
                    //if (collision == RectangleCollision.Top)
                    //{
                    //    dY = (discoid.Y + discoid.Radius) - this.position.Y;
                    //}
                    //else
                    //{
                    //    dY = this.position.Y - (discoid.Y + discoid.Radius);
                    //}
                    //discoid.Position.Y = this.position.Y - dY;
                    if (Math.Sign(discoid.TranslateVelocity.Y) == Math.Sign(discoid.VSpinVelocity.Y) && discoid.VSpinVelocity.Y > 0.0)
                    {
                        discoid.TranslateVelocity = discoid.TranslateVelocity.Add(new Vector2D(0, discoid.VSpinVelocity.Y));
                        discoid.VSpinVelocity = discoid.VSpinVelocity.Add(new Vector2D(discoid.VSpinVelocity.X, 0));
                    }
                    discoid.TranslateVelocity.Y = discoid.TranslateVelocity.Y * (-1.0f * absorption);
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("TableBorder({0}, {1}, {2}, {3})", position.X, position.Y, position.X + size.X, position.Y + size.Y);
        }

        //public override void Draw(SpriteBatch spriteBatch, Vector2D offset)
        //{
        //    Vector2D position = new Vector2D(this.position.X + offset.X, this.position.Y + offset.Y);
        //    Color color = new Color(255, 255, 255, 255);
        //    spriteBatch.Draw(texture, position, new Rectangle(0, 0, (int)size.X, (int)size.Y), color, 0f, new Vector2D(0, 0), (1.0f / 1.0f), SpriteEffects.None, 0f);
        //}

        #endregion functions
    }

    public enum RectangleCollision
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
