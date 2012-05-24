using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Soccerlight.Core.Model
{
    public class Ball : Discoid
    {
        #region attributes

        private Vector2D initPosition;

        #endregion attributes

        #region constructor

        public Ball(Vector2D position, Vector2D size, string id) : base(5, 0.3, false)
        {
            initPosition = position;
            this.Id = id;
            friction = 0.020F;
        }

        #endregion constructor

        #region properties

        public int LastGoalID { get; set; }

        #endregion properties

        #region functions

        public void ResetPosition()
        {
            this.position.X = initPosition.X;
            this.position.Y = initPosition.Y;
        }

        #endregion functions
    }
}

