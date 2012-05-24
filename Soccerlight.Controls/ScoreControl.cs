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
using System.Windows.Media.Imaging;

namespace Soccerlight.Controls
{
    public class ScoreControl : Grid
    {
        TextBlock txtTeam1ID = new TextBlock();
        TextBlock txtTeam2ID = new TextBlock();
        TextBlock txtTeam1Score = new TextBlock();
        TextBlock txtTeam2Score = new TextBlock();
        Border brdTeam1ScoreAux = new Border();
        Border brdTeam2ScoreAux = new Border();
        TextBlock txtTeam1ScoreAux = new TextBlock();
        TextBlock txtTeam2ScoreAux = new TextBlock();
        int team1Score = 0;
        int team2Score = 0;
        Thickness score1TopMargin = new Thickness(0, 20, 0, 0);
        Thickness score2TopMargin = new Thickness(0, 20, 0, 0);
        Storyboard sbScore1 = new Storyboard();
        Storyboard sbScore2 = new Storyboard();
        TranslateTransform translateTransform1;
        TranslateTransform translateTransform2;
        Image imgBall1 = new Image();
        Image imgBall2 = new Image();
        string playingTeamID = "";
        TextBlock txtTime = new TextBlock();
        DateTime time = new DateTime(1, 1, 1, 0, 0, 0);

