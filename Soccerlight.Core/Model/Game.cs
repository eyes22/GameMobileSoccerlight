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
    public class Game
    {
        public Game(string stage, DateTime date, Team team1, Team team2, string stadium, string city)
        {
            Stage = stage;
            Date = date;
            Stadium = stadium;
            City = city;
            Teams = new Dictionary<string, Team>();
            Scores = new Dictionary<string, int>();
            Fouls = new Dictionary<string, int>();

            Team1ID = team1.TeamID;
            Team2ID = team2.TeamID;

            Team1 = team1;
            Team2 = team2;

            Teams.Add(team1.TeamID, team1);
            Teams.Add(team2.TeamID, team2);

            Scores.Add(team1.TeamID, 0);
            Scores.Add(team2.TeamID, 0);

            Fouls.Add(team1.TeamID, 0);
            Fouls.Add(team2.TeamID, 0);

            PlayingTeamID = team1.TeamID;
            AwaitingTeamID = team2.TeamID;
            //PlayingTeamID = team2.TeamID;
            //AwaitingTeamID = team1.TeamID;

            Team1BallStrength = 50;
            Team2BallStrength = 50;
        }
        public Team Team1 { get; set; }
        public Team Team2 { get; set; }
        public string Team1ID { get; set; }
        public string Team2ID { get; set; }
        public Dictionary<string, Team> Teams { get; set; }
        public Dictionary<string, int> Scores { get; set; }
        public Dictionary<string, int> Fouls { get; set; }
        public string PlayingTeamID { get; set; }
        public string AwaitingTeamID { get; set; }
        public double Team1BallStrength { get; set; }
        public double Team2BallStrength { get; set; }
        public string Stage { get; set; }
        public DateTime Date { get; set; }
        public string Stadium { get; set; }
        public string City { get; set; }
        public void SwapTeams()
        {
            string AuxTeamID = Team1ID;
            Team1ID = Team2ID;
            Team2ID = AuxTeamID;

            Team auxTeam = Team1;
            Team1 = Team2;
            Team2 = auxTeam;

            Teams.Clear();
            Teams.Add(Team1ID, Team1);
            Teams.Add(Team2ID, Team2);

            Scores.Clear();
            Scores.Add(Team1ID, 0);
            Scores.Add(Team2ID, 0);

            Fouls.Clear();
            Fouls.Add(Team1ID, 0);
            Fouls.Add(Team2ID, 0);

            PlayingTeamID = Team1ID;
            AwaitingTeamID = Team2ID;
        }
    }
}
