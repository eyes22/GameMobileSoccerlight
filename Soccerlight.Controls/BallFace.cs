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
    public class BallFace : Grid
    {
        public readonly DependencyProperty ImageContentProperty;
        public readonly DependencyProperty NumberProperty;
        public readonly DependencyProperty AngleProperty;

        Image localImageContent = null;
        double localAngle = 0;
        int number;
        ContentPresenter cPresenter;
        RotateTransform rTransform;

        Ellipse e1 = new Ellipse();
        public BallFace(string teamName, int number, Color numberColor, byte r1, byte g1, byte b1, byte r2, byte g2, byte b2, byte r3, byte g3, byte b3, double scale)
        {
            GradientStopCollection gsCollection = new GradientStopCollection();
            gsCollection.Add(new GradientStop() { Offset = 0.0, Color = Color.FromArgb(255, 0x20, 0x20, 0x20) });
            gsCollection.Add(new GradientStop() { Offset = 0.3, Color = Color.FromArgb(255, 0x20, 0x20, 0x20) });
            gsCollection.Add(new GradientStop() { Offset = 0.4, Color = Color.FromArgb(255, 0x20, 0x20, 0x20) });
            gsCollection.Add(new GradientStop() { Offset = 0.5, Color = Color.FromArgb(255, 0x20, 0x20, 0x20) });
            gsCollection.Add(new GradientStop() { Offset = 1.0, Color = Color.FromArgb(255, 0xFF, 0xFF, 0xFF) });

            RadialGradientBrush rgb = new RadialGradientBrush(gsCollection);
            rgb.Center = new Point(0.5, 0.5); //Center="0.5, 0.5" RadiusX="0.9" RadiusY="0.9"
            rgb.RadiusX = 0.25;
            rgb.RadiusY = 0.25;
            //this.Background = rgb;

            //this.Children.Add(new Border() { BorderThickness = new Thickness(2), BorderBrush = new SolidColorBrush(Colors.White)});

            //<Ellipse Margin="11,7,11,23">
            //<Ellipse.Fill>
            //<LinearGradientBrush StartPoint="0,0" EndPoint="0,1">
            //<GradientStop Color="#ffffffff" Offset="0.00"/>
            //<GradientStop x:Name="centralGradientStop" Color="#ff008D00" Offset="1.00"/>
            //</LinearGradientBrush>
            //</Ellipse.Fill>
            //</Ellipse>

            Ellipse eCenter = new Ellipse()
            {
                Margin = new Thickness(0),
                Fill = new SolidColorBrush(Color.FromArgb(255, r1, g1, b1))
            };

            GradientStopCollection gsTopCollection = new GradientStopCollection();
            gsTopCollection.Add(new GradientStop() { Color = Color.FromArgb(255, 0xFF, 0xFF, 0xFF), Offset = 0.0 });
            gsTopCollection.Add(new GradientStop() { Color = Color.FromArgb(255, r1, g1, b1), Offset = 1.0 });

            Ellipse eTop = new Ellipse()
            {
                Margin = new Thickness(10, 2, 10, 35),
                Fill = new LinearGradientBrush()
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = gsTopCollection
                }
            };

            GradientStopCollection gsBottomCollection = new GradientStopCollection();
            gsBottomCollection.Add(new GradientStop() { Color = Color.FromArgb(255, r1, g1, b1), Offset = 0.0 });
            gsBottomCollection.Add(new GradientStop() { Color = Color.FromArgb(255, r1, g1, b1), Offset = 0.5 });
            gsBottomCollection.Add(new GradientStop() { Color = Color.FromArgb(255, r3, g3, b3), Offset = 1.0 });

            Ellipse eBottom = new Ellipse()
            {
                Margin = new Thickness(10, 35, 10, 2),
                Fill = new LinearGradientBrush()
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = gsBottomCollection
                }
            };

            ImageContentProperty = DependencyProperty.Register("ImageContent", typeof(object), typeof(BallFace), new PropertyMetadata(ImageContentPropertyChanged));
            AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(BallFace), new PropertyMetadata(AnglePropertyChanged));

            Grid playerGrid = new Grid();
            playerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(13) });
            playerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(13) });
            playerGrid.Children.Add(new Image() { Margin = new Thickness(0, 0, 0, 0), Source = new BitmapImage(new Uri(string.Format("../Images/{0}.png", teamName), UriKind.Relative)) });
            //playerGrid.Children.Add(new TextBlock() { Text = number.ToString("00"), Foreground = new SolidColorBrush(numberColor), Margin = new Thickness(24, 8, 0, 0), FontSize = 11, FontFamily = new FontFamily("Calibri") });

            cPresenter = new ContentPresenter()
            {
                Content = playerGrid,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Opacity = 1.0,
                Width = 18,
                Height = 18
            };

            this.Children.Add(eCenter);
            this.Children.Add(eTop);
            this.Children.Add(eBottom);
            this.Children.Add(cPresenter);
            this.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));

            TransformGroup tGroup = new TransformGroup();
            tGroup.Children = new TransformCollection();
            tGroup.Children.Add(new ScaleTransform() { ScaleX = scale, ScaleY = scale });
            RotateTransform rt = new RotateTransform() { Angle = 0 };
            tGroup.Children.Add(rt);

            this.RenderTransform = tGroup;

            TransformGroup tGroup2 = new TransformGroup();
            tGroup2.Children = new TransformCollection();
            rTransform = new RotateTransform() { Angle = 0, CenterX = 6.5, CenterY = 6.5 };
            tGroup2.Children.Add(rTransform);

            //<ContentPresenter Grid.Row="0" Margin="0,4,0,0" x:Name="contentPresenter" Content="{TemplateBinding ImageContent}" HorizontalAlignment="Center" VerticalAlignment="Center" Opacity="1.00"/>

            cPresenter.RenderTransform = tGroup2;
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
    }
}
