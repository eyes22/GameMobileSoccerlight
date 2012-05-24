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
    public abstract class TurnEvent
    {
        static int id = 0;

        public int ID
        {
            get { return id++; }
            set { id = value; }
        }
        public string PlayingTeamID { get; set; }
    }

    public class PlayerToPlayerContact : TurnEvent
    {
        public PlayerToPlayerContact(string playingTeamID, Player player1, Player player2, Point location)
        {
            this.PlayingTeamID = playingTeamID;
            Player1 = player1;
            Player2 = player2;
            Location = location;
        }

        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Point Location { get; set; }
    }

    public class PlayerToBallContact : TurnEvent
    {
        public PlayerToBallContact(string playingTeamID, Player player, Point location)
        {
            this.PlayingTeamID = playingTeamID;
            Player = player;
            Location = location;
        }

        public Player Player { get; set; }
        public Point Location { get; set; }
    }

    public class BallEnteredGoal : TurnEvent
    {
        public BallEnteredGoal(string playingTeamID, Goal goal, Point location)
        {
            this.PlayingTeamID = playingTeamID;
            Goal = goal;
            Location = location;
        }

        public Goal Goal { get; set; }
        public Point Location { get; set; }
    }
}
