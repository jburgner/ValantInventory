using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ValantInventoryExerciseCore
{

    // The purpose of this class is to be able to mock timer creation for testing
    // via dependency injection. When the TimerFactory class is injected, the timer
    // will be scheduled normally.  When the TestTimerFactory is injected, the callback
    // will be invoked immediately.

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


    // Inherit from TimerFactory for injecting as TimerFactory
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
