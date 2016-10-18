using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ValantInventoryExerciseCore.Models
{
    public class Items
    {
        [Key]
        public string Label { get; set; }
        public DateTime Expiration { get; set; }
        public int ItemType { get; set; }
    }
}
