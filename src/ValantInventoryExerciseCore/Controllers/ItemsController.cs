using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using ValantInventoryExerciseCore.Models;
using Microsoft.Extensions.Logging;
using System.Net;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ValantInventoryExerciseCore.Controllers
{
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {

        private readonly InventoryApiContext _context;
        private readonly TextWriter _writer;
        private readonly ItemMonitor _itemMonitor;

        private readonly ILogger _logger;

        // The optional parameter for the injection of a writer dependency is for the
        // testing of console output.
        public ItemsController(InventoryApiContext context, IItemMonitor ItemMonitor, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ItemsController>();
            _context = context;
            _itemMonitor = (ItemMonitor)ItemMonitor;
        }

        //not yet implemented
        /*[HttpGet("{Label}", Name = "GetItem")]
        public async Task<IActionResult> Get(string Label)
        {
            var items = await _context.Items.ToArrayAsync();

            var response = items.Select(i => new
            {
                label = i.Label,
                expiration = i.Expiration,
                itemType = i.ItemType
            }).Where(i => i.label == Label).Single();

            return Ok(response);
        }*/

        [HttpDelete("{Label}")]
        //DELETE: api/items/Label
        public IActionResult Delete(string Label)
        {
            //TODO: test async/await for performance with large datasets and heavy traffic
            var items = _context.Items.Where(i => i.Label.Equals(Label));
            
            if(items.Count() == 1)
            {
                var item = items.Single();
            
                _context.Items.Remove(item);
                _context.SaveChanges();

                var stMessage = "Item " + Label + " removed from inventory at " + DateTime.Now;
                Console.WriteLine(stMessage);
                _logger.LogInformation(stMessage);
                //Item is to be deleted, remove its timer
                _itemMonitor.RemoveScheduledExpiration(item);

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost(Name ="CreateItem")]
        // POST api/items
        public IActionResult Post([FromBody]Items item)
        {
            //TODO: test async/await for performance with large datasets and heavy traffic
            //check for uniqueness of label
            if (_context.Items.Where(i => i.Label.Equals(item.Label)).Count() == 0)
            {
                //If the item has not yet expired
                if(item.Expiration > DateTime.Now)
                {
                    _context.Items.Add(item);
                    _context.SaveChanges();

                    //New Item, add an expiration Timer
                    _itemMonitor.ScheduleExpiration(item);

                    //Get action not yet implemented, so reference to GetItem wouldn't be appropriate
                    //return this.CreatedAtRoute("GetItem", new { controller = "Items", Label = item.Label }, item);
                    return StatusCode(201);

                }else
                {
                    //Item has already expired.  Return Bad Request.
                    return StatusCode(400);
                }
            }
            else
            {
                //An item with this label already exists.  Return Conflict.
                return StatusCode(409);
            }
        }
    }
}
