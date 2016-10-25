using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValantInventoryExerciseCore.Models;

namespace ValantInventoryExerciseCore
{

    public interface IItemMonitor
    {
        void ScheduleExpiration(Items item);
        void RemoveScheduledExpiration(Items item);
    }

    public class ItemMonitor : IItemMonitor
    {

        //Maintain task references to keep them from garbage collection
        //and also to check if timer has already been set for a particular item.
        //Item label is used for hash key.
        //private readonly Dictionary<string, Timer> _timerReferences;
        private readonly InventoryApiContext _context;

        private readonly TimeSpan _scheduleFromContextReRunInterval;
        private readonly TimeSpan _taskScheduleLimit;

        private Timer _scheduledRunFromContextTimer;

        private ITimerFactory _timerFactory;

        private ILogger _logger;

        public Dictionary<string, Timer> TimerReferences { get; }

        public ItemMonitor(InventoryApiContext context, ITimerFactory TimerFactory, ILoggerFactory loggerFactory)
        {
            //initialize private variables
            _context = context;
            TimerReferences = new Dictionary<string, Timer>();
            _timerFactory = TimerFactory;
            _logger = loggerFactory.CreateLogger<ItemMonitor>();

            //run periodic monitoring addition weekly
            _scheduleFromContextReRunInterval = new TimeSpan(7, 0, 0, 0);

            //the timer task will be scheduled to run weekly, but get set
            //timers for any expirations dates within 9 days so the timer
            //will be rerun before the covered window expires
            _taskScheduleLimit = _scheduleFromContextReRunInterval + new TimeSpan(2, 0, 0, 0);

            //schedule recurring item monitor, which will schedule expiration task for each item about to expire
            _scheduledRunFromContextTimer = _timerFactory.CreateTimer(x =>
            {
                ScheduleFromContext();
            }, null, _taskScheduleLimit, _scheduleFromContextReRunInterval);

        }

        //Schedule the item passed in for expiration
        public void ScheduleExpiration(Items item)
        {
            //the timespan between the item expiration and now
            TimeSpan timeSpan = item.Expiration.Subtract(DateTime.Now);

            //if the item has already expired, schedule immediately
            if (timeSpan.Ticks < 0)
            {
                timeSpan = new TimeSpan(0);
            }
            //if a timer has not already been set for this item
            if (!TimerReferences.ContainsKey(item.Label))
            {
                //do not schedule for item too far in advance
                if (timeSpan <= _taskScheduleLimit)
                {
                    //schedule one-time timer and preserve reference in Dictionary
                    TimerReferences.Add(item.Label, _timerFactory.CreateTimer(x =>
                    {
                        ItemExpired((Items)x);
                        
                    }, item, timeSpan, new TimeSpan(0,0,0,0,-1)));
                }
            }
        }

        private void ItemExpired(Items item)
        {
            var stMessage = "Item " + item.Label + " expired at " + item.Expiration.ToString();
            _logger.LogInformation(stMessage);
            _context.Remove(item);
            _context.SaveChanges();

            //remove timer reference from dictionary
            TimerReferences.Remove(item.Label);
        }

        public void RemoveScheduledExpiration(Items item)
        {
            //dispose timer and remove dictionary entry
            Timer timerToDispose;
            if (TimerReferences.TryGetValue(item.Label, out timerToDispose))
            {
                timerToDispose.Dispose();
                TimerReferences.Remove(item.Label);
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
