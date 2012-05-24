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
using Soccerlight.Core.Model;

namespace Soccerlight.Controls
{
    public class PlayerFace : Grid
    {
        public readonly DependencyProperty ImageContentProperty;
        public readonly DependencyProperty NumberProperty;
        public readonly DependencyProperty AngleProperty;

        Image localImageContent = null;
        double localAngle = 0;
        //int number;
        ContentPresenter cPresenter;
        RotateTransform rTransform;

        Ellipse e1 = new Ellipse();
        Storyboard sb = new Storyboard();
        Storyboard sbPointer = new Storyboard();
        Color centerColor;
        Color topColor;
        Color bottomColor;
        Ellipse eTop;
        Ellipse eBottom;

        public PlayerFace(Player player)
        {
            this.Height = 40;
            this.Width = 40;
            this.Player = player;

            this.MouseEnter += new MouseEventHandler(PlayerFace_MouseEnter);
            this.MouseLeave += new MouseEventHandler(PlayerFace_MouseLeave);

            //ToolTipService.SetToolTip(this, string.Format("{0} - Messi\r\n-3 goals", player.Id));

            GradientStopCollection gsCollection = new GradientStopCollection();
            gsCollection.Add(new GradientStop() { Offset = 0.0, Color = Color.FromArgb(255, 0x20, 0x20, 0x20) });
            gsCollection.Add(new GradientStop() { Offset = 0.3, Color = Color.FromArgb(255, 0x20, 0x20, 0x20) });
            gsCollection.Add(new GradientStop() { Offset = 0.4, Color = Color.FromArgb(255, 0x20, 0x20, 0x20) });
            gsCollection.Add(new GradientStop() { Offset = 0.5, Color = Color.FromArgb(255, 0x20, 0x20, 0x20) });
            gsCollection.Add(new GradientStop() { Offset = 1.0, Color = Color.FromArgb(255, 0xFF, 0xFF, 0xFF) });

            RadialGradientBrush rgb = new RadialGradientBrush(gsCollection);
            rgb.Center = new Point(0.5, 0.5); //Center="0.5, 0.5" RadiusX="0.9" RadiusY="0.9"
            rgb.RadiusX = 0.5;
            rgb.RadiusY = 0.5;
            //this.Background = rgb;

            //<Ellipse Margin="11,7,11,23">
            //<Ellipse.Fill>
            //<LinearGradientBrush StartPoint="0,0" EndPoint="0,1">
            //<GradientStop Color="#ffffffff" Offset="0.00"/>
            //<GradientStop x:Name="centralGradientStop" Color="#ff008D00" Offset="1.00"/>
            //</LinearGradientBrush>
            //</Ellipse.Fill>
            //</Ellipse>

            centerColor = Color.FromArgb(255, player.Team.R1, player.Team.G1, player.Team.B1);
            SolidColorBrush centerColorBrush = new SolidColorBrush(centerColor);
            Ellipse eCenter = new Ellipse()
            {
                Margin = new Thickness(0),
                Fill = centerColorBrush
            };

            ColorAnimation colorAnimation = new ColorAnimation()
            {
                From = Color.FromArgb(255, player.Team.R1, player.Team.G1, player.Team.B1),
                To = Color.FromArgb(255, player.Team.R3, player.Team.G3, player.Team.B3),
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            sb.Children.Add(colorAnimation);
            Storyboard.SetTarget(colorAnimation, centerColorBrush);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("Color"));
            this.Resources.Add("SelectedPlayerSB", sb);


            GradientStopCollection gsTopCollection = new GradientStopCollection();
            gsTopCollection.Add(new GradientStop() { Color = Color.FromArgb(255, 0xFF, 0xFF, 0xFF), Offset = 0.0 });
            gsTopCollection.Add(new GradientStop() { Color = Color.FromArgb(255, player.Team.R1, player.Team.G1, player.Team.B1), Offset = 1.0 });

            eTop = new Ellipse()
            {
                Margin = new Thickness(10, 2, 10, 29),
                Fill = new LinearGradientBrush()
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = gsTopCollection
                }
            };

