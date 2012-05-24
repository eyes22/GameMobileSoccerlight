using System;
using System.Collections.Generic;
using System.Text;

namespace Soccerlight.Core.Model
{
    public interface IGoalObserver
    {
        void BallEnteredGoal(Goal goal, Ball ball);
        void BallLeftGoal(Goal goal, Ball ball);
    }
}
