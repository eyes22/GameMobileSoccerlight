using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Soccerlight.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Soccerlight.Core;
using Soccerlight.Core.Model;
using System.IO;
using System.Reflection;

namespace Soccerlight.View
{
    public partial class MainPage : BasePage, IGoalObserver
    {
        List<TurnEvent> turnEvents = new List<TurnEvent>();
        bool fallenBallsProcessed = false; 
        PlayerState playerState = PlayerState.None;
        PlayerState lastPlayerState = PlayerState.None;
        List<Discoid> strokenDiscoids = new List<Discoid>(); 
        static List<string> logList = new List<string>(); 
        GameState currentGameState = GameState.None; 
        List<Ball> pottedBalls = new List<Ball>();
        bool afterTurnProcessed = false;
        Random random = new Random(DateTime.Now.Millisecond);
        bool started = false;
        Game currentGame = null;
        List<Group> groups;
        Dictionary<string, Team> teamsDictionary;
        static List<Team> teams = new List<Team>();
        List<TeamPlayer> teamPlayers = new List<TeamPlayer>();
        DispatcherTimer movementTimer = new DispatcherTimer();
        DispatcherTimer clockTimer = new DispatcherTimer();
        BallFace bf;

        Ball ball;        

        List<PlayerFace> playerFaces = new List<PlayerFace>();
        List<Discoid> discoids = new List<Discoid>();
        List<Discoid> strokenPlayers = new List<Discoid>();
        List<TableBorder> tableBorders = new List<TableBorder>();
        List<GoalPost> goalPosts = new List<GoalPost>();

        Point targetPoint = new Point(0,0);
        Vector2D targetVector = new Vector2D(0, 0);
        Point strengthPointNW = new Point(0, 0);
        Point strengthPointSE = new Point(0, 0);

        bool calculatingPositions = true;
        Grid scoreGrid = new Grid();
        ScoreControl scoreControl = new ScoreControl();
        List<Goal> goals = new List<Goal>();
        double lastTotalVelocity = 0;
        bool hasPendingGoalResolution = false;
        double fieldWidth;
        double fieldHeight;
        Point goalPost00Point;
        Point goalPost01Point;
        Point goalPost10Point;
        Point goalPost11Point;
        Storyboard sbStadiumScreen;
        List<Game> gameTable = new List<Game>();
        DateTime totalTime = new DateTime(1, 1, 1, 0, 30, 0);
        string selectedTeamID;

        public MainPage(Dictionary<string, object> parameters) : base(parameters)
        {
            InitializeComponent();

            GameHelper.Instance.IsMovingDiscoids = false;

            clockTimer.Tick += new EventHandler(clockTimer_Tick);
            clockTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);

            movementTimer.Tick += new EventHandler(movementTimer_Tick);
            movementTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            movementTimer.Start();

            clockTimer.Start();

            groups = GameHelper.Instance.Groups;

            teamsDictionary = GameHelper.Instance.TeamsDictionary;

            LoadTeamPlayers(teamPlayers);
            
            LoadGameTable(gameTable);

            selectedTeamID = Parameters["TeamID1"].ToString();
            DateTime lastGameDate = new DateTime(2010,06,01);

            LoadBall();

            currentGame = GetNextGame(selectedTeamID, lastGameDate);

            strengthPointNW = new Point
            (
                this.colLeftEscapeArea.Width.Value +
                this.colLeftPosts.Width.Value +
                this.colLeftGoalArea.Width.Value +
                this.colLeftPenaltyArea.Width.Value +
                this.colHalfWay.Width.Value +
                this.colRightPenaltyArea.Width.Value +
                this.colRightGoalArea.Width.Value +
                this.colRightPosts.Width.Value,
                this.rowTopEscapeArea.Height.Value +
                this.rowTopFieldLine.Height.Value +
                this.rowTopPenaltyArea.Height.Value +
                this.rowTopGoalArea.Height.Value
            );

            strengthPointSE = new Point
            (
                strengthPointNW.X + colMenu.Width.Value,
                strengthPointNW.Y +
                rowBottomGoalArea.Height.Value +
                rowBottomPenaltyArea.Height.Value
            );

            fieldWidth =
                this.colLeftEscapeArea.Width.Value +
                this.colLeftPosts.Width.Value +
                this.colLeftGoalArea.Width.Value +
                this.colLeftPenaltyArea.Width.Value +
                this.colHalfWay.Width.Value +
                this.colRightPenaltyArea.Width.Value +
                this.colRightGoalArea.Width.Value +
                this.colRightPosts.Width.Value;

            fieldHeight =
                this.rowTopEscapeArea.Height.Value +
                this.rowTopFieldLine.Height.Value +
                this.rowTopPenaltyArea.Height.Value +
                this.rowTopGoalArea.Height.Value +
                this.rowBottomGoalArea.Height.Value +
                this.rowBottomPenaltyArea.Height.Value +
                this.rowBottomFieldLine.Height.Value;

            goalPost00Point = new Point(
                this.colLeftEscapeArea.Width.Value, 
                this.rowTopEscapeArea.Height.Value +
                this.rowTopFieldLine.Height.Value +
                this.rowTopPenaltyArea.Height.Value);

            goalPost01Point = new Point(goalPost00Point.X, goalPost00Point.Y + rowTopGoalArea.Height.Value);

            goalPost10Point = new Point(
                this.colLeftEscapeArea.Width.Value +
                this.colLeftPosts.Width.Value +
                this.colLeftGoalArea.Width.Value +
                this.colLeftPenaltyArea.Width.Value +
                this.colHalfWay.Width.Value +
                this.colRightPenaltyArea.Width.Value +
                this.colRightGoalArea.Width.Value,
                goalPost00Point.Y);

            goalPost11Point = new Point(goalPost10Point.X, goalPost10Point.Y + rowTopGoalArea.Height.Value);

            discoids.Add(new GoalPost(new Vector2D(goalPost00Point.X, goalPost00Point.Y), new Vector2D(8, 8), "1001"));
            discoids.Add(new GoalPost(new Vector2D(goalPost01Point.X, goalPost01Point.Y), new Vector2D(8, 8), "1002"));
            discoids.Add(new GoalPost(new Vector2D(goalPost10Point.X, goalPost10Point.Y), new Vector2D(8, 8), "1003"));
            discoids.Add(new GoalPost(new Vector2D(goalPost11Point.X, goalPost11Point.Y), new Vector2D(8, 8), "1004"));

            goals.Add(new Goal(this, 1, new Point(0, goalPost00Point.Y), goalPost00Point, new Point(0, goalPost01Point.Y), goalPost01Point));
            goals.Add(new Goal(this, 2, goalPost10Point, new Point(goalPost10Point.X + colRightGoalArea.Width.Value, goalPost10Point.Y), goalPost11Point, new Point(goalPost11Point.X + colRightGoalArea.Width.Value, goalPost11Point.Y)));

            ball = new Ball(new Vector2D(550, 310), new Vector2D(36, 36), "9");
            ball.X = (goalPost00Point.X + goalPost10Point.X) / 2;
            ball.Y = (rowTopEscapeArea.Height.Value + fieldHeight - rowTopEscapeArea.Height.Value) / 2;

            rootCanvas.Children.Add(bf);

            discoids.Add(ball);

            LoadPlayerFaces();

            ResetPlayerPositions(currentGame.Teams[currentGame.Team1ID], currentGame.Teams[currentGame.Team2ID], rootCanvas, discoids, goalPost00Point.X, goalPost10Point.X, rowTopEscapeArea.Height.Value, fieldHeight - rowTopEscapeArea.Height.Value);