        public ScoreControl()
        {
            brdTeam1ScoreAux.MaxHeight = 
            brdTeam2ScoreAux.MaxHeight = 20;
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(50) });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0) });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(50) });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
            this.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(16) });
            this.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });

            TextBlock txtX = new TextBlock() { Text = "x" };

            imgBall1.Source =
            imgBall2.Source = new BitmapImage(new Uri("../Images/Jabulani.png", UriKind.Relative));

            imgBall1.SetValue(MarginProperty, new Thickness(0, 2, 0, 0));
            imgBall2.SetValue(MarginProperty, new Thickness(0, 2, 0, 0));
            RotateTransform rotateTransform1 = new RotateTransform() { Angle = 0, CenterX = 9, CenterY = 9 };
            RotateTransform rotateTransform2 = new RotateTransform() { Angle = 0, CenterX = 9, CenterY = 9 };
            imgBall1.SetValue(RenderTransformProperty, rotateTransform1);
            imgBall2.SetValue(RenderTransformProperty, rotateTransform2);

            Border brdTime = new Border();
            txtTime.Text = "12:45";
            brdTime.Child = txtTime;
            brdTime.CornerRadius = new CornerRadius(20, 20, 0, 0);
            brdTime.Margin = new Thickness(20, 0, 20, 0);

            Storyboard sbBalls = new Storyboard()
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            DoubleAnimation ballOpacityAnimation1 = new DoubleAnimation()
            {
                From = 0.2,
                To = 1.0,
                Duration = new Duration(new TimeSpan(0, 0, 0, 1, 0)),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            sbBalls.Children.Add(ballOpacityAnimation1);
            Storyboard.SetTarget(ballOpacityAnimation1, imgBall1);
            Storyboard.SetTargetProperty(ballOpacityAnimation1, new PropertyPath("Opacity"));

            DoubleAnimation ballOpacityAnimation2 = new DoubleAnimation()
            {
                From = 0.2,
                To = 1.0,
                Duration = new Duration(new TimeSpan(0, 0, 0, 1, 0)),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            sbBalls.Children.Add(ballOpacityAnimation2);
            Storyboard.SetTarget(ballOpacityAnimation2, imgBall2);
            Storyboard.SetTargetProperty(ballOpacityAnimation2, new PropertyPath("Opacity"));

            DoubleAnimation ballAngle1 = new DoubleAnimation()
            {
                From = 0.0,
                To = 360,
                Duration = new Duration(new TimeSpan(0, 0, 0, 3, 0)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            sbBalls.Children.Add(ballAngle1);
            Storyboard.SetTarget(ballAngle1, rotateTransform1);
            Storyboard.SetTargetProperty(ballAngle1, new PropertyPath("Angle"));

            DoubleAnimation ballAngle2 = new DoubleAnimation()
            {
                From = 0.0,
                To = 360,
                Duration = new Duration(new TimeSpan(0, 0, 0, 3, 0)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            sbBalls.Children.Add(ballAngle2);
            Storyboard.SetTarget(ballAngle2, rotateTransform2);
            Storyboard.SetTargetProperty(ballAngle2, new PropertyPath("Angle"));

            sbBalls.Begin();

            brdTime.SetValue(Grid.RowProperty, 0);
            brdTime.SetValue(Grid.ColumnProperty, 1);
            brdTime.SetValue(Grid.ColumnSpanProperty, 5);

            imgBall1.SetValue(Grid.ColumnProperty, 0);
            imgBall1.SetValue(Grid.RowProperty, 1);
            txtTeam1ID.SetValue(Grid.ColumnProperty, 1);
            txtTeam1ID.SetValue(Grid.RowProperty, 1);
            txtTeam1Score.SetValue(Grid.ColumnProperty, 2);
            txtTeam1Score.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Right);
            txtTeam1Score.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            txtTeam1Score.SetValue(Grid.RowProperty, 1);
            brdTeam1ScoreAux.SetValue(Grid.ColumnProperty, 2);
            brdTeam1ScoreAux.SetValue(Grid.RowProperty, 1);
            txtX.SetValue(Grid.ColumnProperty, 3);
            txtX.SetValue(Grid.RowProperty, 1);
            txtTeam2Score.SetValue(Grid.ColumnProperty, 4);
            txtTeam2Score.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Left);
            txtTeam2Score.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            txtTeam2Score.SetValue(Grid.RowProperty, 1);
            brdTeam2ScoreAux.SetValue(Grid.ColumnProperty, 4);
            brdTeam2ScoreAux.SetValue(Grid.RowProperty, 1);
            txtTeam2ID.SetValue(Grid.ColumnProperty, 5);
            txtTeam2ID.SetValue(Grid.RowProperty, 1);
            imgBall2.SetValue(Grid.ColumnProperty, 6);
            imgBall2.SetValue(Grid.RowProperty, 1);

            //txtTeam1Name.Width = 10;
            //txtTeam2Name.Width = 10;
            //txtTeam1Score.Width = 10;
            //txtTeam2Score.Width = 10;

            txtTeam1ID.Foreground =
            txtTeam2ID.Foreground = new SolidColorBrush(Colors.Black);
            txtX.Foreground =
            txtTeam1Score.Foreground =
            txtTeam2Score.Foreground =
            txtTeam1ScoreAux.Foreground =
            txtTeam2ScoreAux.Foreground = 
            txtTime.Foreground = new SolidColorBrush(Colors.White);

            LinearGradientBrush lgb = new LinearGradientBrush() { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
            lgb.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(255, 160, 140, 0), Offset = 0.0 });
            //lgb.GradientStops.Add(new GradientStop() { Color = Colors.White, Offset = 0.3 });
            lgb.GradientStops.Add(new GradientStop() { Color = Colors.White, Offset = 0.4 });
            lgb.GradientStops.Add(new GradientStop() { Color = Colors.White, Offset = 0.5 });
            //lgb.GradientStops.Add(new GradientStop() { Color = Colors.White, Offset = 0.6 });
            //lgb.GradientStops.Add(new GradientStop() { Color = Colors.White, Offset = 0.7 });
            lgb.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(255, 160, 140, 0), Offset = 1.0 });

            LinearGradientBrush lgbNumbers = new LinearGradientBrush() { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
            lgbNumbers.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(255, 80, 20, 0), Offset = 0.0 });
            lgbNumbers.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(255, 200, 160, 160), Offset = 0.3 });
            lgbNumbers.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(255, 120, 30, 0), Offset = 0.5 });
            lgbNumbers.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(255, 80, 20, 0), Offset = 1.0 });

            brdTeam1ScoreAux.Background =
            brdTeam2ScoreAux.Background =
            brdTime.Background = lgbNumbers;

            txtTeam1ID.FontSize =
            txtTeam2ID.FontSize =
            txtX.FontSize =
            txtTeam1Score.FontSize =
            txtTeam2Score.FontSize =
            txtTeam1ScoreAux.FontSize =
            txtTeam2ScoreAux.FontSize = 16;
            txtTime.FontSize = 14;

            txtTeam1ID.FontWeight =
            txtTeam2ID.FontWeight =
            txtX.FontWeight =
            txtTeam1Score.FontWeight =
            txtTeam2Score.FontWeight =
            txtTeam1ScoreAux.FontWeight =
            txtTeam2ScoreAux.FontWeight =
            txtTime.FontWeight = FontWeights.Bold;

            txtTeam1ID.FontFamily =
            txtTeam2ID.FontFamily =
            txtX.FontFamily =
            txtTeam1Score.FontFamily =
            txtTeam2Score.FontFamily =
            txtTeam1ScoreAux.FontFamily =
            txtTeam2ScoreAux.FontFamily =
            txtTime.FontFamily = new FontFamily("Arial Narrow");

            txtTeam1ID.HorizontalAlignment =
            txtTeam2ID.HorizontalAlignment =
            txtX.HorizontalAlignment =
            txtTeam1Score.HorizontalAlignment =
            txtTeam2Score.HorizontalAlignment =
            txtTeam1ScoreAux.HorizontalAlignment =
            txtTeam2ScoreAux.HorizontalAlignment =
            txtTime.HorizontalAlignment = HorizontalAlignment.Center;

            txtTeam1ScoreAux.Text = "2";

            txtTeam2ScoreAux.Text = "3";

            translateTransform1 = new TranslateTransform()
            {
                 X = 0,
                 Y = 0 
            };

            translateTransform2 = new TranslateTransform()
            {
                X = 0,
                Y = 0
            };

            txtTeam1Score.RenderTransform = translateTransform1;
            txtTeam2Score.RenderTransform = translateTransform2;

            this.Width = 560;
            //StackPanel stackPanel = new StackPanel();
            //stackPanel.Orientation = Orientation.Horizontal;
            //stackPanel.Children.Add(txtTeam1Name);
            //stackPanel.Children.Add(txtTeam2Name);
            //stackPanel.Width = 1000;
            //this.Children.Add(stackPanel);

            Border brdRound = new Border();
            brdRound.CornerRadius = new CornerRadius(0, 0, 20, 20);
            //brdRound.BorderBrush = new SolidColorBrush(Colors.Black);
            brdRound.BorderThickness = new Thickness(2);
            brdRound.SetValue(Grid.ColumnProperty, 1);
            brdRound.SetValue(Grid.ColumnSpanProperty, 5);
            brdRound.SetValue(Grid.RowProperty, 1);
            //Border brdGreen = new Border();
            //brdGreen.Background = new SolidColorBrush(Colors.Green);
            //brdGreen.SetValue(Grid.ColumnSpanProperty, 5);
            //brdGreen.SetValue(Grid.ColumnProperty, 0);
            //brdGreen.SetValue(Grid.RowProperty, 1);

            brdRound.Background = lgb;// new SolidColorBrush(Colors.Black);

            Image maskImage = new Image();
            maskImage.Source = new BitmapImage(new Uri("../Images/ScoreMask.png", UriKind.Relative));

            brdTeam1ScoreAux.Child = txtTeam1ScoreAux;
            brdTeam2ScoreAux.Child = txtTeam2ScoreAux;

            this.Children.Add(brdRound);
            this.Children.Add(imgBall1);
            this.Children.Add(txtTeam1ID);
            this.Children.Add(txtTeam2ID);
            this.Children.Add(txtX);
            this.Children.Add(txtTeam1Score);
            this.Children.Add(txtTeam2Score);
            this.Children.Add(brdTeam1ScoreAux);
            this.Children.Add(brdTeam2ScoreAux);
            this.Children.Add(imgBall2);
            this.Children.Add(brdTime);
            //this.Children.Add(brdGreen);
            //this.Children.Add(maskImage);
            this.VerticalAlignment = VerticalAlignment.Bottom;

            //this.Background = new SolidColorBrush(Colors.Red);

            sbScore1 = new Storyboard();
            sbScore1.Completed += new EventHandler(sbScore1_Completed);
            DoubleAnimation dAnimation1 = new DoubleAnimation()
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 2)),
                From = 20,
                To = 0,
            };

            sbScore2 = new Storyboard();
            sbScore2.Completed += new EventHandler(sbScore2_Completed);
            DoubleAnimation dAnimation2 = new DoubleAnimation()
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 2)),
                From = 20,
                To = 0
            };


            sbScore1.Children.Add(dAnimation1);
            Storyboard.SetTarget(dAnimation1, translateTransform1);
            Storyboard.SetTargetProperty(dAnimation1, new PropertyPath("Y"));

            sbScore2.Children.Add(dAnimation2);
            Storyboard.SetTarget(dAnimation2, translateTransform2);
            Storyboard.SetTargetProperty(dAnimation2, new PropertyPath("Y"));
        }

        void sbScore1_Completed(object sender, EventArgs e)
        {
            txtTeam1Score.Text = team1Score.ToString();
            //txtTeam1ScoreAux.Visibility = Visibility.Collapsed;
        }

        void sbScore2_Completed(object sender, EventArgs e)
        {
            txtTeam2Score.Text = team2Score.ToString();
            //txtTeam2ScoreAux.Visibility = Visibility.Collapsed;
        }

        public string Team1Name
        {
            get { return txtTeam1ID.Text; }
            set { txtTeam1ID.Text = value; }
        }
        public string Team2Name
        {
            get { return txtTeam2ID.Text; }
            set { txtTeam2ID.Text = value; }
        }

        public int Team1Score
        {
            get { return team1Score; }
            set
            {
                team1Score = value;
                txtTeam1ScoreAux.Visibility = Visibility.Visible;
                txtTeam1ScoreAux.Text = team1Score.ToString();
                sbScore1.Begin();
            }
        }
        public int Team2Score
        {
            get { return team2Score; }
            set
            {
                team2Score = value;
                txtTeam2ScoreAux.Visibility = Visibility.Visible;
                txtTeam2ScoreAux.Text = team2Score.ToString();
                sbScore2.Begin();
            }
        }

        public string PlayingTeamID
        {
            get { return playingTeamID; }
            set
            {
                playingTeamID = value;
                imgBall1.Visibility = (playingTeamID == txtTeam1ID.Text) ? Visibility.Visible : Visibility.Collapsed;
                imgBall2.Visibility = (playingTeamID == txtTeam2ID.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public DateTime Time
        {
            get { return time; }
            set
            { 
                time = value;
                txtTime.Text = time.ToString("mm:ss");
            }
        }
    }
}
