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
using Soccerlight.Core.Model;
using System.Collections.Generic;

namespace Soccerlight.Controls
{
    public class GameHelper
    {
        static GameHelper instance;

        private GameHelper()
        {
            Groups = new List<Group>();
            TeamCodes = new List<string>();
            TeamsDictionary = new Dictionary<string, Team>();
            AddGroups();
            AddTeams();
            AddTeamCodes();
        }

        private void AddTeamCodes()
        {
            TeamCodes.Add("RSA");
            TeamCodes.Add("MEX");
            TeamCodes.Add("URU");
            TeamCodes.Add("FRA");
            TeamCodes.Add("ARG");
            TeamCodes.Add("NGA");
            TeamCodes.Add("KOR");
            TeamCodes.Add("GRE");
            TeamCodes.Add("ENG");
            TeamCodes.Add("USA");
            TeamCodes.Add("ALG");
            TeamCodes.Add("SVN");
            TeamCodes.Add("GER");
            TeamCodes.Add("AUS");
            TeamCodes.Add("SRB");
            TeamCodes.Add("GHA");
            TeamCodes.Add("NED");
            TeamCodes.Add("DEN");
            TeamCodes.Add("JPN");
            TeamCodes.Add("CMR");
            TeamCodes.Add("ITA");
            TeamCodes.Add("PAR");
            TeamCodes.Add("NZL");
            TeamCodes.Add("SVK");
            TeamCodes.Add("BRA");
            TeamCodes.Add("PRK");
            TeamCodes.Add("CIV");
            TeamCodes.Add("POR");
            TeamCodes.Add("ESP");
            TeamCodes.Add("SUI");
            TeamCodes.Add("HON");
            TeamCodes.Add("CHI");
        }

