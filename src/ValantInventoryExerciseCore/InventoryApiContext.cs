using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using ValantInventoryExerciseCore.Models;

namespace ValantInventoryExerciseCore
{
    public class InventoryApiContext : DbContext
    {

        //public ItemMonitor ItemMonitor { get; set; }

        public InventoryApiContext(DbContextOptions<InventoryApiContext> options)
            : base(options)
        {
            //ItemMonitor = new ItemMonitor(this, new Dictionary<string, Timer>(), new TimerFactory());
        }

        public DbSet<Items> Items { get; set; }
    }

}