            LayoutRoot.Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri("../Images/Grass.png", UriKind.Relative)) };

            tableBorders.Add(new TableBorder(0, -20, (int)fieldWidth, 20));
            tableBorders.Add(new TableBorder(0, (int)fieldHeight, (int)fieldWidth, 200));
            tableBorders.Add(new TableBorder(-20, 0, 20, (int)fieldHeight));
            tableBorders.Add(new TableBorder((int)fieldWidth, 0, 20, (int)fieldHeight));
            tableBorders.Add(new TableBorder(0, (int)goalPost00Point.Y, (int)colLeftEscapeArea.Width.Value, 10));
            tableBorders.Add(new TableBorder(0, (int)goalPost01Point.Y, (int)colLeftEscapeArea.Width.Value, 10));
            tableBorders.Add(new TableBorder((int)goalPost10Point.X, (int)goalPost10Point.Y, (int)colRightPosts.Width.Value, 10));
            tableBorders.Add(new TableBorder((int)goalPost11Point.X, (int)goalPost11Point.Y, (int)colRightPosts.Width.Value, 10));

            scoreControl.SetValue(Grid.ColumnProperty, 1);
            scoreControl.SetValue(Grid.RowProperty, 0);
            scoreControl.SetValue(Grid.ColumnSpanProperty, 3);
            scoreControl.SetValue(Grid.RowSpanProperty, 1);
            LayoutRoot.Children.Add(scoreControl);

            scoreControl.Team1Name = currentGame.Team1ID;
            scoreControl.Team1Score = currentGame.Scores[currentGame.Team1ID];
            scoreControl.Team2Name = currentGame.Team2ID;
            scoreControl.Team2Score = currentGame.Scores[currentGame.Team2ID];
            scoreControl.PlayingTeamID = currentGame.PlayingTeamID;

            this.Background = new SolidColorBrush(Colors.Black);

            sbStadiumScreen = new Storyboard()
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 10))
            };

            DoubleAnimation translateAnimation = new DoubleAnimation()
            {
                From = 800,
                To = 0,
                Duration = new Duration(new TimeSpan(0, 0, 0, 3))                 
            };

            Storyboard.SetTarget(translateAnimation, lettersXTranslate);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("X"));

            DoubleAnimation lettersOpacityAnimation = new DoubleAnimation()
            {
                From = 0.8,
                To = 1.0,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(lettersOpacityAnimation, grdBrightness);
            Storyboard.SetTargetProperty(lettersOpacityAnimation, new PropertyPath("Opacity"));

            DoubleAnimation screenOpacityAnimation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                BeginTime = new TimeSpan(0, 0, 0, 0),
                Duration = new Duration(new TimeSpan(0, 0, 0, 4))
            };

            sbStadiumScreen.Children.Add(translateAnimation);
            sbStadiumScreen.Children.Add(lettersOpacityAnimation);
            sbStadiumScreen.Children.Add(screenOpacityAnimation);

            Storyboard.SetTarget(screenOpacityAnimation, grdStadiumScreen);
            Storyboard.SetTargetProperty(screenOpacityAnimation, new PropertyPath("Opacity"));

            Storyboard sbBallStrength = new Storyboard()
            {
                RepeatBehavior = RepeatBehavior.Forever
            };

            DoubleAnimation ballStrengthAngle = new DoubleAnimation()
            {
                From = 0.0,
                To = 360,
                Duration = new Duration(new TimeSpan(0, 0, 0, 3, 0)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            sbBallStrength.Children.Add(ballStrengthAngle);
            Storyboard.SetTarget(ballStrengthAngle, rtBallStrength);
            Storyboard.SetTargetProperty(ballStrengthAngle, new PropertyPath("Angle"));
            sbBallStrength.Begin();
        }

        private void LoadPlayerFaces()
        {
            int rootCanvasChildrenCount = rootCanvas.Children.Count();
            for (int i = rootCanvasChildrenCount - 1; i >= 0; i--)
            {
                if (rootCanvas.Children[i] is PlayerFace)
                {
                    PlayerFace pf = rootCanvas.Children[i] as PlayerFace;

                    pf.MouseLeftButtonUp -= LayoutRoot_MouseLeftButtonUp;
                    pf.MouseEnter -= PlayerFace_MouseEnter;
                    pf.MouseLeave -= PlayerFace_MouseLeave;

                    Player p = pf.Player;
                    
                    discoids.Remove(pf.Player);
                    pf.Player = null;
                    playerFaces.Remove(pf);
                    rootCanvas.Children.RemoveAt(i);
                    currentGame.Teams[currentGame.Team1ID].players.Clear();
                    currentGame.Teams[currentGame.Team2ID].players.Clear();
                }
            }

            int[] classicalPlayerPositions = new int[] { 1, 3, 4, 5, 2, 7, 8, 10, 6, 11, 9 };
            for (int i = 0; i <= 10; i++)
            {
                Player p1 = new Player(currentGame.Teams[currentGame.Team1ID], classicalPlayerPositions[i]);
                p1.Position.X = 0;
                p1.Position.Y = 0;
                
                if (i <= 11)
                    p1.IsPlaying = true;

                currentGame.Teams[currentGame.Team1ID].players.Add(p1);

                Player p2 = new Player(currentGame.Teams[currentGame.Team2ID], classicalPlayerPositions[i]);
                p2.Position.X = 0;
                p2.Position.Y = 0;

                if (i <= 11)
                    p2.IsPlaying = true;

                currentGame.Teams[currentGame.Team2ID].players.Add(p2);

                PlayerFace pf1 = new PlayerFace(p1);
                pf1.MouseLeftButtonUp += new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonUp);
                pf1.MouseEnter += new MouseEventHandler(PlayerFace_MouseEnter);
                pf1.MouseLeave += new MouseEventHandler(PlayerFace_MouseLeave);
                playerFaces.Add(pf1);

                PlayerFace pf2 = new PlayerFace(p2);
                pf2.MouseLeftButtonUp += new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonUp);
                pf2.MouseEnter += new MouseEventHandler(PlayerFace_MouseEnter);
                pf2.MouseLeave += new MouseEventHandler(PlayerFace_MouseLeave);
                playerFaces.Add(pf2);

                rootCanvas.Children.Add(pf1);
                rootCanvas.Children.Add(pf2);

                discoids.Add(p1);
                discoids.Add(p2);
            }
        }

        private void LoadBall()
        {
            bf = new BallFace("Jabulani", 06, Colors.White, 0x40, 0x00, 0x00, 0x8D, 0x00, 0x00, 0x8D, 0x00, 0x00, 1.0);
            bf.SetValue(Canvas.LeftProperty, (double)900);
            bf.SetValue(Canvas.TopProperty, (double)120);
            bf.Height = 13;
            bf.Width = 13;
        }

        private Game GetNextGame(string selectedTeamID, DateTime lastGameDate)
        {
            Game result = null;

            var gameQuery = from game in gameTable
                            where
                            (
                                (game.Team1.TeamID == selectedTeamID) ||
                                (game.Team2.TeamID == selectedTeamID)
                            ) &&
                            game.Date > lastGameDate
                            select game;

            result = gameQuery.OrderBy(e => e.Date).First();

            if (result.Team1ID != selectedTeamID)
            {
                result.SwapTeams();
            }            
            
            return result;
        }

        private void LoadGameTable(List<Game> gameTable)
        {
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/11/2010"), teamsDictionary["RSA"], teamsDictionary["MEX"], "Soccer City", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/11/2010"), teamsDictionary["URU"], teamsDictionary["FRA"], "Cape Town Stadium", "Cape Town"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/16/2010"), teamsDictionary["RSA"], teamsDictionary["URU"], "Loftus Versfeld Stadium", "Pretoria "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/17/2010"), teamsDictionary["FRA"], teamsDictionary["MEX"], "Peter Mokaba Stadium", "Polokwane "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/22/2010"), teamsDictionary["MEX"], teamsDictionary["URU"], "Royal Bafokeng Stadium", "RustenBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/22/2010"), teamsDictionary["FRA"], teamsDictionary["RSA"], "Free State Stadium", "Bloemfontein "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/12/2010"), teamsDictionary["KOR"], teamsDictionary["GRE"], "Nelson Mandela Bay Stadium", "Port Elizabeth"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/12/2010"), teamsDictionary["ARG"], teamsDictionary["NGA"], "Ellis Park Stadium", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/17/2010"), teamsDictionary["ARG"], teamsDictionary["KOR"], "Soccer City", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/17/2010"), teamsDictionary["GRE"], teamsDictionary["NGA"], "Free State Stadium", "Bloemfontein "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/22/2010"), teamsDictionary["NGA"], teamsDictionary["KOR"], "Moses Mabhida Stadium", "Durban"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/22/2010"), teamsDictionary["GRE"], teamsDictionary["ARG"], "Peter Mokaba Stadium", "Polokwane "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/12/2010"), teamsDictionary["ENG"], teamsDictionary["USA"], "Royal Bafokeng Stadium", "RustenBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/13/2010"), teamsDictionary["ALG"], teamsDictionary["SVN"], "Peter Mokaba Stadium", "Polokwane "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/18/2010"), teamsDictionary["SVN"], teamsDictionary["USA"], "Ellis Park Stadium", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/18/2010"), teamsDictionary["ENG"], teamsDictionary["ALG"], "Cape Town Stadium", "Cape Town"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/23/2010"), teamsDictionary["SVN"], teamsDictionary["ENG"], "Nelson Mandela Bay Stadium", "Port Elizabeth"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/23/2010"), teamsDictionary["USA"], teamsDictionary["ALG"], "Loftus Versfeld Stadium", "Pretoria "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/13/2010"), teamsDictionary["SRB"], teamsDictionary["GHA"], "Loftus Versfeld Stadium", "Pretoria "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/13/2010"), teamsDictionary["GER"], teamsDictionary["AUS"], "Moses Mabhida Stadium", "Durban"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/18/2010"), teamsDictionary["GER"], teamsDictionary["SRB"], "Nelson Mandela Bay Stadium", "Port Elizabeth"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/19/2010"), teamsDictionary["GHA"], teamsDictionary["AUS"], "Royal Bafokeng Stadium", "RustenBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/23/2010"), teamsDictionary["GHA"], teamsDictionary["GER"], "Soccer City", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/23/2010"), teamsDictionary["AUS"], teamsDictionary["SRB"], "Mbombela Stadium", "Nelspruit"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/14/2010"), teamsDictionary["NED"], teamsDictionary["DEN"], "Soccer City", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/14/2010"), teamsDictionary["JPN"], teamsDictionary["CMR"], "Free State Stadium", "Bloemfontein "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/19/2010"), teamsDictionary["NED"], teamsDictionary["JPN"], "Moses Mabhida Stadium", "Durban"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/19/2010"), teamsDictionary["CMR"], teamsDictionary["DEN"], "Loftus Versfeld Stadium", "Pretoria "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/24/2010"), teamsDictionary["DEN"], teamsDictionary["JPN"], "Royal Bafokeng Stadium", "RustenBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/24/2010"), teamsDictionary["CMR"], teamsDictionary["NED"], "Cape Town Stadium", "Cape Town"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/14/2010"), teamsDictionary["CMR"], teamsDictionary["PAR"], "Cape Town Stadium", "Cape Town"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/15/2010"), teamsDictionary["NZL"], teamsDictionary["SVK"], "Royal Bafokeng Stadium", "RustenBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/20/2010"), teamsDictionary["SVK"], teamsDictionary["PAR"], "Free State Stadium", "Bloemfontein "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/20/2010"), teamsDictionary["CMR"], teamsDictionary["NZL"], "Mbombela Stadium", "Nelspruit"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/24/2010"), teamsDictionary["SVK"], teamsDictionary["CMR"], "Ellis Park Stadium", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/24/2010"), teamsDictionary["PAR"], teamsDictionary["NZL"], "Peter Mokaba Stadium", "Polokwane "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/15/2010"), teamsDictionary["BRA"], teamsDictionary["PRK"], "Ellis Park Stadium", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/15/2010"), teamsDictionary["POR"], teamsDictionary["CIV"], "Soccer City", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/20/2010"), teamsDictionary["BRA"], teamsDictionary["CIV"], "Soccer City", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/25/2010"), teamsDictionary["BRA"], teamsDictionary["POR"], "Nelson Mandela Bay Stadium", "Port Elizabeth"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/21/2010"), teamsDictionary["POR"], teamsDictionary["PRK"], "Cape Town Stadium", "Cape Town"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/25/2010"), teamsDictionary["POR"], teamsDictionary["BRA"], "Moses Mabhida Stadium", "Durban"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/25/2010"), teamsDictionary["PRK"], teamsDictionary["BRA"], "Mbombela Stadium", "Nelspruit"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/16/2010"), teamsDictionary["HON"], teamsDictionary["CHI"], "Mbombela Stadium", "Nelspruit"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/16/2010"), teamsDictionary["ESP"], teamsDictionary["SUI"], "Moses Mabhida Stadium", "Durban"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/21/2010"), teamsDictionary["CHI"], teamsDictionary["SUI"], "Nelson Mandela Bay Stadium", "Port Elizabeth"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/21/2010"), teamsDictionary["ESP"], teamsDictionary["HON"], "Ellis Park Stadium", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/25/2010"), teamsDictionary["CHI"], teamsDictionary["ESP"], "Loftus Versfeld Stadium", "Pretoria "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/25/2010"), teamsDictionary["SUI"], teamsDictionary["HON"], "Free State Stadium", "Bloemfontein "));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/14/2010"), teamsDictionary["ITA"], teamsDictionary["PAR"], "Cape Town Stadium", "Cape Town"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/15/2010"), teamsDictionary["NZL"], teamsDictionary["SVK"], "Royal Bafokeng Stadium", "RustenBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/20/2010"), teamsDictionary["SVK"], teamsDictionary["PAR"], "Free State Stadium", "Bloemfontein"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/24/2010"), teamsDictionary["ITA"], teamsDictionary["NZL"], "Mbombela Stadium", "Nelspruit"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/14/2010"), teamsDictionary["SVK"], teamsDictionary["ITA"], "Ellis Park Stadium", "JohannesBurg"));
            gameTable.Add(new Game("GRP", Convert.ToDateTime("06/14/2010"), teamsDictionary["PAR"], teamsDictionary["NZL"], "Peter Mokaba Stadium", "Polokwane "));
        }

        private static void LoadTeamPlayers(List<TeamPlayer> teamPlayers)
        {
            teamPlayers.Add(new TeamPlayer("ALG", 1, 185015, "Lounes GAOUAOUI"));
            teamPlayers.Add(new TeamPlayer("ARG", 1, 318191, "Diego POZO"));
            teamPlayers.Add(new TeamPlayer("AUS", 1, 94497, "Mark SCHWARZER"));
            teamPlayers.Add(new TeamPlayer("BRA", 1, 179038, "JULIO CESAR"));
            teamPlayers.Add(new TeamPlayer("CAM", 1, 178598, "Idriss KAMENI"));
            teamPlayers.Add(new TeamPlayer("CHI", 1, 202650, "Claudio BRAVO"));
            teamPlayers.Add(new TeamPlayer("CIV", 1, 178958, "Boubacar BARRY"));
            teamPlayers.Add(new TeamPlayer("DEN", 1, 177825, "Thomas SORENSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 1, 62210, "David JAMES"));
            teamPlayers.Add(new TeamPlayer("FRA", 1, 297105, "Hugo LLORIS"));
            teamPlayers.Add(new TeamPlayer("GER", 1, 228912, "Manuel NEUER"));
            teamPlayers.Add(new TeamPlayer("GHA", 1, 306630, "Daniel AGYEI"));
            teamPlayers.Add(new TeamPlayer("GRE", 1, 186516, "Konstantinos CHALKIAS"));
            teamPlayers.Add(new TeamPlayer("HON", 1, 215019, "Ricardo CANALES"));
            teamPlayers.Add(new TeamPlayer("ITA", 1, 159304, "Gianluigi BUFFON"));
            teamPlayers.Add(new TeamPlayer("JAP", 1, 159481, "Seigo NARAZAKI"));
            teamPlayers.Add(new TeamPlayer("PRK", 1, 279084, "RI Myong Guk"));
            teamPlayers.Add(new TeamPlayer("KOR", 1, 77937, "LEE Woon Jae"));
            teamPlayers.Add(new TeamPlayer("MEX", 1, 170808, "Oscar PEREZ"));
            teamPlayers.Add(new TeamPlayer("NED", 1, 184599, "Maarten STEKELENBURG"));
            teamPlayers.Add(new TeamPlayer("NZL", 1, 157045, "Mark PASTON"));
            teamPlayers.Add(new TeamPlayer("NGA", 1, 189300, "Vincent ENYEAMA"));
            teamPlayers.Add(new TeamPlayer("PAR", 1, 185338, "Justo VILLAR"));
            teamPlayers.Add(new TeamPlayer("POR", 1, 299066, "EDUARDO"));
            teamPlayers.Add(new TeamPlayer("SRB", 1, 214386, "Vladimir STOJKOVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 1, 299802, "Jan MUCHA"));
            teamPlayers.Add(new TeamPlayer("SVN", 1, 217488, "Samir HANDANOVIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 1, 214464, "Moneeb JOSEPHS"));
            teamPlayers.Add(new TeamPlayer("ESP", 1, 176644, "Iker CASILLAS"));
            teamPlayers.Add(new TeamPlayer("SUI", 1, 196633, "Diego BENAGLIO"));
            teamPlayers.Add(new TeamPlayer("URU", 1, 229498, "Fernando MUSLERA"));
            teamPlayers.Add(new TeamPlayer("USA", 1, 178420, "Tim HOWARD"));

            teamPlayers.Add(new TeamPlayer("ALG", 2, 215208, "Madjid BOUGHERRA"));
            teamPlayers.Add(new TeamPlayer("ARG", 2, 233703, "Martin DEMICHELIS"));
            teamPlayers.Add(new TeamPlayer("AUS", 2, 178312, "Lucas NEILL"));
            teamPlayers.Add(new TeamPlayer("BRA", 2, 181406, "MAICON"));
            teamPlayers.Add(new TeamPlayer("CAM", 2, 306180, "Benoit ASSOU-EKOTTO"));
            teamPlayers.Add(new TeamPlayer("CHI", 2, 181598, "Ismael FUENTES"));
            teamPlayers.Add(new TeamPlayer("CIV", 2, 291321, "Brou ANGOUA"));
            teamPlayers.Add(new TeamPlayer("DEN", 2, 186373, "Christian POULSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 2, 197464, "Glen JOHNSON"));
            teamPlayers.Add(new TeamPlayer("FRA", 2, 254141, "Bakari SAGNA"));
            teamPlayers.Add(new TeamPlayer("GER", 2, 196791, "Marcell JANSEN"));
            teamPlayers.Add(new TeamPlayer("GHA", 2, 181711, "Hans SARPEI"));
            teamPlayers.Add(new TeamPlayer("GRE", 2, 215517, "Giourkas SEITARIDIS"));
            teamPlayers.Add(new TeamPlayer("HON", 2, 297230, "Osman CHAVEZ"));
            teamPlayers.Add(new TeamPlayer("ITA", 2, 210199, "Christian MAGGIO"));
            teamPlayers.Add(new TeamPlayer("JAP", 2, 188456, "Yuki ABE"));
            teamPlayers.Add(new TeamPlayer("PRK", 2, 210040, "CHA Jong Hyok"));
            teamPlayers.Add(new TeamPlayer("KOR", 2, 198548, "OH Beom Seok"));
            teamPlayers.Add(new TeamPlayer("MEX", 2, 209960, "Francisco RODRIGUEZ"));
            teamPlayers.Add(new TeamPlayer("NED", 2, 291410, "Gregory VAN DER WIEL"));
            teamPlayers.Add(new TeamPlayer("NZL", 2, 195256, "Ben SIGMUND"));
            teamPlayers.Add(new TeamPlayer("NGA", 2, 178403, "Joseph YOBO"));
            teamPlayers.Add(new TeamPlayer("PAR", 2, 185341, "Dario VERON"));
            teamPlayers.Add(new TeamPlayer("POR", 2, 210213, "BRUNO ALVES"));
            teamPlayers.Add(new TeamPlayer("SRB", 2, 291447, "Antonio RUKAVINA"));
            teamPlayers.Add(new TeamPlayer("SVK", 2, 299804, "Peter PEKARIK"));
            teamPlayers.Add(new TeamPlayer("SVN", 2, 217481, "Miso BRECKO"));
            teamPlayers.Add(new TeamPlayer("RSA", 2, 235342, "Siboniso GAXA"));
            teamPlayers.Add(new TeamPlayer("ESP", 2, 216820, "Raul ALBIOL"));
            teamPlayers.Add(new TeamPlayer("SUI", 2, 196605, "Stephan LICHTSTEINER"));
            teamPlayers.Add(new TeamPlayer("URU", 2, 239433, "Diego LUGANO"));
            teamPlayers.Add(new TeamPlayer("USA", 2, 198758, "Jonathan SPECTOR"));

            teamPlayers.Add(new TeamPlayer("ALG", 3, 214771, "Nadir BELHADJ"));
            teamPlayers.Add(new TeamPlayer("ARG", 3, 202429, "Clemente RODRIGUEZ"));
            teamPlayers.Add(new TeamPlayer("AUS", 3, 154061, "Craig MOORE"));
            teamPlayers.Add(new TeamPlayer("BRA", 3, 178520, "LUCIO"));
            teamPlayers.Add(new TeamPlayer("CAM", 3, 290931, "Nicolas NKOULOU"));
            teamPlayers.Add(new TeamPlayer("CHI", 3, 202649, "Waldo PONCE"));
            teamPlayers.Add(new TeamPlayer("CIV", 3, 198489, "Arthur BOKA"));
            teamPlayers.Add(new TeamPlayer("DEN", 3, 309962, "Simon KJAER"));
            teamPlayers.Add(new TeamPlayer("ENG", 3, 182786, "Ashley COLE"));
            teamPlayers.Add(new TeamPlayer("FRA", 3, 217974, "Eric ABIDAL"));
            teamPlayers.Add(new TeamPlayer("GER", 3, 191178, "Arne FRIEDRICH"));
            teamPlayers.Add(new TeamPlayer("GHA", 3, 208353, "Asamoah GYAN"));
            teamPlayers.Add(new TeamPlayer("GRE", 3, 178804, "Christos PATSATZOGLOU"));
            teamPlayers.Add(new TeamPlayer("HON", 3, 213879, "Maynor FIGUEROA"));
            teamPlayers.Add(new TeamPlayer("ITA", 3, 228933, "Domenico CRISCITO"));
            teamPlayers.Add(new TeamPlayer("JAP", 3, 180928, "Yuichi KOMANO"));
            teamPlayers.Add(new TeamPlayer("PRK", 3, 278629, "RI Jun Il"));
            teamPlayers.Add(new TeamPlayer("KOR", 3, 309918, "KIM Hyung Il"));
            teamPlayers.Add(new TeamPlayer("MEX", 3, 218253, "Carlos SALCIDO"));
            teamPlayers.Add(new TeamPlayer("NED", 3, 184607, "John HEITINGA"));
            teamPlayers.Add(new TeamPlayer("NZL", 3, 175397, "Tony LOCHHEAD"));
            teamPlayers.Add(new TeamPlayer("NGA", 3, 214604, "Taye TAIWO"));
            teamPlayers.Add(new TeamPlayer("PAR", 3, 185429, "Claudio MOREL"));
            teamPlayers.Add(new TeamPlayer("POR", 3, 187197, "PAULO FERREIRA"));
            teamPlayers.Add(new TeamPlayer("SRB", 3, 291438, "Aleksandar KOLAROV"));
            teamPlayers.Add(new TeamPlayer("SVK", 3, 197793, "Martin SKRTEL"));
            teamPlayers.Add(new TeamPlayer("SVN", 3, 312573, "Elvedin DZINIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 3, 249474, "Tsepo MASILELA"));
            teamPlayers.Add(new TeamPlayer("ESP", 3, 216973, "Gerard PIQUE"));
            teamPlayers.Add(new TeamPlayer("SUI", 3, 178789, "Ludovic MAGNIN"));
            teamPlayers.Add(new TeamPlayer("URU", 3, 229499, "Diego GODIN"));
            teamPlayers.Add(new TeamPlayer("USA", 3, 187160, "Carlos BOCANEGRA"));

            teamPlayers.Add(new TeamPlayer("ALG", 4, 212808, "Anther YAHIA"));
            teamPlayers.Add(new TeamPlayer("ARG", 4, 181441, "Nicolas BURDISSO"));
            teamPlayers.Add(new TeamPlayer("AUS", 4, 213001, "Tim CAHILL"));
            teamPlayers.Add(new TeamPlayer("BRA", 4, 185324, "JUAN"));
            teamPlayers.Add(new TeamPlayer("CAM", 4, 170657, "Rigobert SONG"));
            teamPlayers.Add(new TeamPlayer("CHI", 4, 228617, "Mauricio ISLA"));
            teamPlayers.Add(new TeamPlayer("CIV", 4, 178963, "Kolo TOURE"));
            teamPlayers.Add(new TeamPlayer("DEN", 4, 227846, "Daniel AGGER"));
            teamPlayers.Add(new TeamPlayer("ENG", 4, 214961, "Steven GERRARD"));
            teamPlayers.Add(new TeamPlayer("FRA", 4, 182230, "Anthony REVEILLERE"));
            teamPlayers.Add(new TeamPlayer("GER", 4, 196889, "Dennis AOGO"));
            teamPlayers.Add(new TeamPlayer("GHA", 4, 184557, "John PANTSIL"));
            teamPlayers.Add(new TeamPlayer("GRE", 4, 228335, "Nikos SPIROPOULOS"));
            teamPlayers.Add(new TeamPlayer("HON", 4, 310376, "Jhony PALACIOS"));
            teamPlayers.Add(new TeamPlayer("ITA", 4, 215274, "Giorgio CHIELLINI"));
            teamPlayers.Add(new TeamPlayer("JAP", 4, 210717, "Marcus Tulio TANAKA"));
            teamPlayers.Add(new TeamPlayer("PRK", 4, 210107, "PAK Nam Chol"));
            teamPlayers.Add(new TeamPlayer("KOR", 4, 237771, "CHO Yong Hyung"));
            teamPlayers.Add(new TeamPlayer("MEX", 4, 178119, "Rafael MARQUEZ"));
            teamPlayers.Add(new TeamPlayer("NED", 4, 228281, "Joris MATHIJSEN"));
            teamPlayers.Add(new TeamPlayer("NZL", 4, 318470, "Winston REID"));
            teamPlayers.Add(new TeamPlayer("NGA", 4, 159416, "Nwankwo KANU"));
            teamPlayers.Add(new TeamPlayer("PAR", 4, 162763, "Denis CANIZA"));
            teamPlayers.Add(new TeamPlayer("POR", 4, 254164, "ROLANDO"));
            teamPlayers.Add(new TeamPlayer("SRB", 4, 291434, "Gojko KACAR"));
            teamPlayers.Add(new TeamPlayer("SVK", 4, 197473, "Marek CECH"));
            teamPlayers.Add(new TeamPlayer("SVN", 4, 299695, "Marko SULER"));
            teamPlayers.Add(new TeamPlayer("RSA", 4, 177007, "Aaron MOKOENA"));
            teamPlayers.Add(new TeamPlayer("ESP", 4, 177846, "Carlos MARCHENA"));
            teamPlayers.Add(new TeamPlayer("SUI", 4, 209989, "Philippe SENDEROS"));
            teamPlayers.Add(new TeamPlayer("URU", 4, 276132, "Jorge FUCILE"));
            teamPlayers.Add(new TeamPlayer("USA", 4, 233531, "Michael BRADLEY"));

            teamPlayers.Add(new TeamPlayer("ALG", 5, 296827, "Rafik HALLICHE"));
            teamPlayers.Add(new TeamPlayer("ARG", 5, 313704, "Mario BOLATTI"));
            teamPlayers.Add(new TeamPlayer("AUS", 5, 178303, "Jason CULINA"));
            teamPlayers.Add(new TeamPlayer("BRA", 5, 194818, "FELIPE MELO"));
            teamPlayers.Add(new TeamPlayer("CAM", 5, 296813, "Sebastien BASSONG"));
            teamPlayers.Add(new TeamPlayer("CHI", 5, 3324, "Pablo CONTRERAS"));
            teamPlayers.Add(new TeamPlayer("CIV", 5, 176971, "Didier ZOKORA"));
            teamPlayers.Add(new TeamPlayer("DEN", 5, 299421, "William KVIST"));
            teamPlayers.Add(new TeamPlayer("ENG", 5, 197603, "Michael DAWSON"));
            teamPlayers.Add(new TeamPlayer("FRA", 5, 187756, "William GALLAS"));
            teamPlayers.Add(new TeamPlayer("GER", 5, 297107, "Serdar TASCI"));
            teamPlayers.Add(new TeamPlayer("GHA", 5, 184560, "John MENSAH"));
            teamPlayers.Add(new TeamPlayer("GRE", 5, 215471, "Vangelis MORAS"));
            teamPlayers.Add(new TeamPlayer("HON", 5, 213878, "Victor BERNARDEZ"));
            teamPlayers.Add(new TeamPlayer("ITA", 5, 155957, "Fabio CANNAVARO"));
            teamPlayers.Add(new TeamPlayer("JAP", 5, 291372, "Yuto NAGATOMO"));
            teamPlayers.Add(new TeamPlayer("PRK", 5, 210103, "RI Kwang Chon"));
            teamPlayers.Add(new TeamPlayer("KOR", 5, 188386, "KIM Nam Il"));
            teamPlayers.Add(new TeamPlayer("MEX", 5, 213995, "Ricardo OSORIO"));
            teamPlayers.Add(new TeamPlayer("NED", 5, 155679, "Giovanni VAN BRONCKHORST"));
            teamPlayers.Add(new TeamPlayer("NZL", 5, 157050, "Ivan VICELICH"));
            teamPlayers.Add(new TeamPlayer("NGA", 5, 178372, "Rabiu AFOLABI"));
            teamPlayers.Add(new TeamPlayer("PAR", 5, 189334, "Julio Cesar CACERES"));
            teamPlayers.Add(new TeamPlayer("POR", 5, 187174, "DUDA"));
            teamPlayers.Add(new TeamPlayer("SRB", 5, 298699, "Nemanja VIDIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 5, 215716, "Radoslav ZABAVNIK"));
            teamPlayers.Add(new TeamPlayer("SVN", 5, 187530, "Bostjan CESAR"));
            teamPlayers.Add(new TeamPlayer("RSA", 5, 313869, "Anele NGCONGCA"));
            teamPlayers.Add(new TeamPlayer("ESP", 5, 177914, "Carles PUYOL"));
            teamPlayers.Add(new TeamPlayer("SUI", 5, 196620, "Steve VON BERGEN"));
            teamPlayers.Add(new TeamPlayer("URU", 5, 276131, "Walter GARGANO"));
            teamPlayers.Add(new TeamPlayer("USA", 5, 175511, "Oguchi ONYEWU"));

            teamPlayers.Add(new TeamPlayer("ALG", 6, 205709, "Yazid MANSOURI"));
            teamPlayers.Add(new TeamPlayer("ARG", 6, 201964, "Gabriel HEINZE"));
            teamPlayers.Add(new TeamPlayer("AUS", 6, 210003, "Michael BEAUCHAMP"));
            teamPlayers.Add(new TeamPlayer("BRA", 6, 318171, "MICHEL BASTOS"));
            teamPlayers.Add(new TeamPlayer("CAM", 6, 200261, "Alexandre SONG"));
            teamPlayers.Add(new TeamPlayer("CHI", 6, 229436, "Carlos CARMONA"));
            teamPlayers.Add(new TeamPlayer("CIV", 6, 294608, "Steve GOHOURI"));
            teamPlayers.Add(new TeamPlayer("DEN", 6, 186351, "Lars JACOBSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 6, 214962, "John TERRY"));
            teamPlayers.Add(new TeamPlayer("FRA", 6, 323699, "Marc PLANUS"));
            teamPlayers.Add(new TeamPlayer("GER", 6, 196900, "Sami KHEDIRA"));
            teamPlayers.Add(new TeamPlayer("GHA", 6, 294617, "Anthony ANNAN"));
            teamPlayers.Add(new TeamPlayer("GRE", 6, 228351, "Alexandros TZIOLIS"));
            teamPlayers.Add(new TeamPlayer("HON", 6, 207995, "Hendry THOMAS"));
            teamPlayers.Add(new TeamPlayer("ITA", 6, 209119, "Daniele DE ROSSI"));
            teamPlayers.Add(new TeamPlayer("JAP", 6, 268474, "Atsuto UCHIDA"));
            teamPlayers.Add(new TeamPlayer("PRK", 6, 230744, "KIM Kum Il"));
            teamPlayers.Add(new TeamPlayer("KOR", 6, 291389, "KIM Bo Kyung"));
            teamPlayers.Add(new TeamPlayer("MEX", 6, 70097, "Gerardo TORRADO"));
            teamPlayers.Add(new TeamPlayer("NED", 6, 179568, "Mark VAN BOMMEL"));
            teamPlayers.Add(new TeamPlayer("NZL", 6, 198160, "Ryan NELSEN"));
            teamPlayers.Add(new TeamPlayer("NGA", 6, 235412, "Danny SHITTU"));
            teamPlayers.Add(new TeamPlayer("PAR", 6, 189436, "Carlos BONET"));
            teamPlayers.Add(new TeamPlayer("POR", 6, 214957, "RICARDO CARVALHO"));
            teamPlayers.Add(new TeamPlayer("SRB", 6, 214388, "Branislav IVANOVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 6, 178464, "Zdenko STRBA"));
            teamPlayers.Add(new TeamPlayer("SVN", 6, 217485, "Branko ILIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 6, 182181, "MacBeth SIBAYA"));
            teamPlayers.Add(new TeamPlayer("ESP", 6, 183857, "Andres INIESTA"));
            teamPlayers.Add(new TeamPlayer("SUI", 6, 214477, "Benjamin HUGGEL"));
            teamPlayers.Add(new TeamPlayer("URU", 6, 286484, "Mauricio VICTORINO"));
            teamPlayers.Add(new TeamPlayer("USA", 6, 182797, "Steve CHERUNDOLO"));

            teamPlayers.Add(new TeamPlayer("ALG", 7, 323344, "Ryad BOUDEBOUZ"));
            teamPlayers.Add(new TeamPlayer("ARG", 7, 266800, "Angel DI MARIA"));
            teamPlayers.Add(new TeamPlayer("AUS", 7, 178306, "Brett EMERTON"));
            teamPlayers.Add(new TeamPlayer("BRA", 7, 202076, "ELANO"));
            teamPlayers.Add(new TeamPlayer("CAM", 7, 200262, "Landry NGUEMO"));
            teamPlayers.Add(new TeamPlayer("CHI", 7, 228627, "Alexis SANCHEZ"));
            teamPlayers.Add(new TeamPlayer("CIV", 7, 317093, "Seydou DOUMBIA"));
            teamPlayers.Add(new TeamPlayer("DEN", 7, 186353, "Daniel JENSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 7, 252997, "Aaron LENNON"));
            teamPlayers.Add(new TeamPlayer("FRA", 7, 248373, "Franck RIBERY"));
            teamPlayers.Add(new TeamPlayer("GER", 7, 196752, "Bastian SCHWEINSTEIGER"));
            teamPlayers.Add(new TeamPlayer("GHA", 7, 238381, "Samuel INKOOM"));
            teamPlayers.Add(new TeamPlayer("GRE", 7, 297112, "Georgios SAMARAS"));
            teamPlayers.Add(new TeamPlayer("HON", 7, 213876, "Ramon NUNEZ"));
            teamPlayers.Add(new TeamPlayer("ITA", 7, 254181, "Simone PEPE"));
            teamPlayers.Add(new TeamPlayer("JAP", 7, 177768, "Yasuhito ENDO"));
            teamPlayers.Add(new TeamPlayer("PRK", 7, 210043, "AN Chol Hyok"));
            teamPlayers.Add(new TeamPlayer("KOR", 7, 177788, "PARK Ji Sung"));
            teamPlayers.Add(new TeamPlayer("MEX", 7, 271393, "Pablo BARRERA"));
            teamPlayers.Add(new TeamPlayer("NED", 7, 217313, "Dirk KUYT"));
            teamPlayers.Add(new TeamPlayer("NZL", 7, 184726, "Simon ELLIOTT"));
            teamPlayers.Add(new TeamPlayer("NGA", 7, 183498, "John UTAKA"));
            teamPlayers.Add(new TeamPlayer("PAR", 7, 276119, "Oscar CARDOZO"));
            teamPlayers.Add(new TeamPlayer("POR", 7, 201200, "CRISTIANO RONALDO"));
            teamPlayers.Add(new TeamPlayer("SRB", 7, 291452, "Zoran TOSIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 7, 313909, "Vladimir WEISS"));
            teamPlayers.Add(new TeamPlayer("SVN", 7, 306106, "Nejc PECNIK"));
            teamPlayers.Add(new TeamPlayer("RSA", 7, 214470, "Lance DAVIDS"));
            teamPlayers.Add(new TeamPlayer("ESP", 7, 229884, "David VILLA"));
            teamPlayers.Add(new TeamPlayer("SUI", 7, 209979, "Tranquillo BARNETTA"));
            teamPlayers.Add(new TeamPlayer("URU", 7, 267834, "Edinson CAVANI"));
            teamPlayers.Add(new TeamPlayer("USA", 7, 175498, "DaMarcus BEASLEY"));

            teamPlayers.Add(new TeamPlayer("ALG", 8, 321682, "Medhi LACEN"));
            teamPlayers.Add(new TeamPlayer("ARG", 8, 153978, "Juan VERON"));
            teamPlayers.Add(new TeamPlayer("AUS", 8, 181474, "Luke WILKSHIRE"));
            teamPlayers.Add(new TeamPlayer("BRA", 8, 187267, "GILBERTO SILVA"));
            teamPlayers.Add(new TeamPlayer("CAM", 8, 176652, "GEREMI"));
            teamPlayers.Add(new TeamPlayer("CHI", 8, 267543, "Arturo VIDAL"));
            teamPlayers.Add(new TeamPlayer("CIV", 8, 216970, "Salomon KALOU"));
            teamPlayers.Add(new TeamPlayer("DEN", 8, 169682, "Jesper GRONKJAER"));
            teamPlayers.Add(new TeamPlayer("ENG", 8, 185385, "Frank LAMPARD"));
            teamPlayers.Add(new TeamPlayer("FRA", 8, 254144, "Yoann GOURCUFF"));
            teamPlayers.Add(new TeamPlayer("GER", 8, 305036, "Mesut OEZIL"));
            teamPlayers.Add(new TeamPlayer("GHA", 8, 306634, "Jonathan MENSAH"));
            teamPlayers.Add(new TeamPlayer("GRE", 8, 299087, "Avraam PAPADOPOULOS"));
            teamPlayers.Add(new TeamPlayer("HON", 8, 209827, "Wilson PALACIOS"));
            teamPlayers.Add(new TeamPlayer("ITA", 8, 177845, "Gennaro GATTUSO"));
            teamPlayers.Add(new TeamPlayer("JAP", 8, 198751, "Daisuke MATSUI"));
            teamPlayers.Add(new TeamPlayer("PRK", 8, 210106, "JI Yun Nam"));
            teamPlayers.Add(new TeamPlayer("KOR", 8, 209904, "KIM Jung Woo"));
            teamPlayers.Add(new TeamPlayer("MEX", 8, 227849, "Israel CASTRO"));
            teamPlayers.Add(new TeamPlayer("NED", 8, 216964, "Nigel DE JONG"));
            teamPlayers.Add(new TeamPlayer("NZL", 8, 199701, "Tim BROWN"));
            teamPlayers.Add(new TeamPlayer("NGA", 8, 3989, "Yakubu AYEGBENI"));
            teamPlayers.Add(new TeamPlayer("PAR", 8, 182624, "Edgar BARRETO"));
            teamPlayers.Add(new TeamPlayer("POR", 8, 187198, "PEDRO MENDES"));
            teamPlayers.Add(new TeamPlayer("SRB", 8, 210166, "Danko LAZOVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 8, 237767, "Jan KOZAK"));
            teamPlayers.Add(new TeamPlayer("SVN", 8, 187539, "Robert KOREN"));
            teamPlayers.Add(new TeamPlayer("RSA", 8, 249477, "Siphiwe TSHABALALA"));
            teamPlayers.Add(new TeamPlayer("ESP", 8, 177855, "XAVI"));
            teamPlayers.Add(new TeamPlayer("SUI", 8, 261029, "Gokhan INLER"));
            teamPlayers.Add(new TeamPlayer("URU", 8, 185369, "Sebastian EGUREN"));
            teamPlayers.Add(new TeamPlayer("USA", 8, 207851, "Clint DEMPSEY"));

            teamPlayers.Add(new TeamPlayer("ALG", 9, 305702, "Abdelkader GHEZZAL"));
            teamPlayers.Add(new TeamPlayer("ARG", 9, 271550, "Gonzalo HIGUAIN"));
            teamPlayers.Add(new TeamPlayer("AUS", 9, 252195, "Joshua KENNEDY"));
            teamPlayers.Add(new TeamPlayer("BRA", 9, 200158, "LUIS FABIANO"));
            teamPlayers.Add(new TeamPlayer("CAM", 9, 170667, "Samuel ETOO"));
            teamPlayers.Add(new TeamPlayer("CHI", 9, 209501, "Humberto SUAZO"));
            teamPlayers.Add(new TeamPlayer("CIV", 9, 291329, "Ismael TIOTE"));
            teamPlayers.Add(new TeamPlayer("DEN", 9, 154949, "Jon Dahl TOMASSON"));
            teamPlayers.Add(new TeamPlayer("ENG", 9, 194176, "Peter CROUCH"));
            teamPlayers.Add(new TeamPlayer("FRA", 9, 180749, "Djibril CISSE"));
            teamPlayers.Add(new TeamPlayer("GER", 9, 228304, "Stefan KIESSLING"));
            teamPlayers.Add(new TeamPlayer("GHA", 9, 183008, "Derek BOATENG"));
            teamPlayers.Add(new TeamPlayer("GRE", 9, 182768, "Angelos CHARISTEAS"));
            teamPlayers.Add(new TeamPlayer("HON", 9, 155651, "Carlos PAVON"));
            teamPlayers.Add(new TeamPlayer("ITA", 9, 234927, "Vincenzo IAQUINTA"));
            teamPlayers.Add(new TeamPlayer("JAP", 9, 286278, "Shinji OKAZAKI"));
            teamPlayers.Add(new TeamPlayer("PRK", 9, 282255, "JONG Tae Se"));
            teamPlayers.Add(new TeamPlayer("KOR", 9, 156216, "AHN Jung Hwan"));
            teamPlayers.Add(new TeamPlayer("MEX", 9, 241632, "Guillermo FRANCO"));
            teamPlayers.Add(new TeamPlayer("NED", 9, 217315, "Robin VAN PERSIE"));
            teamPlayers.Add(new TeamPlayer("NZL", 9, 198161, "Shane SMELTZ"));
            teamPlayers.Add(new TeamPlayer("NGA", 9, 199817, "Obafemi MARTINS"));
            teamPlayers.Add(new TeamPlayer("PAR", 9, 4039, "Roque SANTA CRUZ"));
            teamPlayers.Add(new TeamPlayer("POR", 9, 313541, "LIEDSON"));
            teamPlayers.Add(new TeamPlayer("SRB", 9, 217019, "Marko PANTELIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 9, 215725, "Stanislav SESTAK"));
            teamPlayers.Add(new TeamPlayer("SVN", 9, 299706, "Zlatan LJUBIJANKIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 9, 230776, "Katlego MPHELA"));
            teamPlayers.Add(new TeamPlayer("ESP", 9, 183864, "Fernando TORRES"));
            teamPlayers.Add(new TeamPlayer("SUI", 9, 182647, "Alexander FREI"));
            teamPlayers.Add(new TeamPlayer("URU", 9, 270775, "Luis SUAREZ"));
            teamPlayers.Add(new TeamPlayer("USA", 9, 276138, "Herculez GOMEZ"));

            teamPlayers.Add(new TeamPlayer("ALG", 10, 177512, "Rafik SAIFI"));
            teamPlayers.Add(new TeamPlayer("ARG", 10, 229397, "Lionel MESSI"));
            teamPlayers.Add(new TeamPlayer("AUS", 10, 159069, "Harry KEWELL"));
            teamPlayers.Add(new TeamPlayer("BRA", 10, 184312, "KAKA"));
            teamPlayers.Add(new TeamPlayer("CAM", 10, 198293, "Achille EMANA"));
            teamPlayers.Add(new TeamPlayer("CHI", 10, 202655, "Jorge VALDIVIA"));
            teamPlayers.Add(new TeamPlayer("CIV", 10, 290186, "GERVINHO"));
            teamPlayers.Add(new TeamPlayer("DEN", 10, 168837, "Martin JORGENSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 10, 196842, "Wayne ROONEY"));
            teamPlayers.Add(new TeamPlayer("FRA", 10, 182236, "Sidney GOVOU"));
            teamPlayers.Add(new TeamPlayer("GER", 10, 196789, "Lukas PODOLSKI"));
            teamPlayers.Add(new TeamPlayer("GHA", 10, 155382, "Stephen APPIAH"));
            teamPlayers.Add(new TeamPlayer("GRE", 10, 168832, "Georgios KARAGOUNIS"));
            teamPlayers.Add(new TeamPlayer("HON", 10, 213953, "Jerry PALACIOS"));
            teamPlayers.Add(new TeamPlayer("ITA", 10, 196699, "Antonio DI NATALE"));
            teamPlayers.Add(new TeamPlayer("JAP", 10, 177773, "Shunsuke NAKAMURA"));
            teamPlayers.Add(new TeamPlayer("PRK", 10, 210032, "HONG Yong Jo"));
            teamPlayers.Add(new TeamPlayer("KOR", 10, 207761, "PARK Chu Young"));
            teamPlayers.Add(new TeamPlayer("MEX", 10, 162580, "Cuauhtemoc BLANCO"));
            teamPlayers.Add(new TeamPlayer("NED", 10, 215002, "Wesley SNEIJDER"));
            teamPlayers.Add(new TeamPlayer("NZL", 10, 184851, "Chris KILLEN"));
            teamPlayers.Add(new TeamPlayer("NGA", 10, 273285, "Brown IDEYE"));
            teamPlayers.Add(new TeamPlayer("PAR", 10, 302599, "Edgar BENITEZ"));
            teamPlayers.Add(new TeamPlayer("POR", 10, 210229, "DANNY"));
            teamPlayers.Add(new TeamPlayer("SRB", 10, 170750, "Dejan STANKOVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 10, 241640, "Marek SAPARA"));
            teamPlayers.Add(new TeamPlayer("SVN", 10, 299703, "Valter BIRSA"));
            teamPlayers.Add(new TeamPlayer("RSA", 10, 189676, "Steven PIENAAR"));
            teamPlayers.Add(new TeamPlayer("ESP", 10, 200179, "Cesc FABREGAS"));
            teamPlayers.Add(new TeamPlayer("SUI", 10, 179033, "Blaise NKUFO"));
            teamPlayers.Add(new TeamPlayer("URU", 10, 189259, "Diego FORLAN"));
            teamPlayers.Add(new TeamPlayer("USA", 10, 175507, "Landon DONOVAN"));

            teamPlayers.Add(new TeamPlayer("ALG", 11, 296836, "Rafik DJEBBOUR"));
            teamPlayers.Add(new TeamPlayer("ARG", 11, 182373, "Carlos TEVEZ"));
            teamPlayers.Add(new TeamPlayer("AUS", 11, 182002, "Scott CHIPPERFIELD"));
            teamPlayers.Add(new TeamPlayer("BRA", 11, 194815, "ROBINHO"));
            teamPlayers.Add(new TeamPlayer("CAM", 11, 199600, "Jean MAKOUN"));
            teamPlayers.Add(new TeamPlayer("CHI", 11, 201785, "Mark GONZALEZ"));
            teamPlayers.Add(new TeamPlayer("CIV", 11, 212306, "Didier DROGBA"));
            teamPlayers.Add(new TeamPlayer("DEN", 11, 254120, "Nicklas BENDTNER"));
            teamPlayers.Add(new TeamPlayer("ENG", 11, 179662, "Joe COLE"));
            teamPlayers.Add(new TeamPlayer("FRA", 11, 306322, "Andre Pierre GIGNAC"));
            teamPlayers.Add(new TeamPlayer("GER", 11, 182206, "Miroslav KLOSE"));
            teamPlayers.Add(new TeamPlayer("GHA", 11, 183000, "Sulley MUNTARI"));
            teamPlayers.Add(new TeamPlayer("GRE", 11, 215484, "Loukas VYNTRA"));
            teamPlayers.Add(new TeamPlayer("HON", 11, 177987, "David SUAZO"));
            teamPlayers.Add(new TeamPlayer("ITA", 11, 210205, "Alberto GILARDINO"));
            teamPlayers.Add(new TeamPlayer("JAP", 11, 211976, "Keiji TAMADA"));
            teamPlayers.Add(new TeamPlayer("PRK", 11, 212369, "MUN In Guk"));
            teamPlayers.Add(new TeamPlayer("KOR", 11, 291402, "LEE Seung Yeoul"));
            teamPlayers.Add(new TeamPlayer("MEX", 11, 234552, "Carlos VELA"));
            teamPlayers.Add(new TeamPlayer("NED", 11, 184616, "Arjen ROBBEN"));
            teamPlayers.Add(new TeamPlayer("NZL", 11, 199704, "Leo BERTOS"));
            teamPlayers.Add(new TeamPlayer("NGA", 11, 214228, "Peter ODEMWINGIE"));
            teamPlayers.Add(new TeamPlayer("PAR", 11, 276118, "Jonathan SANTANA"));
            teamPlayers.Add(new TeamPlayer("POR", 11, 179159, "SIMAO"));
            teamPlayers.Add(new TeamPlayer("SRB", 11, 210148, "Nenad MILIJAS"));
            teamPlayers.Add(new TeamPlayer("SVK", 11, 184410, "Robert VITTEK"));
            teamPlayers.Add(new TeamPlayer("SVN", 11, 299701, "Milivoje NOVAKOVIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 11, 269987, "Teko MODISE"));
            teamPlayers.Add(new TeamPlayer("ESP", 11, 177907, "Joan CAPDEVILA"));
            teamPlayers.Add(new TeamPlayer("SUI", 11, 216627, "Valon BEHRAMI"));
            teamPlayers.Add(new TeamPlayer("URU", 11, 229502, "Alvaro PEREIRA"));
            teamPlayers.Add(new TeamPlayer("USA", 11, 278536, "Stuart HOLDEN"));

            teamPlayers.Add(new TeamPlayer("ALG", 12, 323337, "Habib BELLAID"));
            teamPlayers.Add(new TeamPlayer("ARG", 12, 202427, "Ariel GARCE"));
            teamPlayers.Add(new TeamPlayer("AUS", 12, 229021, "Adam FEDERICI"));
            teamPlayers.Add(new TeamPlayer("BRA", 12, 200718, "GOMES"));
            teamPlayers.Add(new TeamPlayer("CAM", 12, 319344, "Gaetan BONG"));
            teamPlayers.Add(new TeamPlayer("CHI", 12, 281253, "Miguel PINTO"));
            teamPlayers.Add(new TeamPlayer("CIV", 12, 198112, "Jean Jacques GOSSO"));
            teamPlayers.Add(new TeamPlayer("DEN", 12, 214947, "Thomas KAHLENBERG"));
            teamPlayers.Add(new TeamPlayer("ENG", 12, 217336, "Robert GREEN"));
            teamPlayers.Add(new TeamPlayer("FRA", 12, 170711, "Thierry HENRY"));
            teamPlayers.Add(new TeamPlayer("GER", 12, 210010, "Tim WIESE"));
            teamPlayers.Add(new TeamPlayer("GHA", 12, 249462, "Prince TAGOE"));
            teamPlayers.Add(new TeamPlayer("GRE", 12, 209121, "Alexandros TZORVAS"));
            teamPlayers.Add(new TeamPlayer("HON", 12, 295931, "Georgie WELCOME"));
            teamPlayers.Add(new TeamPlayer("ITA", 12, 313831, "Federico MARCHETTI"));
            teamPlayers.Add(new TeamPlayer("JAP", 12, 185525, "Kisho YANO"));
            teamPlayers.Add(new TeamPlayer("PRK", 12, 282237, "CHOE Kum Chol"));
            teamPlayers.Add(new TeamPlayer("KOR", 12, 177785, "LEE Young Pyo"));
            teamPlayers.Add(new TeamPlayer("MEX", 12, 289195, "Paul AGUILAR"));
            teamPlayers.Add(new TeamPlayer("NED", 12, 218246, "Khalid BOULAHROUZ"));
            teamPlayers.Add(new TeamPlayer("NZL", 12, 198176, "Glen MOSS"));
            teamPlayers.Add(new TeamPlayer("NGA", 12, 178738, "Kalu UCHE"));
            teamPlayers.Add(new TeamPlayer("PAR", 12, 181732, "Diego BARRETO"));
            teamPlayers.Add(new TeamPlayer("POR", 12, 214404, "BETO"));
            teamPlayers.Add(new TeamPlayer("SRB", 12, 302533, "Bojan ISAILOVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 12, 197787, "Dusan PERNIS"));
            teamPlayers.Add(new TeamPlayer("SVN", 12, 299692, "Jasmin HANDANOVIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 12, 306029, "Reneilwe LETSHOLONYANE"));
            teamPlayers.Add(new TeamPlayer("ESP", 12, 217839, "Victor VALDES"));
            teamPlayers.Add(new TeamPlayer("SUI", 12, 209972, "Marco WOELFLI"));
            teamPlayers.Add(new TeamPlayer("URU", 12, 276134, "Juan CASTILLO"));
            teamPlayers.Add(new TeamPlayer("USA", 12, 271212, "Jonathan BORNSTEIN"));

            teamPlayers.Add(new TeamPlayer("ALG", 13, 296826, "Karim MATMOUR"));
            teamPlayers.Add(new TeamPlayer("ARG", 13, 3076, "Walter SAMUEL"));
            teamPlayers.Add(new TeamPlayer("AUS", 13, 178324, "Vince GRELLA"));
            teamPlayers.Add(new TeamPlayer("BRA", 13, 190962, "DANI ALVES"));
            teamPlayers.Add(new TeamPlayer("CAM", 13, 312316, "Eric CHOUPO MOTING"));
            teamPlayers.Add(new TeamPlayer("CHI", 13, 281256, "Marco ESTRADA"));
            teamPlayers.Add(new TeamPlayer("CIV", 13, 198111, "ROMARIC"));
            teamPlayers.Add(new TeamPlayer("DEN", 13, 214948, "Per KROLDRUP"));
            teamPlayers.Add(new TeamPlayer("ENG", 13, 238634, "Stephen WARNOCK"));
            teamPlayers.Add(new TeamPlayer("FRA", 13, 212246, "Patrice EVRA"));
            teamPlayers.Add(new TeamPlayer("GER", 13, 321722, "Thomas MUELLER"));
            teamPlayers.Add(new TeamPlayer("GHA", 13, 294620, "Andre AYEW"));
            teamPlayers.Add(new TeamPlayer("GRE", 13, 215476, "Michail SIFAKIS"));
            teamPlayers.Add(new TeamPlayer("HON", 13, 304934, "Roger ESPINOZA"));
            teamPlayers.Add(new TeamPlayer("ITA", 13, 298656, "Salvatore BOCCHETTI"));
            teamPlayers.Add(new TeamPlayer("JAP", 13, 295190, "Daiki IWAMASA"));
            teamPlayers.Add(new TeamPlayer("PRK", 13, 210023, "PAK Chol Jin"));
            teamPlayers.Add(new TeamPlayer("KOR", 13, 318861, "KIM Jae Sung"));
            teamPlayers.Add(new TeamPlayer("MEX", 13, 215285, "Guillermo OCHOA"));
            teamPlayers.Add(new TeamPlayer("NED", 13, 155673, "Andre OOIJER"));
            teamPlayers.Add(new TeamPlayer("NZL", 13, 289579, "Andy BARRON"));
            teamPlayers.Add(new TeamPlayer("NGA", 13, 238748, "Yussuf AYILA"));
            teamPlayers.Add(new TeamPlayer("PAR", 13, 276117, "Enrique VERA"));
            teamPlayers.Add(new TeamPlayer("POR", 13, 187204, "MIGUEL"));
            teamPlayers.Add(new TeamPlayer("SRB", 13, 243852, "Aleksandar LUKOVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 13, 197801, "Filip HOLOSKO"));
            teamPlayers.Add(new TeamPlayer("SVN", 13, 299693, "Bojan JOKIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 13, 269986, "Kagisho DIKGACOI"));
            teamPlayers.Add(new TeamPlayer("ESP", 13, 267811, "Juan Manuel MATA"));
            teamPlayers.Add(new TeamPlayer("SUI", 13, 214474, "Stephane GRICHTING"));
            teamPlayers.Add(new TeamPlayer("URU", 13, 158398, "Sebastian ABREU"));
            teamPlayers.Add(new TeamPlayer("USA", 13, 197408, "Ricardo CLARK"));

            teamPlayers.Add(new TeamPlayer("ALG", 14, 244870, "Abdelkader LAIFAOUI"));
            teamPlayers.Add(new TeamPlayer("ARG", 14, 182372, "Javier MASCHERANO"));
            teamPlayers.Add(new TeamPlayer("AUS", 14, 182606, "Brett HOLMAN"));
            teamPlayers.Add(new TeamPlayer("BRA", 14, 184316, "LUISAO"));
            teamPlayers.Add(new TeamPlayer("CAM", 14, 278732, "Aurelien CHEDJOU"));
            teamPlayers.Add(new TeamPlayer("CHI", 14, 229435, "Matias FERNANDEZ"));
            teamPlayers.Add(new TeamPlayer("CIV", 14, 240681, "Emmanuel KONE"));
            teamPlayers.Add(new TeamPlayer("DEN", 14, 254112, "Jakob POULSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 14, 177648, "Gareth BARRY"));
            teamPlayers.Add(new TeamPlayer("FRA", 14, 254145, "Jeremy TOULALAN"));
            teamPlayers.Add(new TeamPlayer("GER", 14, 323324, "Holger BADSTUBER"));
            teamPlayers.Add(new TeamPlayer("GHA", 14, 184564, "Matthew AMOAH"));
            teamPlayers.Add(new TeamPlayer("GRE", 14, 215513, "Dimitrios SALPINGIDIS"));
            teamPlayers.Add(new TeamPlayer("HON", 14, 237071, "Oscar GARCIA"));
            teamPlayers.Add(new TeamPlayer("ITA", 14, 177833, "Morgan DE SANCTIS"));
            teamPlayers.Add(new TeamPlayer("JAP", 14, 278116, "Kengo NAKAMURA"));
            teamPlayers.Add(new TeamPlayer("PRK", 14, 238414, "PAK Nam Chol"));
            teamPlayers.Add(new TeamPlayer("KOR", 14, 296013, "LEE Jung Soo"));
            teamPlayers.Add(new TeamPlayer("MEX", 14, 228599, "Javier HERNANDEZ"));
            teamPlayers.Add(new TeamPlayer("NED", 14, 254097, "Demy DE ZEEUW"));
            teamPlayers.Add(new TeamPlayer("NZL", 14, 313808, "Rory FALLON"));
            teamPlayers.Add(new TeamPlayer("NGA", 14, 233751, "Sani KAITA"));
            teamPlayers.Add(new TeamPlayer("PAR", 14, 178278, "Paulo DA SILVA"));
            teamPlayers.Add(new TeamPlayer("POR", 14, 200201, "MIGUEL VELOSO"));
            teamPlayers.Add(new TeamPlayer("SRB", 14, 298885, "Milan JOVANOVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 14, 230496, "Martin JAKUBKO"));
            teamPlayers.Add(new TeamPlayer("SVN", 14, 217482, "Zlatko DEDIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 14, 176979, "Matthew BOOTH"));
            teamPlayers.Add(new TeamPlayer("ESP", 14, 207528, "XABI ALONSO"));
            teamPlayers.Add(new TeamPlayer("SUI", 14, 305016, "Marco PADALINO"));
            teamPlayers.Add(new TeamPlayer("URU", 14, 305378, "Nicolas LODEIRO"));
            teamPlayers.Add(new TeamPlayer("USA", 14, 181949, "Edson BUDDLE"));

            teamPlayers.Add(new TeamPlayer("ALG", 15, 205717, "Karim ZIANI"));
            teamPlayers.Add(new TeamPlayer("ARG", 15, 310116, "Nicolas OTAMENDI"));
            teamPlayers.Add(new TeamPlayer("AUS", 15, 197514, "Mile JEDINAK"));
            teamPlayers.Add(new TeamPlayer("BRA", 15, 289964, "THIAGO SILVA"));
            teamPlayers.Add(new TeamPlayer("CAM", 15, 214135, "Pierre WEBO"));
            teamPlayers.Add(new TeamPlayer("CHI", 15, 209502, "Jean BEAUSEJOUR"));
            teamPlayers.Add(new TeamPlayer("CIV", 15, 176984, "Aruna DINDANE"));
            teamPlayers.Add(new TeamPlayer("DEN", 15, 299431, "Simon POULSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 15, 195763, "Matt UPSON"));
            teamPlayers.Add(new TeamPlayer("FRA", 15, 225204, "Florent MALOUDA"));
            teamPlayers.Add(new TeamPlayer("GER", 15, 196750, "Piotr TROCHOWSKI"));
            teamPlayers.Add(new TeamPlayer("GHA", 15, 279145, "Isaac VORSAH"));
            teamPlayers.Add(new TeamPlayer("GRE", 15, 228336, "Vasileios TOROSIDIS"));
            teamPlayers.Add(new TeamPlayer("HON", 15, 213945, "Walter MARTINEZ"));
            teamPlayers.Add(new TeamPlayer("ITA", 15, 298664, "Claudio MARCHISIO"));
            teamPlayers.Add(new TeamPlayer("JAP", 15, 184373, "Yasuyuki KONNO"));
            teamPlayers.Add(new TeamPlayer("PRK", 15, 210007, "KIM Yong Jun"));
            teamPlayers.Add(new TeamPlayer("KOR", 15, 209900, "KIM Dong Jin"));
            teamPlayers.Add(new TeamPlayer("MEX", 15, 238112, "Hector MORENO"));
            teamPlayers.Add(new TeamPlayer("NED", 15, 254094, "Edson BRAAFHEID"));
            teamPlayers.Add(new TeamPlayer("NZL", 15, 267783, "Michael McGLINCHEY"));
            teamPlayers.Add(new TeamPlayer("NGA", 15, 269089, "Lukman HARUNA"));
            teamPlayers.Add(new TeamPlayer("PAR", 15, 283757, "Victor CACERES"));
            teamPlayers.Add(new TeamPlayer("POR", 15, 275931, "PEPE"));
            teamPlayers.Add(new TeamPlayer("SRB", 15, 216929, "Nikola ZIGIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 15, 306306, "Miroslav STOCH"));
            teamPlayers.Add(new TeamPlayer("SVN", 15, 314021, "Rene KRHIN"));
            teamPlayers.Add(new TeamPlayer("RSA", 15, 236275, "Lucas THWALA"));
            teamPlayers.Add(new TeamPlayer("ESP", 15, 216814, "SERGIO RAMOS"));
            teamPlayers.Add(new TeamPlayer("SUI", 15, 178791, "Hakan YAKIN"));
            teamPlayers.Add(new TeamPlayer("URU", 15, 185359, "Diego PEREZ"));
            teamPlayers.Add(new TeamPlayer("USA", 15, 271213, "Jay DeMERIT"));

            teamPlayers.Add(new TeamPlayer("ALG", 16, 296819, "Faouzi CHAOUCHI"));
            teamPlayers.Add(new TeamPlayer("ARG", 16, 228528, "Sergio AGUERO"));
            teamPlayers.Add(new TeamPlayer("AUS", 16, 182617, "Carl VALERI"));
            teamPlayers.Add(new TeamPlayer("BRA", 16, 176534, "GILBERTO MELO"));
            teamPlayers.Add(new TeamPlayer("CAM", 16, 3369, "Hamidou SOULEYMANOU"));
            teamPlayers.Add(new TeamPlayer("CHI", 16, 281252, "Fabian ORELLANA"));
            teamPlayers.Add(new TeamPlayer("CIV", 16, 240726, "Aristide ZOGBO"));
            teamPlayers.Add(new TeamPlayer("DEN", 16, 186334, "Stephan ANDERSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 16, 207924, "James MILNER"));
            teamPlayers.Add(new TeamPlayer("FRA", 16, 254133, "Steve MANDANDA"));
            teamPlayers.Add(new TeamPlayer("GER", 16, 196748, "Philipp LAHM"));
            teamPlayers.Add(new TeamPlayer("GHA", 16, 321726, "Stephen AHORLU"));
            teamPlayers.Add(new TeamPlayer("GRE", 16, 214888, "Sotirios KYRGIAKOS"));
            teamPlayers.Add(new TeamPlayer("HON", 16, 213877, "Mauricio SABILLON"));
            teamPlayers.Add(new TeamPlayer("ITA", 16, 214022, "Mauro CAMORANESI"));
            teamPlayers.Add(new TeamPlayer("JAP", 16, 180936, "Yoshito OKUBO"));
            teamPlayers.Add(new TeamPlayer("PRK", 16, 210081, "NAM Song Chol"));
            teamPlayers.Add(new TeamPlayer("KOR", 16, 268406, "KI Sung Yueng"));
            teamPlayers.Add(new TeamPlayer("MEX", 16, 228592, "Efrain JUAREZ"));
            teamPlayers.Add(new TeamPlayer("NED", 16, 254089, "Michel VORM"));
            teamPlayers.Add(new TeamPlayer("NZL", 16, 271380, "Aaron CLAPHAM"));
            teamPlayers.Add(new TeamPlayer("NGA", 16, 184856, "Austin EJIDE"));
            teamPlayers.Add(new TeamPlayer("PAR", 16, 253460, "Cristian RIVEROS"));
            teamPlayers.Add(new TeamPlayer("POR", 16, 210214, "RAUL MEIRELES"));
            teamPlayers.Add(new TeamPlayer("SRB", 16, 298694, "Ivan OBRADOVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 16, 215718, "Jan DURICA"));
            teamPlayers.Add(new TeamPlayer("SVN", 16, 187526, "Aleksander SELIGA"));
            teamPlayers.Add(new TeamPlayer("RSA", 16, 294665, "Itumeleng KHUNE"));
            teamPlayers.Add(new TeamPlayer("ESP", 16, 303034, "Sergio BUSQUETS"));
            teamPlayers.Add(new TeamPlayer("SUI", 16, 216650, "Gelson FERNANDES"));
            teamPlayers.Add(new TeamPlayer("URU", 16, 286481, "Maximiliano PEREIRA"));
            teamPlayers.Add(new TeamPlayer("USA", 16, 289197, "Francisco TORRES"));

            teamPlayers.Add(new TeamPlayer("ALG", 17, 323340, "Adlane GUEDIOURA"));
            teamPlayers.Add(new TeamPlayer("ARG", 17, 198149, "Jonas GUTIERREZ"));
            teamPlayers.Add(new TeamPlayer("AUS", 17, 290802, "Nikita RUKAVYTSYA"));
            teamPlayers.Add(new TeamPlayer("BRA", 17, 239419, "JOSUE"));
            teamPlayers.Add(new TeamPlayer("CAM", 17, 178608, "Mohamadou IDRISSOU"));
            teamPlayers.Add(new TeamPlayer("CHI", 17, 267527, "Gary MEDEL"));
            teamPlayers.Add(new TeamPlayer("CIV", 17, 3355, "Siaka TIENE"));
            teamPlayers.Add(new TeamPlayer("DEN", 17, 299434, "Mikkel BECKMANN"));
            teamPlayers.Add(new TeamPlayer("ENG", 17, 217344, "Shaun WRIGHT-PHILLIPS"));
            teamPlayers.Add(new TeamPlayer("FRA", 17, 211228, "Sebastien SQUILLACI"));
            teamPlayers.Add(new TeamPlayer("GER", 17, 207888, "Per MERTESACKER"));
            teamPlayers.Add(new TeamPlayer("GHA", 17, 305690, "Ibrahim AYEW"));
            teamPlayers.Add(new TeamPlayer("GRE", 17, 230517, "Theofanis GEKAS"));
            teamPlayers.Add(new TeamPlayer("HON", 17, 194210, "Edgar ALVAREZ"));
            teamPlayers.Add(new TeamPlayer("ITA", 17, 210189, "Angelo PALOMBO"));
            teamPlayers.Add(new TeamPlayer("JAP", 17, 289027, "Makoto HASEBE"));
            teamPlayers.Add(new TeamPlayer("PRK", 17, 218098, "AN Yong Hak"));
            teamPlayers.Add(new TeamPlayer("KOR", 17, 268414, "LEE Chung Yong"));
            teamPlayers.Add(new TeamPlayer("MEX", 17, 234551, "Giovani DOS SANTOS"));
            teamPlayers.Add(new TeamPlayer("NED", 17, 291415, "Eljero ELIA"));
            teamPlayers.Add(new TeamPlayer("NZL", 17, 175399, "Dave MULLIGAN"));
            teamPlayers.Add(new TeamPlayer("NGA", 17, 178734, "Chidi ODIAH"));
            teamPlayers.Add(new TeamPlayer("PAR", 17, 209505, "Aureliano TORRES"));
            teamPlayers.Add(new TeamPlayer("POR", 17, 299064, "RUBEN AMORIM"));
            teamPlayers.Add(new TeamPlayer("SRB", 17, 210144, "Milos KRASIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 17, 299810, "Marek HAMSIK"));
            teamPlayers.Add(new TeamPlayer("SVN", 17, 299697, "Andraz KIRM"));
            teamPlayers.Add(new TeamPlayer("RSA", 17, 269990, "Bernard PARKER"));
            teamPlayers.Add(new TeamPlayer("ESP", 17, 297330, "Alvaro ARBELOA"));
            teamPlayers.Add(new TeamPlayer("SUI", 17, 216642, "Reto ZIEGLER"));
            teamPlayers.Add(new TeamPlayer("URU", 17, 286238, "Egidio AREVALO"));
            teamPlayers.Add(new TeamPlayer("USA", 17, 238072, "Jozy ALTIDORE"));

            teamPlayers.Add(new TeamPlayer("ALG", 18, 239237, "Carl MEDJANI"));
            teamPlayers.Add(new TeamPlayer("ARG", 18, 180583, "Martin PALERMO"));
            teamPlayers.Add(new TeamPlayer("AUS", 18, 209168, "Eugene GALEKOVIC"));
            teamPlayers.Add(new TeamPlayer("BRA", 18, 298959, "RAMIRES"));
            teamPlayers.Add(new TeamPlayer("CAM", 18, 310048, "Eyong ENOH"));
            teamPlayers.Add(new TeamPlayer("CHI", 18, 229439, "Gonzalo JARA"));
            teamPlayers.Add(new TeamPlayer("CIV", 18, 178964, "Kader KEITA"));
            teamPlayers.Add(new TeamPlayer("DEN", 18, 235508, "Soren LARSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 18, 182787, "Jamie CARRAGHER"));
            teamPlayers.Add(new TeamPlayer("FRA", 18, 184257, "Alou DIARRA"));
            teamPlayers.Add(new TeamPlayer("GER", 18, 275162, "Toni KROOS"));
            teamPlayers.Add(new TeamPlayer("GHA", 18, 306640, "Dominic ADIYIAH"));
            teamPlayers.Add(new TeamPlayer("GRE", 18, 297316, "Sotiris NINIS"));
            teamPlayers.Add(new TeamPlayer("HON", 18, 178185, "Noel VALLADARES"));
            teamPlayers.Add(new TeamPlayer("ITA", 18, 297409, "Fabio QUAGLIARELLA"));
            teamPlayers.Add(new TeamPlayer("JAP", 18, 233500, "Keisuke HONDA"));
            teamPlayers.Add(new TeamPlayer("PRK", 18, 210033, "KIM Myong Gil"));
            teamPlayers.Add(new TeamPlayer("KOR", 18, 198546, "JUNG Sung Ryong"));
            teamPlayers.Add(new TeamPlayer("MEX", 18, 251352, "Andres GUARDADO"));
            teamPlayers.Add(new TeamPlayer("NED", 18, 217306, "Stijn SCHAARS"));
            teamPlayers.Add(new TeamPlayer("NZL", 18, 198177, "Andy BOYENS"));
            teamPlayers.Add(new TeamPlayer("NGA", 18, 230228, "Victor OBINNA"));
            teamPlayers.Add(new TeamPlayer("PAR", 18, 207707, "Nelson VALDEZ"));
            teamPlayers.Add(new TeamPlayer("POR", 18, 214410, "HUGO ALMEIDA"));
            teamPlayers.Add(new TeamPlayer("SRB", 18, 214994, "Milos NINKOVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 18, 302439, "Erik JENDRISEK"));
            teamPlayers.Add(new TeamPlayer("SVN", 18, 187541, "Aleksandar RADOSAVLJEVIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 18, 4078, "Siyabonga NOMVETHE"));
            teamPlayers.Add(new TeamPlayer("ESP", 18, 318601, "PEDRO"));
            teamPlayers.Add(new TeamPlayer("SUI", 18, 319435, "Albert BUNJAKU"));
            teamPlayers.Add(new TeamPlayer("URU", 18, 209511, "Ignacio GONZALEZ"));
            teamPlayers.Add(new TeamPlayer("USA", 18, 276139, "Brad GUZAN"));

            teamPlayers.Add(new TeamPlayer("ALG", 19, 183882, "Hassan YEBDA"));
            teamPlayers.Add(new TeamPlayer("ARG", 19, 198273, "Diego MILITO"));
            teamPlayers.Add(new TeamPlayer("AUS", 19, 209169, "Richard GARCIA"));
            teamPlayers.Add(new TeamPlayer("BRA", 19, 181415, "JULIO BAPTISTA"));
            teamPlayers.Add(new TeamPlayer("CAM", 19, 200935, "Stephane MBIA"));
            teamPlayers.Add(new TeamPlayer("CHI", 19, 202653, "Gonzalo FIERRO"));
            teamPlayers.Add(new TeamPlayer("CIV", 19, 198106, "Yaya TOURE"));
            teamPlayers.Add(new TeamPlayer("DEN", 19, 179027, "Dennis ROMMEDAHL"));
            teamPlayers.Add(new TeamPlayer("ENG", 19, 186453, "Jermain DEFOE"));
            teamPlayers.Add(new TeamPlayer("FRA", 19, 297102, "Abou DIABY"));
            teamPlayers.Add(new TeamPlayer("GER", 19, 312546, "CACAU"));
            teamPlayers.Add(new TeamPlayer("GHA", 19, 318419, "Lee ADDY"));
            teamPlayers.Add(new TeamPlayer("GRE", 19, 299090, "Sokratis PAPASTATHOPOULOS"));
            teamPlayers.Add(new TeamPlayer("HON", 19, 177986, "Danilo TURCIOS"));
            teamPlayers.Add(new TeamPlayer("ITA", 19, 177655, "Gianluca ZAMBROTTA"));
            teamPlayers.Add(new TeamPlayer("JAP", 19, 214612, "Takayuki MORIMOTO"));
            teamPlayers.Add(new TeamPlayer("PRK", 19, 237148, "RI Chol Myong"));
            teamPlayers.Add(new TeamPlayer("KOR", 19, 263395, "YEOM Ki Hun"));
            teamPlayers.Add(new TeamPlayer("MEX", 19, 271241, "Jonny MAGALLON"));
            teamPlayers.Add(new TeamPlayer("NED", 19, 216733, "Ryan BABEL"));
            teamPlayers.Add(new TeamPlayer("NZL", 19, 274102, "Tommy SMITH"));
            teamPlayers.Add(new TeamPlayer("NGA", 19, 200301, "Chinedu OGBUKE OBASI"));
            teamPlayers.Add(new TeamPlayer("PAR", 19, 322704, "Lucas BARRIOS"));
            teamPlayers.Add(new TeamPlayer("POR", 19, 187199, "TIAGO"));
            teamPlayers.Add(new TeamPlayer("SRB", 19, 317539, "Radosav PETROVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 19, 319373, "Juraj KUCKA"));
            teamPlayers.Add(new TeamPlayer("SVN", 19, 216271, "Suad FILEKOVIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 19, 294661, "Surprise MORIRI"));
            teamPlayers.Add(new TeamPlayer("ESP", 19, 233029, "Fernando LLORENTE"));
            teamPlayers.Add(new TeamPlayer("SUI", 19, 294749, "Eren DERDIYOK"));
            teamPlayers.Add(new TeamPlayer("URU", 19, 176546, "Andres SCOTTI"));
            teamPlayers.Add(new TeamPlayer("USA", 19, 278532, "Maurice EDU"));

            teamPlayers.Add(new TeamPlayer("ALG", 20, 323339, "Djamel MESBAH"));
            teamPlayers.Add(new TeamPlayer("ARG", 20, 184291, "Maxi RODRIGUEZ"));
            teamPlayers.Add(new TeamPlayer("AUS", 20, 197411, "Mark MILLIGAN"));
            teamPlayers.Add(new TeamPlayer("BRA", 20, 188394, "KLEBERSON"));
            teamPlayers.Add(new TeamPlayer("CAM", 20, 290756, "Georges MANDJECK"));
            teamPlayers.Add(new TeamPlayer("CHI", 20, 181603, "Rodrigo MILLAR"));
            teamPlayers.Add(new TeamPlayer("CIV", 20, 213910, "Guy DEMEL"));
            teamPlayers.Add(new TeamPlayer("DEN", 20, 317039, "Thomas ENEVOLDSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 20, 186444, "Ledley KING"));
            teamPlayers.Add(new TeamPlayer("FRA", 20, 297106, "Mathieu VALBUENA"));
            teamPlayers.Add(new TeamPlayer("GER", 20, 299442, "Jerome BOATENG"));
            teamPlayers.Add(new TeamPlayer("GHA", 20, 216742, "Quincy OWUSU-ABEYIE"));
            teamPlayers.Add(new TeamPlayer("GRE", 20, 215515, "Pantelis KAPETANOS"));
            teamPlayers.Add(new TeamPlayer("HON", 20, 155646, "Amado GUEVARA"));
            teamPlayers.Add(new TeamPlayer("ITA", 20, 254182, "Giampaolo PAZZINI"));
            teamPlayers.Add(new TeamPlayer("JAP", 20, 177771, "Junichi INAMOTO"));
            teamPlayers.Add(new TeamPlayer("PRK", 20, 210065, "KIM Myong Won"));
            teamPlayers.Add(new TeamPlayer("KOR", 20, 170828, "LEE Dong Gook"));
            teamPlayers.Add(new TeamPlayer("MEX", 20, 228594, "Jorge TORRES"));
            teamPlayers.Add(new TeamPlayer("NED", 20, 216723, "Ibrahim AFELLAY"));
            teamPlayers.Add(new TeamPlayer("NZL", 20, 274078, "Chris WOOD"));
            teamPlayers.Add(new TeamPlayer("NGA", 20, 290636, "Dickson ETUHU"));
            teamPlayers.Add(new TeamPlayer("PAR", 20, 316199, "Nestor ORTIGOZA"));
            teamPlayers.Add(new TeamPlayer("POR", 20, 199026, "DECO"));
            teamPlayers.Add(new TeamPlayer("SRB", 20, 234404, "Neven SUBOTIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 20, 207956, "Kamil KOPUNEK"));
            teamPlayers.Add(new TeamPlayer("SVN", 20, 187538, "Andrej KOMAC"));
            teamPlayers.Add(new TeamPlayer("RSA", 20, 297811, "Bongani KHUMALO"));
            teamPlayers.Add(new TeamPlayer("ESP", 20, 270714, "Javier MARTINEZ"));
            teamPlayers.Add(new TeamPlayer("SUI", 20, 231624, "Pirmin SCHWEGLER"));
            teamPlayers.Add(new TeamPlayer("URU", 20, 306142, "Alvaro FERNANDEZ"));
            teamPlayers.Add(new TeamPlayer("USA", 20, 296249, "Robbie FINDLEY"));

            teamPlayers.Add(new TeamPlayer("ALG", 21, 323346, "Foued KADIR"));
            teamPlayers.Add(new TeamPlayer("ARG", 21, 306144, "Mariano ANDUJAR"));
            teamPlayers.Add(new TeamPlayer("AUS", 21, 198613, "David CARNEY"));
            teamPlayers.Add(new TeamPlayer("BRA", 21, 298628, "NILMAR"));
            teamPlayers.Add(new TeamPlayer("CAM", 21, 320375, "Joel MATIP"));
            teamPlayers.Add(new TeamPlayer("CHI", 21, 177270, "Rodrigo TELLO"));
            teamPlayers.Add(new TeamPlayer("CIV", 21, 198051, "Emmanuel EBOUE"));
            teamPlayers.Add(new TeamPlayer("DEN", 21, 321716, "Christian ERIKSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 21, 177656, "Emile HESKEY"));
            teamPlayers.Add(new TeamPlayer("FRA", 21, 170710, "Nicolas ANELKA"));
            teamPlayers.Add(new TeamPlayer("GER", 21, 297407, "Marko MARIN"));
            teamPlayers.Add(new TeamPlayer("GHA", 21, 294619, "Kwadwo ASAMOAH"));
            teamPlayers.Add(new TeamPlayer("GRE", 21, 214887, "Konstantinos KATSOURANIS"));
            teamPlayers.Add(new TeamPlayer("HON", 21, 230117, "Emilio IZAGUIRRE"));
            teamPlayers.Add(new TeamPlayer("ITA", 21, 177876, "Andrea PIRLO"));
            teamPlayers.Add(new TeamPlayer("JAP", 21, 198117, "Eiji KAWASHIMA"));
            teamPlayers.Add(new TeamPlayer("PRK", 21, 268400, "RI Kwang Hyok"));
            teamPlayers.Add(new TeamPlayer("KOR", 21, 197964, "KIM Young Kwang"));
            teamPlayers.Add(new TeamPlayer("MEX", 21, 188382, "Adolfo BAUTISTA"));
            teamPlayers.Add(new TeamPlayer("NED", 21, 184615, "Klaas Jan HUNTELAAR"));
            teamPlayers.Add(new TeamPlayer("NZL", 21, 175400, "Jeremy CHRISTIE"));
            teamPlayers.Add(new TeamPlayer("NGA", 21, 267647, "Uwa ECHIEJILE"));
            teamPlayers.Add(new TeamPlayer("PAR", 21, 313535, "Antolin ALCARAZ"));
            teamPlayers.Add(new TeamPlayer("POR", 21, 210212, "RICARDO COSTA"));
            teamPlayers.Add(new TeamPlayer("SRB", 21, 214992, "Dragan MRDJA"));
            teamPlayers.Add(new TeamPlayer("SVK", 21, 309974, "Kornel SALATA"));
            teamPlayers.Add(new TeamPlayer("SVN", 21, 244535, "Dalibor STEVANOVIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 21, 269984, "Siyabonga SANGWENI"));
            teamPlayers.Add(new TeamPlayer("ESP", 21, 200176, "DAVID SILVA"));
            teamPlayers.Add(new TeamPlayer("SUI", 21, 196593, "Johnny LEONI"));
            teamPlayers.Add(new TeamPlayer("URU", 21, 313905, "Sebastian FERNANDEZ"));
            teamPlayers.Add(new TeamPlayer("USA", 21, 298712, "Clarence GOODSON"));

            teamPlayers.Add(new TeamPlayer("ALG", 22, 315883, "Djamal ABDOUN"));
            teamPlayers.Add(new TeamPlayer("ARG", 22, 266783, "Sergio ROMERO"));
            teamPlayers.Add(new TeamPlayer("AUS", 22, 290800, "Dario VIDOSIC"));
            teamPlayers.Add(new TeamPlayer("BRA", 22, 275765, "DONI"));
            teamPlayers.Add(new TeamPlayer("CAM", 22, 320376, "Guy NDY"));
            teamPlayers.Add(new TeamPlayer("CHI", 22, 313874, "Esteban PAREDES"));
            teamPlayers.Add(new TeamPlayer("CIV", 22, 207946, "Souleymane BAMBA"));
            teamPlayers.Add(new TeamPlayer("DEN", 22, 179030, "Jesper CHRISTIANSEN"));
            teamPlayers.Add(new TeamPlayer("ENG", 22, 185384, "Michael CARRICK"));
            teamPlayers.Add(new TeamPlayer("FRA", 22, 214566, "Gael CLICHY"));
            teamPlayers.Add(new TeamPlayer("GER", 22, 177669, "Hans Joerg BUTT"));
            teamPlayers.Add(new TeamPlayer("GHA", 22, 170232, "Richard KINGSON"));
            teamPlayers.Add(new TeamPlayer("GRE", 22, 321732, "Stelios MALEZAS"));
            teamPlayers.Add(new TeamPlayer("HON", 22, 213871, "Donis ESCOBER"));
            teamPlayers.Add(new TeamPlayer("ITA", 22, 216691, "Riccardo MONTOLIVO"));
            teamPlayers.Add(new TeamPlayer("JAP", 22, 177765, "Yuji NAKAZAWA"));
            teamPlayers.Add(new TeamPlayer("PRK", 22, 238408, "KIM Kyong Il"));
            teamPlayers.Add(new TeamPlayer("KOR", 22, 188387, "CHA Du Ri"));
            teamPlayers.Add(new TeamPlayer("MEX", 22, 197890, "Alberto MEDINA"));
            teamPlayers.Add(new TeamPlayer("NED", 22, 160388, "Sander BOSCHKER"));
            teamPlayers.Add(new TeamPlayer("NZL", 22, 268861, "Jeremy BROCKIE"));
            teamPlayers.Add(new TeamPlayer("NGA", 22, 230233, "Dele ADELEYE"));
            teamPlayers.Add(new TeamPlayer("PAR", 22, 178051, "Aldo BOBADILLA"));
            teamPlayers.Add(new TeamPlayer("POR", 22, 254161, "DANIEL FERNANDES"));
            teamPlayers.Add(new TeamPlayer("SRB", 22, 207645, "Zdravko KUZMANOVIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 22, 177868, "Martin PETRAS"));
            teamPlayers.Add(new TeamPlayer("SVN", 22, 187532, "Matej MAVRIC"));
            teamPlayers.Add(new TeamPlayer("RSA", 22, 302620, "Shu-Aib WALTERS"));
            teamPlayers.Add(new TeamPlayer("ESP", 22, 232978, "Jesus NAVAS"));
            teamPlayers.Add(new TeamPlayer("SUI", 22, 209973, "Mario EGGIMANN"));
            teamPlayers.Add(new TeamPlayer("URU", 22, 267829, "Martin CACERES"));
            teamPlayers.Add(new TeamPlayer("USA", 22, 228810, "Benny FEILHABER"));

            teamPlayers.Add(new TeamPlayer("ALG", 23, 323326, "Rais M BOLHI"));
            teamPlayers.Add(new TeamPlayer("ARG", 23, 321683, "Javier PASTORE"));
            teamPlayers.Add(new TeamPlayer("AUS", 23, 178299, "Marco BRESCIANO"));
            teamPlayers.Add(new TeamPlayer("BRA", 23, 233953, "GRAFITE"));
            teamPlayers.Add(new TeamPlayer("CAM", 23, 323698, "Vincent ABOUBAKAR"));
            teamPlayers.Add(new TeamPlayer("CHI", 23, 323720, "Luis MARIN"));
            teamPlayers.Add(new TeamPlayer("CIV", 23, 198105, "Daniel YEBOAH"));
            teamPlayers.Add(new TeamPlayer("DEN", 23, 186369, "Patrick MTILIGA"));
            teamPlayers.Add(new TeamPlayer("ENG", 23, 299956, "Joe HART"));
            teamPlayers.Add(new TeamPlayer("FRA", 23, 184251, "Cedric CARRASSO"));
            teamPlayers.Add(new TeamPlayer("GER", 23, 216784, "Mario GOMEZ"));
            teamPlayers.Add(new TeamPlayer("GHA", 23, 196897, "Kevin Prince BOATENG"));
            teamPlayers.Add(new TeamPlayer("GRE", 23, 323540, "Athanasios PRITTAS"));
            teamPlayers.Add(new TeamPlayer("HON", 23, 208064, "Sergio MENDOZA"));
            teamPlayers.Add(new TeamPlayer("ITA", 23, 321641, "Leonardo BONUCCI"));
            teamPlayers.Add(new TeamPlayer("JAP", 23, 156047, "Yoshikatsu KAWAGUCHI"));
            teamPlayers.Add(new TeamPlayer("PRK", 23, 322908, "PAK Sung Hyok"));
            teamPlayers.Add(new TeamPlayer("KOR", 23, 278124, "KANG Min Soo"));
            teamPlayers.Add(new TeamPlayer("MEX", 23, 271236, "Luis MICHEL"));
            teamPlayers.Add(new TeamPlayer("NED", 23, 180811, "Rafael VAN DER VAART"));
            teamPlayers.Add(new TeamPlayer("NZL", 23, 184756, "James BANNATYNE"));
            teamPlayers.Add(new TeamPlayer("NGA", 23, 244401, "Dele AIYENUGBA"));
            teamPlayers.Add(new TeamPlayer("PAR", 23, 228509, "Rodolfo GAMARRA"));
            teamPlayers.Add(new TeamPlayer("POR", 23, 269784, "FABIO COENTRAO"));
            teamPlayers.Add(new TeamPlayer("SRB", 23, 323620, "Andjelko DJURICIC"));
            teamPlayers.Add(new TeamPlayer("SVK", 23, 299803, "Dusan KUCIAK"));
            teamPlayers.Add(new TeamPlayer("SVN", 23, 299704, "Tim MATAVZ"));
            teamPlayers.Add(new TeamPlayer("RSA", 23, 323345, "Thanduyise KHUBONI"));
            teamPlayers.Add(new TeamPlayer("ESP", 23, 175413, "Pepe REINA"));
            teamPlayers.Add(new TeamPlayer("SUI", 23, 321653, "Xherdan SHAQIRI"));
            teamPlayers.Add(new TeamPlayer("URU", 23, 175629, "Martin SILVA"));
            teamPlayers.Add(new TeamPlayer("USA", 23, 187757, "Marcus HAHNEMANN"));
        }

        void PlayerFace_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayerFace pf = (PlayerFace)sender;
            Player player = pf.Player;
            ShowPlayerInfo(player);
        }

        private void ShowPlayerInfo(Player player)
        {
            var query = from tp in teamPlayers
                        where tp.TeamID == currentGame.PlayingTeamID
                        && tp.Number == Convert.ToInt32(player.Id)
                        && player.Team.TeamID == currentGame.PlayingTeamID
                        select tp;

            if (query.Any())
            {
                TeamPlayer teamPlayer = query.First();
                imgPlayer.ImageSource = new BitmapImage(new Uri(string.Format("http://pt.fifa.com/imgml/tournament/worldcup2010/players/xl/{0}.png", teamPlayer.ID), UriKind.Absolute));
                numPlayer.Text = teamPlayer.Number.ToString();
                txtPlayerName.Text = teamPlayer.Name.ToString();
                grdPlayerInfo.Visibility = Visibility.Visible;
            }
        }

        void PlayerFace_MouseLeave(object sender, MouseEventArgs e)
        {
            grdPlayerInfo.Visibility = Visibility.Collapsed;
        }

        private void ResetPlayerPositions(Team xt1, Team xt2, Canvas xrootCanvas, List<Discoid> xdiscoids, double leftEndX, double rightEndX, double topEndY, double bottomEndY)
        {
            hasPendingGoalResolution = false;

            int columnCount1 = currentGame.Teams[currentGame.Team1ID].Formation.Length;
            int columnCount2 = currentGame.Teams[currentGame.Team2ID].Formation.Length;
            int currentColumn1 = 0;
            int currentColumn2 = 0;
            int currentRow1 = 0;
            int currentRow2 = 0;
            int rowCount1 = currentGame.Teams[currentGame.Team1ID].Formation[currentColumn1];
            int rowCount2 = currentGame.Teams[currentGame.Team2ID].Formation[currentColumn2];
            double columnWidth1 = ((rightEndX - leftEndX) / 2) / columnCount1;
            double columnWidth2 = ((rightEndX - leftEndX) / 2) / columnCount2;
            double rowHeight1 = 2 * ((bottomEndY - topEndY) / 2) / rowCount1;
            double rowHeight2 = 2 * ((bottomEndY - topEndY) / 2) / rowCount2;
            for (int i = 0; i <= 10; i++)
            {
                rowCount1 = currentGame.Teams[currentGame.Team1ID].Formation[currentColumn1];
                rowCount2 = currentGame.Teams[currentGame.Team2ID].Formation[currentColumn2];
                double vOffset1 = ((bottomEndY - topEndY) / 2) - (rowCount1 * rowHeight1) / 2;
                double vOffset2 = ((bottomEndY - topEndY) / 2) - (rowCount2 * rowHeight2) / 2;

                Player p1 = currentGame.Teams[currentGame.Team1ID].players[i];
                Player p2 = currentGame.Teams[currentGame.Team2ID].players[i];

                p1.Position.X = leftEndX + columnWidth1 * (0.5) + columnWidth1 * currentColumn1;
                p1.Position.Y = topEndY + rowHeight1 * (0.5) + rowHeight1 * currentRow1 + vOffset1;

                currentRow1++;
                if (currentRow1 == rowCount1)
                {
                    currentRow1 = 0;
                    currentColumn1++;

                    if (currentColumn1 < currentGame.Teams[currentGame.Team1ID].Formation.Length)
                    {
                        rowCount1 = currentGame.Teams[currentGame.Team1ID].Formation[currentColumn1];
                        columnWidth1 = ((rightEndX - leftEndX) / 2) / columnCount1;
                        rowHeight1 = 2 * ((bottomEndY - topEndY) / 2) / rowCount1;
                    }
                }

                p2.Position.X = rightEndX - columnWidth2 * (0.5) - columnWidth2 * currentColumn2;
                p2.Position.Y = topEndY + rowHeight2 * (0.5) + rowHeight2 * currentRow2 + vOffset2;

                currentRow2++;
                if (currentRow2 == rowCount2)
                {
                    currentRow2 = 0;
                    currentColumn2++;

                    if (currentColumn2 < currentGame.Teams[currentGame.Team2ID].Formation.Length)
                    {
                        rowCount2 = currentGame.Teams[currentGame.Team2ID].Formation[currentColumn2];
                        columnWidth2 = ((rightEndX - leftEndX) / 2) / columnCount2;
                        rowHeight2 = 2 * ((bottomEndY - topEndY) / 2) / rowCount2;
                    }
                }
            }
        }

        void movementTimer_Tick(object sender, EventArgs e)
        {
            movementTimer.Stop();

            MoveResult moveResult = MoveDiscoids();

            if (moveResult != null)
            {
                if (moveResult.IsTurnOver && currentGame.PlayingTeamID == currentGame.Team2ID)
                {
                    GenerateBestShot();
                }
            }

            movementTimer.Start();

            //y = ax + b
            double a = (targetPoint.Y - (double)playerFaces[1].GetValue(Canvas.TopProperty)) / (targetPoint.X - (double)playerFaces[1].GetValue(Canvas.LeftProperty));
            //b = y - ax
            double b = targetPoint.Y - a * targetPoint.X;

            for (int i = 0; i < playerFaces.Count; i++)
            {
                Vector2D v = discoids[i].TranslateVelocity;
                double angle = (double)playerFaces[1].Angle;
                angle = (double)playerFaces[i].Angle + (v.Dot(v)) / 50;
                playerFaces[i].Angle = angle;

                double width = (double)playerFaces[i].GetValue(Canvas.WidthProperty);
                double height = (double)playerFaces[i].GetValue(Canvas.HeightProperty);
                playerFaces[i].SetValue(Canvas.LeftProperty, discoids[5 + i].Position.X - discoids[5 + i].Radius);
                playerFaces[i].SetValue(Canvas.TopProperty, discoids[5 + i].Position.Y - discoids[5 + i].Radius);
            }

            double ballAngle = (double)playerFaces[1].Angle;
            Vector2D v2 = ball.TranslateVelocity;
            ballAngle = (double)bf.Angle + (v2.Dot(v2)) / 10;
            bf.Angle = ballAngle;

            bf.SetValue(Canvas.LeftProperty, ball.Position.X - ball.Radius);
            bf.SetValue(Canvas.TopProperty, ball.Position.Y - ball.Radius);
        }

        void clockTimer_Tick(object sender, EventArgs e)
        {
            scoreControl.Time = scoreControl.Time.AddSeconds(1);
        }

        private void LayoutRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(rootCanvas);

            if ((point.X > strengthPointNW.X) &&
                (point.X < strengthPointSE.X) &&
                (point.Y > strengthPointNW.Y) &&
                (point.Y < strengthPointSE.Y))
            {
                e.Handled = true;
                double relativeY = strengthPointSE.Y - point.Y;
                currentGame.Team1BallStrength = ((strengthPointSE.Y - point.Y) / (strengthPointSE.Y - strengthPointNW.Y)) * 100.0;

                imgBallStrength.Margin = new Thickness(0, point.Y - strengthPointNW.Y - imgBallStrength.ActualHeight / 2.0, 0, 0);
                brdStrength.Margin = new Thickness(8, point.Y - strengthPointNW.Y + imgBallStrength.ActualHeight / 2.0, 8, 8);
            }
            else if (GameHelper.Instance.CurrentMousePlayer == null && GameHelper.Instance.CurrentSelectedPlayer != null && !GameHelper.Instance.IsMovingDiscoids)
            {
                HitPlayer(point.X, point.Y);
            }
            else
            {
                if (GameHelper.Instance.CurrentMousePlayer != null)
                {
                    if (currentGame.PlayingTeamID == GameHelper.Instance.CurrentMousePlayer.Team.TeamID)
                    {
                        Player newSelectedPlayer = GameHelper.Instance.CurrentMousePlayer;
                        ChangePlayer(newSelectedPlayer);
                        GameHelper.Instance.CurrentSelectedPlayer = GameHelper.Instance.CurrentMousePlayer;
                    }
                }
            }
        }

        private void GenerateBestShot()
        {
            double x = 0;
            double y = 0;
            List<GhostBall> ghostBalls = GetGhostBalls(teamsDictionary[currentGame.PlayingTeamID], ball, false);

            GhostBall bestGhostBall = null;

            var ghostBallQuery = from gb in ghostBalls
                                 select gb;

            if (ghostBallQuery.Any())
            {
                bestGhostBall = ghostBallQuery.OrderBy(p => p.Difficulty).OrderBy(p => p.Player2BallDistance).First();
            }

            if (bestGhostBall != null)
            {
                x = bestGhostBall.Point.X;
                y = bestGhostBall.Point.Y;
                GameHelper.Instance.CurrentSelectedPlayer = bestGhostBall.Player;

                currentGame.Team2BallStrength = bestGhostBall.Player2BallDistance / 2;
                HitPlayer(x, y);
            }
        }

        private void ChangePlayer(Player newSelectedPlayer)
        {
            foreach (PlayerFace pf in playerFaces)
            {
                if (pf.Player != newSelectedPlayer)
                    pf.UnSelect();
                else
                    pf.Select();
            }
        }

        void HitPlayer(double x, double y)
        {
            if (scoreControl.Time.Minute == 0 && scoreControl.Time.Second == 0)
            {
                scoreControl.Time = new DateTime(1, 1, 1, 0, 0, 1);
                clockTimer.Start();
            }

            GameHelper.Instance.IsMovingDiscoids = true;
            turnEvents.Clear();
            foreach (PlayerFace pf in playerFaces)
            {
                pf.UnSelect();
            }

            started = true;

            double ballStrength = 50;

            if (currentGame.Team1ID == currentGame.PlayingTeamID)
            {
                ballStrength = currentGame.Team1BallStrength;
            }
            else
            {
                ballStrength = currentGame.Team2BallStrength;
            }

            double v = (ballStrength / 100.0) * 30.0;

            Player selectedPlayer = GameHelper.Instance.CurrentSelectedPlayer;

            ShowPlayerInfo(selectedPlayer);

            double dx = x - GameHelper.Instance.CurrentSelectedPlayer.Position.X;
            double dy = y - GameHelper.Instance.CurrentSelectedPlayer.Position.Y;
            double h = (float)(Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
            double sin = dy / h;
            double cos = dx / h;
            selectedPlayer.IsBallInGoal = false;
            selectedPlayer.TranslateVelocity = new Vector2D(v * cos, v * sin);
            Vector2D normalVelocity = new Vector2D(selectedPlayer.TranslateVelocity.X, selectedPlayer.TranslateVelocity.Y);
            normalVelocity.Normalize();

            //Calculates the top spin/back spin velocity, in the same direction as the normal velocity, but in opposite angle
            double topBottomVelocityRatio = selectedPlayer.TranslateVelocity.Length() * (targetVector.Y / 100.0f);
            selectedPlayer.VSpinVelocity = new Vector2D(-1.0f * topBottomVelocityRatio * normalVelocity.X, -1.0f * topBottomVelocityRatio * normalVelocity.Y);

            //xSound defines if the sound is coming from the left or the right
            double xSound = (double)(selectedPlayer.Position.X - 300.0f) / 300.0f;

            //CreateSnapshot(GetBallPositionList());

            //if (currentGameState != GameState.TestShot)
            //{
            //    PlayCue(GameSound.Shot01);
            //}

            //Calculates the ball positions as long as there are moving balls
            //MoveBalls();
            
            fallenBallsProcessed = false;
        }

        private MoveResult MoveDiscoids()
        {
            if (!started)
                return null;

            MoveResult moveResult = new MoveResult() { DiscoidPositions = new List<DiscoidPosition>(), IsTurnOver = false };

            //Flag indicating that the program is still calculating
            //the positions, that is, the balls are still in an inconsistent state.
            calculatingPositions = true;

            foreach (Discoid discoid in discoids)
            {
                if (Math.Abs(discoid.Position.X) < 5 && Math.Abs(discoid.Position.Y) < 5 && Math.Abs(discoid.TranslateVelocity.X) < 10 && Math.Abs(discoid.TranslateVelocity.Y) < 10)
                {
                    discoid.Position.X =
                    discoid.Position.Y = 0;

                    discoid.TranslateVelocity = new Vector2D(0, 0);
                }
            }

            bool conflicted = true;

            //process this loop as long as some balls are still colliding
            while (conflicted)
            {
                conflicted = false;

                bool someCollision = true;
                while (someCollision)
                {
                    foreach (Goal goal in goals)
                    {
                        bool inGoal = goal.IsBallInGoal(ball);
                    }

                    someCollision = false;
                    foreach (Discoid discoidA in discoids)
                    {
                        if (discoidA is Player)
                        {
                            if (!((Player)discoidA).IsPlaying)
                                break;
                        }

                        //Resolve collisions between balls and each of the 6 borders in the table
                        RectangleCollision borderCollision = RectangleCollision.None;
                        foreach (TableBorder tableBorder in tableBorders)
                        {
                            borderCollision = tableBorder.Colliding(discoidA);

                            //if (borderCollision != RectangleCollision.None && !discoidA.IsBallInGoal)
                            if (borderCollision != RectangleCollision.None)
                            {
                                someCollision = true;
                                tableBorder.ResolveCollision(discoidA, borderCollision);
                            }
                        }

                        //Resolve collisions between players
                        foreach (Discoid discoidB in discoids)
                        {
                            if (discoidB is Player)
                            {
                                if (!((Player)discoidB).IsPlaying)
                                    break;
                            }

                            if (discoidA != discoidB)
                            {
                                //if (discoidA.Colliding(discoidB) && !discoidA.IsBallInGoal && !discoidB.IsBallInGoal)
                                if (discoidA.Colliding(discoidB))
                                {
                                    if ((discoidA is Player && discoidB is Ball) ||
                                        (discoidB is Player && discoidA is Ball))
                                    {
                                        Player p = null;
                                        Ball b = null;
                                        if (discoidA is Player)
                                        {
                                            p = (Player)discoidA;
                                            b = (Ball)discoidB;
                                        }
                                        else
                                        {
                                            p = (Player)discoidB;
                                            b = (Ball)discoidA;
                                        }

                                        turnEvents.Add(new PlayerToBallContact(currentGame.PlayingTeamID, p, new Point(ball.Position.X, ball.Position.Y)));
                                    }
                                    else if (discoidA is Player && discoidB is Player)
                                    {
                                        if (
                                            (((Player)discoidA).Team.TeamID != currentGame.PlayingTeamID) ||
                                            ((Player)discoidB).Team.TeamID != currentGame.PlayingTeamID)
                                        {
                                            PlayerToPlayerContact p2p = new PlayerToPlayerContact(currentGame.PlayingTeamID, (Player)discoidA, (Player)discoidB, new Point(discoidA.Position.X, discoidA.Position.Y));

                                            var q = from te in turnEvents
                                                    where te is PlayerToBallContact
                                                    where ((PlayerToBallContact)te).Player.Team.TeamID == currentGame.PlayingTeamID
                                                    select te;

                                            if (!q.Any())
                                            {
                                                turnEvents.Add(p2p);
                                                Player p = null;
                                                if (((Player)discoidA).Team.TeamID == currentGame.PlayingTeamID)
                                                {
                                                    p = (Player)discoidA;
                                                }
                                                else
                                                {
                                                    p = (Player)discoidB;
                                                }
                                             }
                                        }
                                    }


                                    if (discoidA.Points == 0)
                                    {
                                        strokenPlayers.Add(discoidB);
                                    }
                                    else if (discoidB.Points == 0)
                                    {
                                        strokenPlayers.Add(discoidA);
                                    }

                                    while (discoidA.Colliding(discoidB))
                                    {
                                        someCollision = true;
                                        discoidA.ResolveCollision(discoidB);
                                    }
                                }
                            }
                        }

                        //Calculate ball's translation velocity (movement) as well as the spin velocity.
                        //The friction coefficient is used to decrease ball's velocity
                        if (discoidA.TranslateVelocity.X != 0.0d ||
                            discoidA.TranslateVelocity.Y != 0.0d)
                        {
                            double signalXVelocity = discoidA.TranslateVelocity.X >= 0.0f ? 1.0f : -1.0f;
                            double signalYVelocity = discoidA.TranslateVelocity.Y >= 0.0f ? 1.0f : -1.0f;
                            double absXVelocity = Math.Abs(discoidA.TranslateVelocity.X);
                            double absYVelocity = Math.Abs(discoidA.TranslateVelocity.Y);

                            Vector2D absVelocity = new Vector2D(absXVelocity, absYVelocity);

                            Vector2D normalizedDiff = new Vector2D(absVelocity.X, absVelocity.Y);
                            normalizedDiff.Normalize();

                            absVelocity.X = absVelocity.X * (1.0f - discoidA.Friction) - normalizedDiff.X * discoidA.Friction;
                            absVelocity.Y = absVelocity.Y * (1.0f - discoidA.Friction) - normalizedDiff.Y * discoidA.Friction;

                            if (absVelocity.X < 0f)
                                absVelocity.X = 0f;

                            if (absVelocity.Y < 0f)
                                absVelocity.Y = 0f;

                            double vx = absVelocity.X * signalXVelocity;
                            double vy = absVelocity.Y * signalYVelocity;

                            if (double.IsNaN(vx))
                                vx = 0;

                            if (double.IsNaN(vy))
                                vy = 0;

                            discoidA.TranslateVelocity = new Vector2D(vx, vy);
                        }

                        //Calculate ball's translation velocity (movement) as well as the spin velocity.
                        //The friction coefficient is used to decrease ball's velocity
                        if (discoidA.VSpinVelocity.X != 0.0d || discoidA.VSpinVelocity.Y != 0.0d)
                        {
                            double signalXVelocity = discoidA.VSpinVelocity.X >= 0.0f ? 1.0f : -1.0f;
                            double signalYVelocity = discoidA.VSpinVelocity.Y >= 0.0f ? 1.0f : -1.0f;
                            double absXVelocity = Math.Abs(discoidA.VSpinVelocity.X);
                            double absYVelocity = Math.Abs(discoidA.VSpinVelocity.Y);

                            Vector2D absVelocity = new Vector2D(absXVelocity, absYVelocity);

                            Vector2D normalizedDiff = new Vector2D(absVelocity.X, absVelocity.Y);
                            normalizedDiff.Normalize();

                            absVelocity.X = absVelocity.X - normalizedDiff.X * discoidA.Friction / 1.2f;
                            absVelocity.Y = absVelocity.Y - normalizedDiff.Y * discoidA.Friction / 1.2f;

                            if (absVelocity.X < 0f)
                                absVelocity.X = 0f;

                            if (absVelocity.Y < 0f)
                                absVelocity.Y = 0f;

                            discoidA.VSpinVelocity = new Vector2D(absVelocity.X * signalXVelocity, absVelocity.Y * signalYVelocity);
                        }
                    }

                    //Calculate the ball position based on both the ball's translation velocity and vertical spin velocity.
                    foreach (Discoid discoid in discoids)
                    {
                        discoid.Position.X += discoid.TranslateVelocity.X + discoid.VSpinVelocity.X;
                        discoid.Position.Y += discoid.TranslateVelocity.Y + discoid.VSpinVelocity.Y;
                    }
                }

                conflicted = false;
            }

            
            double totalVelocity = 0;
            foreach (Discoid discoid in discoids)
            {
                totalVelocity += discoid.TranslateVelocity.X;
                totalVelocity += discoid.TranslateVelocity.Y;
            }

            calculatingPositions = false;

            bool isTurnOver = false;
            if (Math.Abs(totalVelocity) < 0.005 && Math.Abs(lastTotalVelocity) > 0)
            {
                totalVelocity = 0;
                foreach(Discoid d in discoids)
                {
                    d.TranslateVelocity = new Vector2D(0, 0);
                }
                //MoveDiscoids();
                //poolState = PoolState.AwaitingShot;
                if (!goals[0].IsBallInGoal(ball) && !goals[1].IsBallInGoal(ball))
                {
                    ball.IsBallInGoal = false;
                }

                if (Math.Abs(lastTotalVelocity) > 0 && hasPendingGoalResolution)
                {
                    //ResetPlayerPositions(currentGame.Teams[currentGame.Team1ID], currentGame.Teams[currentGame.Team2ID], rootCanvas, discoids, goalPost00Point.X, goalPost10Point.X, rowTopEscapeArea.Height.Value, fieldHeight - rowTopEscapeArea.Height.Value);
                    //ball.X = (goalPost00Point.X + goalPost10Point.X) / 2;
                    //ball.Y = (rowTopEscapeArea.Height.Value + fieldHeight - rowTopEscapeArea.Height.Value) / 2;
                }
                ProcessAfterTurn();
                isTurnOver = true;
                GameHelper.Instance.IsMovingDiscoids = false;

                if (scoreControl.Time > totalTime)
                {
                    Team selectedTeam = currentGame.Team1;
                    currentGame = GetNextGame(selectedTeamID, currentGame.Date);

                    LoadPlayerFaces();
                    ResetPlayerPositions(currentGame.Teams[currentGame.Team1ID], currentGame.Teams[currentGame.Team2ID], rootCanvas, discoids, goalPost00Point.X, goalPost10Point.X, rowTopEscapeArea.Height.Value, fieldHeight - rowTopEscapeArea.Height.Value);

                    scoreControl.Team1Name = currentGame.Team1ID;
                    scoreControl.Team1Score = currentGame.Scores[currentGame.Team1ID];
                    scoreControl.Team2Name = currentGame.Team2ID;
                    scoreControl.Team2Score = currentGame.Scores[currentGame.Team2ID];
                    scoreControl.PlayingTeamID = currentGame.PlayingTeamID;
                    scoreControl.Time = new DateTime(1, 1, 1, 0, 0, 1);

                    ball.ResetPosition();
                    bf.SetValue(Canvas.LeftProperty, ball.Position.X - ball.Radius);
                    bf.SetValue(Canvas.TopProperty, ball.Position.Y - ball.Radius);
                }
            }
            lastTotalVelocity = totalVelocity;

            //playerPositionList = GetBallPositionList();

            return new MoveResult() { DiscoidPositions = moveResult.DiscoidPositions, IsTurnOver = isTurnOver };
        }

        private void rootCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        #region IGoalObserver Members

        public void BallEnteredGoal(Goal goal, Ball ball)
        {
            turnEvents.Add(new BallEnteredGoal(currentGame.PlayingTeamID, goal, new Point(ball.Position.X, ball.Position.Y)));
            pottedBalls.Add(ball);
            //if (ball.LastGoalID == 1)
            //{
            //    scoreControl.Team2Score++;
            //}
            //else
            //{
            //    scoreControl.Team1Score++;
            //}
            //letterContainer.Visibility = Visibility.Visible;
            //sbLetters.Begin();
            //hasPendingGoalResolution = true;
        }

        public void BallLeftGoal(Goal goal, Ball ball)
        {
            
        }

        #endregion

        private List<GhostBall> GetGhostBalls(Team team, Ball ball, bool despair)
        {
            List<GhostBall> ghostBalls = new List<GhostBall>();

            int i = 0;
            foreach (Player player in team.players)
            {
                if (player.IsPlaying)
                {
                    foreach (Goal goal in goals)
                    {
                        List<Point> hotSpots = goal.HotSpots;

                        foreach (Point hotSpot in hotSpots)
                        {
                            //distances between goal hotspot and ball center
                            double dxGoalBallOn = hotSpot.X - ball.Position.X;
                            double dyGoalBallOn = hotSpot.Y - ball.Position.Y;
                            double hGoalBallOn = Math.Sqrt(dxGoalBallOn * dxGoalBallOn + dyGoalBallOn * dyGoalBallOn);
                            double a = dyGoalBallOn / dxGoalBallOn;

                            //distances between ball on center and ghost ball center
                            double hBallOnGhost = (ball.Radius - 1.5) * 2.0;
                            double dxBallOnGhost = hBallOnGhost * (dxGoalBallOn / hGoalBallOn);
                            double dyBallOnGhost = hBallOnGhost * (dyGoalBallOn / hGoalBallOn);

                            //ghost ball coordinates
                            double gX = ball.Position.X - dxBallOnGhost;
                            double gY = ball.Position.Y - dyBallOnGhost;
                            double dxGhostPlayer = player.Position.X - gX;
                            double dyGhostPlayer = player.Position.Y - gY;
                            double hGhostPlayer = Math.Sqrt(dxGhostPlayer * dxGhostPlayer + dyGhostPlayer * dyGhostPlayer);

                            //distances between ball center and player center
                            double dxBallOnPlayer = ball.Position.X - player.Position.X;
                            double dyBallOnPlayer = ball.Position.Y - player.Position.Y;
                            double hBallOnPlayer = Math.Sqrt(dxBallOnPlayer * dxBallOnPlayer + dyBallOnPlayer * dyBallOnPlayer);

                            //if (((Math.Sign(dxGoalBallOn) == Math.Sign(dxBallOnPlayer) || dxGoalBallOn == 0) && (Math.Sign(dyGoalBallOn) == Math.Sign(dyBallOnPlayer) || dyGoalBallOn == 0)))
                            //{
                            //    GhostBall ghostBall = new GhostBall(player, new Point(gX, gY), hBallOnPlayer, a, 0);
                            //    ghostBalls.Add(ghostBall);
                            //    i++;
                            //}

                            int value = 0;
                            if (
                                !(((Math.Sign(dxGoalBallOn) == Math.Sign(dxBallOnPlayer) || dxGoalBallOn == 0) && 
                                (Math.Sign(dyGoalBallOn) == Math.Sign(dyBallOnPlayer) || dyGoalBallOn == 0)))
                                )
                            {
                                value += 10;
                            }

                            GhostBall ghostBall = new GhostBall(player, new Point(gX, gY), hBallOnPlayer, a, value);
                            ghostBalls.Add(ghostBall);
                            i++;
                        }
                    }
                }
            }

            return ghostBalls;
        }

        private void ProcessAfterTurn()
        {
            afterTurnProcessed = true;
            currentGame.Fouls[currentGame.PlayingTeamID] = 0;

            int redCount = 0;
            int fallenRedCount = 0;
            int wonPoints = 0;
            int lostPoints = 0;
            bool someInTable = false;

            //List<Discoid> players = from d in discoids.Where( e => e is Player).ToList();

            foreach (Ball ball in pottedBalls)
            {
                if (ball.Points == 0)
                {
                    //ball.ResetPositionAt(ball.InitPosition.X, ball.InitPosition.Y);
                    ball.IsBallInGoal = false;
                }
                else if (ball.Points > 1)
                {
                    //if (fallenRedCount < redCount || teams[playingTeamID - 1].BallOn.Points != ball.Points)
                    //{
                        //if (currentGameState != GameState.TestShot)
                        //{
                        //    if (fallenRedCount < redCount)
                        //        logList.Add(string.Format("{0} is back to table (there are still red balls on the table)", ball.Id));
                        //    else if (teams[playingTeamID - 1].BallOn.Points != ball.Points)
                        //        logList.Add(string.Format("{0} is back to table (expected: {1})", ball.Id, teams[playingTeamID - 1].BallOn.Id));
                        //}

                        //for (int points = ball.Points; points > 1; points--)
                        //{
                        //    Ball candidateBall = GetCandidateBall(ball, points);
                        //    if (candidateBall != null)
                        //    {
                        //        ball.ResetPositionAt(candidateBall.InitPosition.X, candidateBall.InitPosition.Y);
                        //        ball.IsBallInPocket = false;
                        //        break;
                        //    }
                        //}
                    //}
                }
            }

            //if (teams[playingTeamID - 1].BallOn == null)
            //    teams[playingTeamID - 1].BallOn = ballSprites[1];

            bool touchedOpponentBeforeBall = false;
            bool touchedBall = false;

            int strokenDiscoidsCount = 0;
            foreach (Discoid strokenDiscoid in strokenDiscoids)
            {
                if (strokenDiscoid is Ball)
                {
                    touchedBall = true;
                    break;
                }
                else
                {
                    Player strokenPlayer = (Player)strokenDiscoid;
                    //causing the player to first hit an opponent player before hitting the ball
                    if ((strokenPlayer.Team.TeamID != currentGame.PlayingTeamID) && !touchedBall)
                    {
                        touchedOpponentBeforeBall = true;
                        break;
                    }
                }
                
                //if (strokenDiscoidsCount == 0)
                //{
                //    if (ball.Points != teams[playingTeamID - 1].BallOn.Points)
                //    {
                //        if (ball.Points == 1 || teams[playingTeamID - 1].BallOn.Points == 1 || fallenRedCount == redCount)
                //        {
                //            if (currentGameState != GameState.TestShot)
                //                logList.Add(string.Format("foul: {0} was touched first, while {1} was expected", ball.Id, (fallenRedCount < redCount && teams[playingTeamID - 1].BallOn.Points > 1) ? "some color ball" : teams[playingTeamID - 1].BallOn.Id));

                //            teams[playingTeamID - 1].FoulList.Add((teams[playingTeamID - 1].BallOn.Points < 4 ? 4 : teams[playingTeamID - 1].BallOn.Points));
                //            break;
                //        }
                //    }
                //}

                strokenDiscoidsCount++;
            }

            ////Foul: causing the cue ball to miss all object balls
            //if (strokenDiscoidsCount == 0)
            //    teams[playingTeamID - 1].FoulList.Add(4);

            foreach (Ball ball in pottedBalls)
            {
                //causing the cue ball to enter a pocket
                if (!touchedOpponentBeforeBall)
                    currentGame.Scores[currentGame.PlayingTeamID]++;
            }

            //if (teams[playingTeamID - 1].FoulList.Count == 0)
            //{
            //    foreach (Ball ball in pottedBalls)
            //    {
            //        //legally potting reds or colors
            //        wonPoints += ball.Points;

            //        if (currentGameState != GameState.TestShot)
            //            logList.Add(string.Format("Player {0} won {1} points", teams[playingTeamID - 1].CurrentPlayer.Name, wonPoints));
            //    }
            //}
            //else
            //{
            //    teams[playingTeamID - 1].FoulList.Sort();
            //    lostPoints = teams[playingTeamID - 1].FoulList[teams[playingTeamID - 1].FoulList.Count - 1];

            //    if (currentGameState != GameState.TestShot)
            //        logList.Add(string.Format("Player {0} lost {1} points", teams[playingTeamID - 1].CurrentPlayer.Name, lostPoints));
            //}

            //teams[playingTeamID - 1].Points += wonPoints;
            //teams[awaitingTeamID - 1].Points += lostPoints;

            bool swappedPlayers = false;
            //check if it's other player's turn
            if ((!touchedBall || touchedOpponentBeforeBall) && currentGameState != GameState.TestShot)
            {
                swappedPlayers = true;
                //outgoingShot.HasFinishedTurn = true;
                //if (currentGameState != GameState.TestShot)
                //    logList.Add(string.Format("Player {0} has finished turn", teams[playingTeamID - 1].CurrentPlayer.Name));

                //Turnover();

                //cueSprite.Texture = (teams[playingTeamID - 1].Id == 1 ? cueTexture1 : cueTexture2);
            }

            foreach (TurnEvent turnEvent in turnEvents)
            {
                if (turnEvent is BallEnteredGoal)
                {
                    grdStadiumScreen.Visibility = Visibility.Visible;
                    sbStadiumScreen.Begin();
                    hasPendingGoalResolution = true;

                    if (ball.LastGoalID == 1)
                    {
                        scoreControl.Team2Score++;
                    }
                    else
                    {
                        scoreControl.Team1Score++;
                    }
                    Turnover(null);
                    ResetPlayerPositions(currentGame.Teams[currentGame.Team1ID], currentGame.Teams[currentGame.Team2ID], rootCanvas, discoids, goalPost00Point.X, goalPost10Point.X, rowTopEscapeArea.Height.Value, fieldHeight - rowTopEscapeArea.Height.Value);
                    ball.Position.X = (goalPost00Point.X + goalPost10Point.X) / 2;
                    ball.Position.Y = (rowTopEscapeArea.Height.Value + fieldHeight - rowTopEscapeArea.Height.Value) / 2;
                    break;
                }
                else if (turnEvent is PlayerToBallContact)
                {
                    Player p = ((PlayerToBallContact)turnEvent).Player;
                    if (p.Team.TeamID != currentGame.PlayingTeamID)
                    {
                        Turnover(p);
                        break;
                    }                    
                }
                else if (turnEvent is PlayerToPlayerContact)
                {
                    Player p1 = ((PlayerToPlayerContact)turnEvent).Player1;
                    Player p2 = ((PlayerToPlayerContact)turnEvent).Player2;
                    //Player of the playing team touched an opponent
                    //player before the ball, so it's a foul
                    if (p1.Team.TeamID != currentGame.PlayingTeamID)
                    {
                        Turnover(p1);
                        break;
                    }
                    else if (p2.Team.TeamID != currentGame.PlayingTeamID)
                    {
                        Turnover(p2);
                        break;
                    }                    
                }
            }

            if (turnEvents.Count == 0)
            {
                Turnover(null);
            }

            //if (!someInTable && currentGameState != GameState.TestShot)
            //{
            //    UpdateGameState(GameState.GameOver);
            //    outgoingShot.GameOver = true;
            //    return;
            //}

            //int fallenBallsCount = fallenBalls.Count;
            //for (int i = fallenBallsCount - 1; i >= 0; i--)
            //{
            //    if (!fallenBalls[i].IsBallInPocket)
            //    {
            //        fallenBalls.RemoveAt(i);
            //    }
            //}

            //teams[awaitingTeamID - 1].JustSwapped = true;
            //teams[playingTeamID - 1].JustSwapped = swappedPlayers;

            if (swappedPlayers)
            {
                playerState = PlayerState.Aiming;
            }
            else
            {
                playerState = PlayerState.Calling;
                //if (playerState == PlayerState.Aiming)
                //{
                //    if (fallenRedCount < redCount)
                //    {
                //        if (teams[playingTeamID - 1].BallOn.Points == 1)
                //        {
                //            playerState = PlayerState.Calling;
                //        }
                //    }
                //}
            }

            //if (currentGameState != GameState.TestShot)
            //{
            //    teams[playingTeamID - 1].BallOn = GetNextBallOn(swappedPlayers, teams[playingTeamID - 1].BallOn);

            //    Texture2D ballOnTexture = null;
            //    switch (teams[playingTeamID - 1].BallOn.Points)
            //    {
            //        case (int)BallValues.Red:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\RedBall");
            //            break;
            //        case (int)BallValues.Yellow:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\YellowBall");
            //            break;
            //        case (int)BallValues.Green:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\GreenBall");
            //            break;
            //        case (int)BallValues.Brown:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\BrownBall");
            //            break;
            //        case (int)BallValues.Blue:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\BlueBall");
            //            break;
            //        case (int)BallValues.Pink:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\PinkBall");
            //            break;
            //        case (int)BallValues.Black:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\BlackBall");
            //            break;
            //    }

            //    if (teams[playingTeamID - 1].Id == 1)
            //        ballOn1.Texture = ballOnTexture;
            //    else
            //        ballOn2.Texture = ballOnTexture;
            //}

            //targetVector = new Vector2(0, 0);

            if (currentGameState == GameState.Play)
            {
                //teams[playingTeamID - 1].Attempts =
                //teams[awaitingTeamID - 1].Attempts = 0;

                //teams[playingTeamID - 1].AttemptsToWin =
                //teams[awaitingTeamID - 1].AttemptsToWin = 0;

                //teams[playingTeamID - 1].AttemptsNotToLose =
                //teams[awaitingTeamID - 1].AttemptsNotToLose = 0;

                //teams[playingTeamID - 1].AttemptsOfDespair =
                //teams[awaitingTeamID - 1].AttemptsOfDespair = 0;

                //teams[playingTeamID - 1].BestShot.LostPoints =
                //teams[awaitingTeamID - 1].BestShot.LostPoints = 1000;

                //teams[playingTeamID - 1].BestShot.WonPoints =
                //teams[awaitingTeamID - 1].BestShot.WonPoints = 0;

                //teams[playingTeamID - 1].BestShotSelected =
                //teams[awaitingTeamID - 1].BestShotSelected = false;
            }

            strokenDiscoids.Clear();
            pottedBalls.Clear();
        }

        private void Turnover(Player newSelectedPlayer)
        {
            foreach (PlayerFace pf in playerFaces)
            {
                if (pf.Player != newSelectedPlayer)
                    pf.UnSelect();
                else
                    pf.Select();
            }

            string auxID = currentGame.PlayingTeamID;
            currentGame.PlayingTeamID = currentGame.AwaitingTeamID;
            currentGame.AwaitingTeamID = auxID;
            scoreControl.PlayingTeamID = currentGame.PlayingTeamID;
            GameHelper.Instance.CurrentSelectedPlayer = newSelectedPlayer;
        }

        private void brdBallStrengthContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //e.Handled = true;

            //double y = e.GetPosition(brdBallStrengthContainer).Y;
            //if (y > (imgBallStrength.ActualHeight * 2.0) && y < (brdBallStrengthContainer.ActualHeight - imgBallStrength.ActualHeight))
            //{
            //    ballStrength = brdBallStrengthContainer.ActualHeight - y - imgBallStrength.ActualHeight;
            //}
            //imgBallStrength.Margin = new Thickness(0, y - imgBallStrength.ActualHeight / 2.0, 0, 0);
            //brdStrength.Margin = new Thickness(8, y + imgBallStrength.ActualHeight / 2.0, 8, 8);
        }
    }

    public enum PlayerState
    {
        None,
        SelectingNumberOfPlayers,
        SelectingHost,
        Connecting,
        ReceivingInvitation,
        Aiming,
        Calling
    }

    public enum GameState
    {
        None,
        SignIn,
        Setup,
        ShowOpponents,
        Play,
        TestShot,
        GameOver
    }

    public enum FoulTypes
    {
        DirectFreeKick = 1,
        IndirectFreeKick = 2,
        PenaltyKick = 3
    }
}