        public static GameHelper Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameHelper();
                return instance;
            }
        }

        public Player CurrentMousePlayer { get; set; }
        public Player CurrentSelectedPlayer { get; set;}
        public bool IsMovingDiscoids { get; set; }
        public List<Group> Groups { get; set; }
        public Dictionary<string, Team> TeamsDictionary { get; set; }
        public List<string> TeamCodes { get; set; }

        void AddGroups()
        {
            Groups.Add(new Group() { GroupID = "A" });
            Groups.Add(new Group() { GroupID = "B" });
            Groups.Add(new Group() { GroupID = "C" });
            Groups.Add(new Group() { GroupID = "D" });
            Groups.Add(new Group() { GroupID = "E" });
            Groups.Add(new Group() { GroupID = "F" });
            Groups.Add(new Group() { GroupID = "G" });
            Groups.Add(new Group() { GroupID = "H" });
        }

        void AddTeams()
        {
            TeamsDictionary.Add("RSA", new Team() { TeamID = "RSA", TeamName = "South Africa", NumberColor = Colors.White, GroupID = "A", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("MEX", new Team() { TeamID = "MEX", TeamName = "Mexico", NumberColor = Colors.White, GroupID = "A", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("URU", new Team() { TeamID = "URU", TeamName = "Uruguay", NumberColor = Colors.Black, GroupID = "B", R1 = 0x00, G1 = 0xE0, B1 = 0xE0, R2 = 0x00, G2 = 0xF0, B2 = 0xFF, R3 = 0x00, G3 = 0xF0, B3 = 0xFF });
            TeamsDictionary.Add("FRA", new Team() { TeamID = "FRA", TeamName = "France", NumberColor = Colors.White, GroupID = "A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            TeamsDictionary.Add("ARG", new Team() { TeamID = "ARG", TeamName = "Argentina", NumberColor = Colors.Black, GroupID = "B", R1 = 0x00, G1 = 0xE0, B1 = 0xE0, R2 = 0x00, G2 = 0xF0, B2 = 0xFF, R3 = 0x00, G3 = 0xF0, B3 = 0xFF });
            TeamsDictionary.Add("NGA", new Team() { TeamID = "NGA", TeamName = "Nigeria", NumberColor = Colors.White, GroupID = "B", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("KOR", new Team() { TeamID = "KOR", TeamName = "South Korea", NumberColor = Colors.White, GroupID = "B", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("GRE", new Team() { TeamID = "GRE", TeamName = "Greece", NumberColor = Colors.White, GroupID = "A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            TeamsDictionary.Add("ENG", new Team() { TeamID = "ENG", TeamName = "England", NumberColor = Colors.White, GroupID = "C", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("USA", new Team() { TeamID = "USA", TeamName = "United States", NumberColor = Colors.Black, GroupID = "C", R1 = 0xB0, G1 = 0xB0, B1 = 0xB0, R2 = 0xC0, G2 = 0xC0, B2 = 0xC0, R3 = 0xF0, G3 = 0xF0, B3 = 0xF0 });
            TeamsDictionary.Add("ALG", new Team() { TeamID = "ALG", TeamName = "Algeria", NumberColor = Colors.White, GroupID = "C", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("SVN", new Team() { TeamID = "SVN", TeamName = "Slovenia", NumberColor = Colors.White, GroupID = "A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            TeamsDictionary.Add("GER", new Team() { TeamID = "GER", TeamName = "Germany", NumberColor = Colors.Black, GroupID = "D", R1 = 0xB0, G1 = 0xB0, B1 = 0xB0, R2 = 0xC0, G2 = 0xC0, B2 = 0xC0, R3 = 0xF0, G3 = 0xF0, B3 = 0xF0 });
            TeamsDictionary.Add("AUS", new Team() { TeamID = "AUS", TeamName = "Australia", NumberColor = Colors.Blue, GroupID = "G", R1 = 0x80, G1 = 0x70, B1 = 0x00, R2 = 0xC0, G2 = 0xB0, B2 = 0x00, R3 = 0xC0, G3 = 0xB0, B3 = 0x00 });
            TeamsDictionary.Add("SRB", new Team() { TeamID = "SRB", TeamName = "Serbia", NumberColor = Colors.White, GroupID = "D", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("GHA", new Team() { TeamID = "GHA", TeamName = "Ghana", NumberColor = Colors.White, GroupID = "D", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("NED", new Team() { TeamID = "NED", TeamName = "Netherlands", NumberColor = Colors.White, GroupID = "E", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("DEN", new Team() { TeamID = "DEN", TeamName = "Denmark", NumberColor = Colors.White, GroupID = "E", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("JPN", new Team() { TeamID = "JPN", TeamName = "Japan", NumberColor = Colors.White, GroupID = "A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            TeamsDictionary.Add("CMR", new Team() { TeamID = "CMR", TeamName = "Cameroon", NumberColor = Colors.White, GroupID = "E", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("ITA", new Team() { TeamID = "ITA", TeamName = "Italy", NumberColor = Colors.White, GroupID = "A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            TeamsDictionary.Add("PAR", new Team() { TeamID = "PAR", TeamName = "Paraguay", NumberColor = Colors.White, GroupID = "F", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("NZL", new Team() { TeamID = "NZL", TeamName = "New Zealand", NumberColor = Colors.White, GroupID = "F", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("SVK", new Team() { TeamID = "SVK", TeamName = "Slovakia", NumberColor = Colors.White, GroupID = "A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            TeamsDictionary.Add("BRA", new Team() { TeamID = "BRA", TeamName = "Brazil", NumberColor = Colors.Blue, GroupID = "G", R1 = 0x80, G1 = 0x70, B1 = 0x00, R2 = 0xC0, G2 = 0xB0, B2 = 0x00, R3 = 0xC0, G3 = 0xB0, B3 = 0x00 });
            TeamsDictionary.Add("PRK", new Team() { TeamID = "PRK", TeamName = "North Korea", NumberColor = Colors.White, GroupID = "G", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("CIV", new Team() { TeamID = "CIV", TeamName = "Ivory Coast", NumberColor = Colors.Blue, GroupID = "G", R1 = 0x80, G1 = 0x70, B1 = 0x00, R2 = 0xC0, G2 = 0xB0, B2 = 0x00, R3 = 0xC0, G3 = 0xB0, B3 = 0x00 });
            TeamsDictionary.Add("POR", new Team() { TeamID = "POR", TeamName = "Portugal", NumberColor = Colors.White, GroupID = "G", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("ESP", new Team() { TeamID = "ESP", TeamName = "Spain", NumberColor = Colors.White, GroupID = "H", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("SUI", new Team() { TeamID = "SUI", TeamName = "Switzerland", NumberColor = Colors.White, GroupID = "H", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            TeamsDictionary.Add("HON", new Team() { TeamID = "HON", TeamName = "Honduras", NumberColor = Colors.White, GroupID = "A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            TeamsDictionary.Add("CHI", new Team() { TeamID = "CHI", TeamName = "Chile", NumberColor = Colors.White, GroupID = "H", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
        }
    }
}
