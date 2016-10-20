using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ValantInventoryExerciseCore
{
    public class TimerFactory
    {
        public virtual Timer CreateTimer(TimerCallback callback, object state, int dueTime, int period )
        {
            return new Timer(callback, state, dueTime, period);
        }

        public virtual Timer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return new Timer(callback, state, dueTime, period);
        }
    }

    public class TestTimerFactory : TimerFactory
    {
        public override Timer CreateTimer(TimerCallback callback, object state, int dueTime, int period)
        {
            //immediately invoke callback
            callback.Invoke(state);
            //move the due time to some distant point in the future
            return base.CreateTimer(callback, state, int.MaxValue - 20000, period);
        }

        public override Timer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            //immediately invoke callback
            callback.Invoke(state);
            //move the due time to some distant point in the future
            return base.CreateTimer(callback, state, new TimeSpan(10, 0, 0, 0), period);
        }
    }
}
