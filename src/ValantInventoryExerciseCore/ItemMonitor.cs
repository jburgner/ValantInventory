using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValantInventoryExerciseCore.Models;

namespace ValantInventoryExerciseCore
{
    public class ItemMonitor
    {
        //maintain task references to keep them from garbage collection
        private readonly Dictionary<string, Timer> _taskReferences;
        private readonly InventoryApiContext _context;

        private readonly TimeSpan _scheduleFromContextReRunInterval;
        private readonly TimeSpan _taskScheduleLimit;

        private Timer _scheduledRunFromContextTimer;

        private TimerFactory _timerFactory;

        public ItemMonitor(InventoryApiContext context, Dictionary<string, Timer> TaskReferences, TimerFactory TimerFactory)
        {
            _context = context;
            _taskReferences = TaskReferences;
            _timerFactory = TimerFactory;

            //run periodic monitoring addition weekly
            _scheduleFromContextReRunInterval = new TimeSpan(7, 0, 0, 0);

            //the limit before which a task will be scheduled
            //two days more than the re-run interval so the task will be rerun before the covered window expires
            _taskScheduleLimit = _scheduleFromContextReRunInterval + new TimeSpan(2, 0, 0, 0);

            _scheduledRunFromContextTimer = _timerFactory.CreateTimer(x =>
            {
                ScheduleFromContext();
            }, null, _taskScheduleLimit, _scheduleFromContextReRunInterval);

        }

        public void ScheduleExpiration(Items item)
        {

            TimeSpan timeSpan = item.Expiration.Subtract(DateTime.Now);
            if (timeSpan.Ticks < 0)
            {
                timeSpan = new TimeSpan(0);
            }
            if (!_taskReferences.ContainsKey(item.Label))
            {
                if (timeSpan <= _taskScheduleLimit)
                {
                    _taskReferences.Add(item.Label, _timerFactory.CreateTimer(x =>
                    {
                        TimerElapsed((Items)x);
                        
                    }, item, timeSpan, new TimeSpan(0,0,0,0,-1)));
                }
            }
        }

        private void TimerElapsed(Items item)
        {
            Console.WriteLine("Item " + item.Label + " expired at " + item.Expiration.ToString());
            //uncomment to enable removal of expired items
            //_context.RemoveRange(_context.Items.Where(i => i.Label == item.Label));
            //_context.SaveChanges();
            _taskReferences.Remove(item.Label);
        }

        public void RemoveScheduledExpiration(Items item)
        {
            //dispose and remove dictionary entry
            Timer timerToDispose;
            if (_taskReferences.TryGetValue(item.Label, out timerToDispose))
            {
                timerToDispose.Dispose();
                _taskReferences.Remove(item.Label);
            }
        }

        private void ScheduleFromContext()
        {
            _context.Items
                //items that are scheduled to expire between now and the task schedule limit
                .Where(i =>
                    i.Expiration < DateTime.Now + _taskScheduleLimit
                    && i.Expiration > DateTime.Now
                )
                .ToList().ForEach(i =>
                {
                    //schedule expiration task if it does not already exist
                    ScheduleExpiration(i);  
                });
        }

    }
}
