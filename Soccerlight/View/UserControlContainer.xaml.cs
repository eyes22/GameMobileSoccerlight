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

namespace Soccerlight.View
{
    public partial class UserControlContainer : UserControl
    {
        List<BasePage> executingPages = new List<BasePage>();
        public UserControlContainer()
        {
            InitializeComponent();
        }

        public void SwitchToView(Type type, Dictionary<string, object> parameters)
        {
            BasePage switchToPage = null;
            foreach (BasePage page in executingPages)
            {
                if (typeof(Page).FullName == type.FullName)
                {
                    switchToPage = page;
                }
            }

            if (switchToPage == null)
            {
                switchToPage = (BasePage)Activator.CreateInstance(type, new object[] {parameters} );
                executingPages.Add(switchToPage);
            }

            LayoutRoot.Children.Clear();
            Height = switchToPage.Height;
            Width = switchToPage.Width;
            LayoutRoot.Children.Add(switchToPage);
        }

        public void CloseCurrentView()
        {
            executingPages.RemoveAt(executingPages.Count - 1);
            BasePage previousPage = executingPages[executingPages.Count - 1];
            LayoutRoot.Children.Clear();
            Height = previousPage.Height;
            Width = previousPage.Width;
            LayoutRoot.Children.Add(previousPage);
        }
    }
}
