using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using ValantInventoryExerciseCore.Models;

namespace ValantInventoryExerciseCore
{
    public class InventoryApiContext : DbContext
    {

        public ItemMonitor ItemMonitor { get; set; }

        public InventoryApiContext(DbContextOptions<InventoryApiContext> options)
            : base(options)
        {
            ItemMonitor = new ItemMonitor(this, new Dictionary<string, Timer>(), new TimerFactory());
        }

        public override int SaveChanges()
        {

            foreach(var entry in ChangeTracker.Entries<Items>())
            {

                
                if (entry.State == EntityState.Deleted)
                {
                    //if the Item is to be deleted, remove its timer
                    ItemMonitor.RemoveScheduledExpiration(entry.Entity);
                }else if(entry.State == EntityState.Added)
                {
                    //if this is a new Item, add a Timer
                    ItemMonitor.ScheduleExpiration(entry.Entity);
                }

                //TODO: Handle modified items, as the expiration date may have changed.
                //Not yet implemented in initial spec.
            }
                

            return base.SaveChanges();
        }

        public DbSet<Items> Items { get; set; }
    }

}
