
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Soccerlight.Core.Model
{
    public class GoalPost : Discoid
    {
        #region attributes
        
        #endregion attributes

        #region constructor

        public GoalPost(Vector2D position, Vector2D size, string id) : base(8, 0.5, true)
        {
            this.Id = id;
            this.Position.X = position.X;
            this.Position.Y = position.Y;
            this.Width = size.X;
            this.Height = size.Y;
        }

        #endregion constructor

        #region properties

        #endregion properties

        #region functions

        #endregion functions
    }
}

