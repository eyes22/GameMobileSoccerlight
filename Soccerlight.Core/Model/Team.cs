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
using System.Collections.Generic;

namespace Soccerlight.Core.Model
{
    public class Team
    {
        int currentTeamIndex = 0;

        public Team()
        {
            Formation = new int[] {1, 3, 5, 2};
        }

        public string TeamID { get; set; }
        public string TeamName { get; set; }
        public string GroupID { get; set; }
        public Color NumberColor { get; set; }
        public byte R1 { get; set; }
        public byte G1 { get; set; }
        public byte B1 { get; set; }
        public byte R2 { get; set; }
        public byte G2 { get; set; }
        public byte B2 { get; set; }
        public byte R3 { get; set; }
        public byte G3 { get; set; }
        public byte B3 { get; set; }
        //public Dictionary<int, Player> players = new Dictionary<int, Player>();
        public List<Player> players = new List<Player>();
        public int CurrentGoalID { get; set; }
        public int[] Formation { get; set; }
        
        //public Ball BallOn { get; set; }

        public int Points { get; set; }

        //public List<int> FoulList { get; set; }

        public int Strength { get; set; }

        public bool JustSwapped { get; set; }

        public int ShotCount { get; set; }

        public Player CurrentPlayer
        {
            get
            {

                if (currentTeamIndex > players.Count - 1)
                    return null;
                else
                    return players[currentTeamIndex];
            }
            set
            {
                players[currentTeamIndex] = value;
            }
        }

        public void NextPlayer()
        {
            currentTeamIndex++;

            if (currentTeamIndex >= players.Count)
                currentTeamIndex = 0;
        }

        public Vector2D TestPosition { get; set; }

        public int TestStrength { get; set; }

        public int Attempts { get; set; }
        public int AttemptsToWin { get; set; }
        public int AttemptsNotToLose { get; set; }
        public int AttemptsOfDespair { get; set; }
        public bool BestShotSelected { get; set; }
        //public TestShot BestShot { get; set; }
        //public TestShot LastShot { get; set; }
        public bool IsRotatingCue { get; set; }
        public float CurrentCueAngle { get; set; }
        public float FinalCueAngle { get; set; }

    }
}
