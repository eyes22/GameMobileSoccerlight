using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Soccerlight.Core.Model
{
    public class GhostBall
    {
        public GhostBall(Player player, Point point, double player2BallDistance, double ball2GoalDistance, int difficulty)
        {
            this.Player = player;
            this.Point = point;
            this.Player2BallDistance = player2BallDistance;
            this.Ball2GoalDistance = ball2GoalDistance;
            this.Difficulty = difficulty;
        }

        public Player Player { get; set; }
        public Point Point { get; set; }
        public double Player2BallDistance { get; set; }
        public double Ball2GoalDistance { get; set; }
        public int Difficulty { get; set; }
    }
}
