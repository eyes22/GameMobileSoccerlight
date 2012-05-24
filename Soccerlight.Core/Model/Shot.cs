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
    public class Shot
    {
        public Shot(Player player, Point target, double strength, int value)
        {
            this.Player = player;
            this.Target = target;
            this.Strength = strength;
            this.Value = value;
        }
        public Player Player { get; set; }
        public Point Target { get; set; }
        public double Strength { get; set; }
        public int Value { get; set; }
    }
}
