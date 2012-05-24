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

namespace Soccerlight.View
{
    public class BasePage : UserControl
    {
        protected Dictionary<string, object> Parameters { get; set; }

        public BasePage(Dictionary<string, object> parameters)
        {
            this.Parameters = parameters;
        }
    }
}
