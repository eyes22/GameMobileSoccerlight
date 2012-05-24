using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Soccerlight.Core.Model
{
    public class Goal
    {
        #region attributes
        IGoalObserver subscriber;
        #endregion attributes

        #region constructor
        public Goal(IGoalObserver subscriber, int id, Point nwPoint, Point nePoint, Point swPoint, Point sePoint)
        {
            this.ID = id;
            this.NWPoint = nwPoint;
            this.NEPoint = nePoint;
            this.SWPoint = swPoint;
            this.SEPoint = sePoint;
            this.subscriber = subscriber;
        }
        #endregion constructor
        
        #region properties
        public int ID { get; set; }

        public Point NWPoint { get; set; }
        public Point NEPoint { get; set; }
        public Point SWPoint { get; set; }
        public Point SEPoint { get; set; }

        #endregion properties

        #region functions
        public bool IsBallInGoal(Ball ball)
        {
            bool ret = false;

            if (
                ball.Position.X > NWPoint.X &&
                ball.Position.X < NEPoint.X &&
                ball.Position.Y > NWPoint.Y &&
                ball.Position.Y < SWPoint.Y)
            {
                bool wasInFrontOfGoal = false;
                if (this.ID == 1) //left goal
                {
                    wasInFrontOfGoal = (ball.X - ball.TranslateVelocity.X  > SEPoint.X);
                }
                else
                {
                    wasInFrontOfGoal = (ball.X - ball.TranslateVelocity.X < NWPoint.X);
                }

                if (wasInFrontOfGoal)
                {
                    if (!ball.IsBallInGoal)
                    {
                        ball.LastGoalID = ID;
                        ball.IsBallInGoal = true;
                        subscriber.BallEnteredGoal(this, ball);
                    }
                    ret = true;
                }
                else
                {
                    ret = false;
                }
            }
            else
            {
                if (ball.IsBallInGoal && ball.LastGoalID == ID)
                {
                    ball.IsBallInGoal = false;
                    subscriber.BallLeftGoal(this, ball);
                }
            }

            return ret;
        }

        public double HotSpotX { get { return (this.NWPoint.X + this.SEPoint.X) / 2; } }
        public double HotSpotY { get { return (this.NWPoint.Y + this.SEPoint.Y) / 2; } }
        public List<Point> HotSpots
        {
            get
            {
                List<Point> ret = new List<Point>();
                ret.Add(new Point((this.NWPoint.X + this.SEPoint.X) / 2, this.NWPoint.Y));
                ret.Add(new Point((this.NWPoint.X + this.SEPoint.X) / 2, (this.NWPoint.Y + this.SEPoint.Y) / 2));
                ret.Add(new Point((this.NWPoint.X + this.SEPoint.X) / 2, this.SEPoint.Y));
                return ret;
            }
        }
        #endregion functions
    }
}
