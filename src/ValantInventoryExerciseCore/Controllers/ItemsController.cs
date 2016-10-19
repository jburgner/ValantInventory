using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using ValantInventoryExerciseCore.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ValantInventoryExerciseCore.Controllers
{
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {

        private readonly InventoryApiContext _context;
        private readonly TextWriter _writer;
        

        // The optional parameter for the injection of a writer dependency is for the
        // testing of console output.
        public ItemsController(InventoryApiContext context, TextWriter writer = null)
        {
            if(!Object.ReferenceEquals(null, writer)) { 
                _writer = writer;
                Console.SetOut(writer);
            }
  
            _context = context;
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

            var items = _context.Items.Where(i => i.Label.Equals(Label));
            
            if(items.Count() == 1)
            {
                var item = items.Single();
            
                _context.Items.Remove(item);
                _context.SaveChanges();

                Console.WriteLine("Item " + Label + " removed from inventory at " + DateTime.UtcNow);

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
            if (_context.Items.Where(i => i.Label.Equals(item.Label)).Count() == 0)
            {

                _context.Items.Add(item);
                _context.SaveChanges();

                //Get action not yet implemented, so reference to GetItem wouldn't be appropriate
                //return this.CreatedAtRoute("GetItem", new { controller = "Items", Label = item.Label }, item);
                return StatusCode(201);
            }else
            {
                return StatusCode(409);
            }
        }
    }
}
