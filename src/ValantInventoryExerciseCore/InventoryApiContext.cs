using Microsoft.EntityFrameworkCore;
using ValantInventoryExerciseCore.Models;

namespace ValantInventoryExerciseCore
{
    public class InventoryApiContext : DbContext
    {
        public InventoryApiContext(DbContextOptions<InventoryApiContext> options)
            : base(options)
        {
        }

        public DbSet<Items> Items { get; set; }
    }
}