            GradientStopCollection gsBottomCollection = new GradientStopCollection();
            gsBottomCollection.Add(new GradientStop() { Color = Color.FromArgb(255, player.Team.R1, player.Team.G1, player.Team.B1), Offset = 0.0 });
            gsBottomCollection.Add(new GradientStop() { Color = Color.FromArgb(255, player.Team.R1, player.Team.G1, player.Team.B1), Offset = 0.5 });
            gsBottomCollection.Add(new GradientStop() { Color = Color.FromArgb(255, player.Team.R3, player.Team.G3, player.Team.B3), Offset = 1.0 });

            eBottom = new Ellipse()
            {
                Margin = new Thickness(10, 29, 10, 2),
                Fill = new LinearGradientBrush()
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = gsBottomCollection
                }
            };

            ImageContentProperty = DependencyProperty.Register("ImageContent", typeof(object), typeof(PlayerFace), new PropertyMetadata(ImageContentPropertyChanged));
            AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(PlayerFace), new PropertyMetadata(AnglePropertyChanged));

            Grid playerGrid = new Grid();
            //playerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(18) });
            //playerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(24) });
            playerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(32) });
            playerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(36) });
            playerGrid.Children.Add(new Image() { Margin = new Thickness(-9, 0, 0, 0), Source = new BitmapImage(new Uri(string.Format("../Images/{0}.png", player.Team.TeamID), UriKind.Relative)), Stretch = Stretch.Uniform });
            //playerGrid.Children[0].SetValue(Grid.ColumnProperty, 0);
            //playerGrid.Children[0].SetValue(Grid.RowProperty, 0); 
            playerGrid.Children.Add(new TextBlock() { Text = player.Id, Foreground = new SolidColorBrush(player.Team.NumberColor), Margin = new Thickness(24, 8, 0, 0), FontSize = 11, FontFamily = new FontFamily("Calibri") });
            //playerGrid.Children[1].SetValue(Grid.ColumnProperty, 1);

            cPresenter = new ContentPresenter()
            {
                Content = playerGrid,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 1.0,
                Width = 36,
                Height = 36
            };

            //e1.Stroke = new SolidColorBrush(Colors.Red);
            //e1.StrokeThickness = 4;
            //e1.Fill = new SolidColorBrush(Colors.Yellow);

            Image imgPointer = new Image()
            {
                Source = new BitmapImage(new Uri("../Images/PlayerPointer.PNG", UriKind.Relative)),
                Opacity = 0.50

            };

            TransformGroup tgPointer = new TransformGroup();
            TranslateTransform tTransformPointer = new TranslateTransform()
            {
                X = -14,
                Y = -14
            };
            RotateTransform rTransformPointer = new RotateTransform()
            {
                Angle = 0,
                CenterX = 5.5,
                CenterY = 5.5
            };
            ScaleTransform scTransformPointer = new ScaleTransform()
            {
                ScaleX = 3.5,
                ScaleY = 3.5
            };
            tgPointer.Children.Add(tTransformPointer);
            tgPointer.Children.Add(rTransformPointer);
            tgPointer.Children.Add(scTransformPointer);
            imgPointer.RenderTransform = tgPointer;

            this.Children.Add(eCenter);
            this.Children.Add(eTop);
            this.Children.Add(eBottom);
            this.Children.Add(cPresenter);
            //this.Children.Add(imgPointer);
            this.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
            //Grid.Row="0" Margin="0,4,0,0" 
            //x:Name="contentPresenter" 
            //Content="{TemplateBinding ImageContent}"
            //HorizontalAlignment="Center" 
            //VerticalAlignment="Center" 
            //Opacity="1.00"

            TransformGroup tGroup = new TransformGroup();
            tGroup.Children = new TransformCollection();
            tGroup.Children.Add(new ScaleTransform() { ScaleX = 1.00, ScaleY = 1.00});
            RotateTransform rt = new RotateTransform() { Angle = 0 };
            tGroup.Children.Add(rt);

            this.RenderTransform = tGroup;

            TransformGroup tGroup2 = new TransformGroup();
            tGroup2.Children = new TransformCollection();
            rTransform = new RotateTransform() { Angle = 0, CenterX = 20.5, CenterY = 17 };
            tGroup2.Children.Add(rTransform);

            //<ContentPresenter Grid.Row="0" Margin="0,4,0,0" x:Name="contentPresenter" Content="{TemplateBinding ImageContent}" HorizontalAlignment="Center" VerticalAlignment="Center" Opacity="1.00"/>

            cPresenter.RenderTransform = tGroup2;

            sbPointer = new Storyboard()
            {
                Duration = new Duration(new TimeSpan(0, 0, 1)),
                RepeatBehavior = RepeatBehavior.Forever
            };
            DoubleAnimation dAnimationPointer = new DoubleAnimation()
            {
                From = 0,
                To = 360
            };
            Storyboard.SetTarget(dAnimationPointer, rTransform);
            Storyboard.SetTargetProperty(dAnimationPointer, new PropertyPath("Angle"));
            sbPointer.Children.Add(dAnimationPointer);
        }

        void PlayerFace_MouseLeave(object sender, MouseEventArgs e)
        {
            GameHelper.Instance.CurrentMousePlayer = null;
            if (GameHelper.Instance.CurrentSelectedPlayer == null)
            {
                //eTop.Visibility = Visibility.Visible;
                //eBottom.Visibility = Visibility.Visible;
                sb.Stop();
            }
            else
            {
                if (GameHelper.Instance.CurrentSelectedPlayer != this.Player)
                {
                    //eTop.Visibility = Visibility.Visible;
                    //eBottom.Visibility = Visibility.Visible;
                    sb.Stop();
                }
            }
        }

        void PlayerFace_MouseEnter(object sender, MouseEventArgs e)
        {
            GameHelper.Instance.CurrentMousePlayer = this.Player;
            //eTop.Visibility = Visibility.Collapsed;
            //eBottom.Visibility = Visibility.Collapsed;
            sb.Begin();
        }

        void ImageContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            localImageContent = (Image)e.NewValue;
            //cPresenter.Content = localImageContent;
        }

        void AnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            localAngle = (double)e.NewValue;
            rTransform.Angle = localAngle;
        }

        public object ImageContent
        {
            get
            {
                return base.GetValue(ImageContentProperty);
            }
            set
            {
                base.SetValue(ImageContentProperty, value);
            }
        }

        private Image LocalImageContent
        {
            get
            {
                return localImageContent;
            }
            set
            {
                localImageContent = value;
            }
        }

        public object Angle
        {
            get
            {
                return base.GetValue(AngleProperty);
            }
            set
            {
                base.SetValue(AngleProperty, value);
            }
        }

        private double LocalAngle
        {
            get
            {
                return localAngle;
            }
            set
            {
                localAngle = value;
                rTransform.Angle = localAngle;
            }
        }

        public Player Player { get; set; }

        public void UnSelect()
        {
            sb.Stop();
            sbPointer.Stop();
        }

        public void Select()
        {
            //eTop.Visibility = Visibility.Collapsed;
            //eBottom.Visibility = Visibility.Collapsed;
            sb.Begin();
            sbPointer.Begin();
        }
    }

    public class Teams
    {
        public static string Argentina = "Argentina";
        public static string Brazil = "Brazil";
        public static string France = "France";
        public static string Germany = "Germany";
        public static string Italy = "Italy";
        public static string Portugal = "Portugal";
        public static string Uruguay = "Uruguay";
    }
}
