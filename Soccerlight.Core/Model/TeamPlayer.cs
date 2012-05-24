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
    public class TeamPlayer
    {
        //public TeamPlayer(string teamID, int number, int id, string name, string birthDate, string position, string currentTeam, double height)
        public TeamPlayer(string teamID, int number, int id, string name)
        {
            TeamID = teamID;
            Number = number;
            ID = id;
            Name = name;
            //BirthDate = DateTime.Today;// birthDate;
            //Position = position;
            //CurrentTeam = currentTeam;
            //Height = height;
        }
        public string TeamID { get; set; }
        public int Number { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public string Position { get; set; }
        public string CurrentTeam { get; set; }
        public double Height { get; set; }
    }
}
