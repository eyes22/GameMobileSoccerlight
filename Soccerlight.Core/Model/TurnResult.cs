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
    public class MoveResult
    {
        public MoveResult()
        {
        }
        
        public List<DiscoidPosition> DiscoidPositions { get; set; }
        public bool IsTurnOver { get; set; }
    }
}
