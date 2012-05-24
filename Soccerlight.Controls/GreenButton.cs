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

namespace Soccerlight.Controls
{
    [TemplateVisualState(Name = "Normal", GroupName = "ViewStates")]
    [TemplateVisualState(Name = "Highlighted", GroupName = "ViewStates")]
    public class GreenButton : Button
    {
        public static readonly DependencyProperty ImageContentProperty =
        DependencyProperty.Register("ImageContent", typeof(object),
        typeof(GreenButton), null);

        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string),
        typeof(GreenButton), null);

        public static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.Register("IsHighlighted", typeof(bool), typeof(GreenButton), null);

        public GreenButton()
        {
            DefaultStyleKey = typeof(GreenButton);
            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(GreenButton_IsEnabledChanged);
        }

        void GreenButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                this.IsHighlighted = false;
                ChangeVisualState(true);
                this.Opacity = 0.35;
            }
            else
            {
                this.Opacity = 1.00;
                VisualStateManager.GoToState(this, "Normal", true);
            }
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

        public object Text
        {
            get
            {
                return base.GetValue(TextProperty);
            }
            set
            {
                base.SetValue(TextProperty, value);
            }
        }

        public bool IsHighlighted
        {
            get
            {
                return (bool)base.GetValue(IsHighlightedProperty);
            }
            set
            {
                base.SetValue(IsHighlightedProperty, value);
                ChangeVisualState(true);
            }
        }

        private void ChangeVisualState(bool useTransitions)
        {
            if (IsHighlighted)
            {
                VisualStateManager.GoToState(this, "Highlighted", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.MouseEnter += new MouseEventHandler(GreenButton_MouseEnter);
            this.MouseLeave += new MouseEventHandler(GreenButton_MouseLeave);
            this.MouseMove += new MouseEventHandler(GreenButton_MouseMove);

            this.HorizontalContentAlignment = HorizontalAlignment.Center;
            this.ChangeVisualState(false);
        }

        void GreenButton_MouseMove(object sender, MouseEventArgs e)
        {
            this.IsHighlighted = true;
            ChangeVisualState(true);
        }

        void GreenButton_MouseEnter(object sender, MouseEventArgs e)
        {
            this.IsHighlighted = true;
            ChangeVisualState(true);
        }

        void GreenButton_MouseLeave(object sender, MouseEventArgs e)
        {
            this.IsHighlighted = false;
            ChangeVisualState(true);
        }

        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsHighlighted = !this.IsHighlighted;
            ChangeVisualState(true);
        }
    }
}
