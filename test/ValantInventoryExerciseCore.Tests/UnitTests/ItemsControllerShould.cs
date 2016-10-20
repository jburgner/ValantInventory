using System;
using System.Threading.Tasks;
using ValantInventoryExerciseCore;
using ValantInventoryExerciseCore.Controllers;
using ValantInventoryExerciseCore.Models;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Tests
{
    public class ItemsControllerShould
    {

        private InventoryApiContext SetUpContext(IEnumerable<Items> items)
        {

            //set up the mock context for new tests
            var dbCOB = new DbContextOptionsBuilder<InventoryApiContext>();

            //use a different database for testing than the default in memory database
            dbCOB.UseInMemoryDatabase("ItemControllerTestDB");

            var mockContext = new InventoryApiContext(dbCOB.Options);

            //delete any existing test items
            mockContext.Items.RemoveRange(mockContext.Items);
            mockContext.SaveChanges();
            //if items were sent as a parameter, add them to the database
            if (!Object.ReferenceEquals(items, null))
            {
                mockContext.Items.AddRange(items);
            }
            mockContext.SaveChanges();

            return mockContext;

        }

        [Fact]
        public void ReturnAViewResult_OfOK_FromDeleteByLabel_WhenLabelExists() 
        {

            //Arrange
            var actionLabel = "DeleteMe!";
            
            var itemToDelete = new Items
            {
                Label = actionLabel,
                Expiration = DateTime.Now.AddDays(1),
                ItemType = 1

            };
            var data = new List<Items>
            {
                itemToDelete,
                new Items { Label = "Don't Delete Me!", Expiration = DateTime.Now.AddYears(1), ItemType = 1 }
            }.AsQueryable();

            var stExpectedConsoleOut = "Item " + itemToDelete.Label + " removed from inventory at ";

            var stringWriter = new StringWriter();
            var mockContext = SetUpContext(data);
            var itemsController = new ItemsController(mockContext, stringWriter);

            //Action
            var result = itemsController.Delete(actionLabel);

            //Assert
            Assert.IsType<OkResult>(result);
            Assert.Contains(stExpectedConsoleOut, stringWriter.ToString());
        }

        [Fact]
        public void DeleteItem_FromDeleteByLabel_WhenLabelExists()
        {

            //Arrange
            var actionLabel = "DeleteMe!";

            var itemToDelete = new Items
            {
                Label = actionLabel,
                Expiration = DateTime.Now.AddDays(1),
                ItemType = 1

            };
            var data = new List<Items>
            {
                itemToDelete,
                new Items { Label = "Don't Delete Me!", Expiration = DateTime.Now.AddYears(1), ItemType = 1 }
            }.AsQueryable();

            var mockContext = SetUpContext(data);
            var itemsController = new ItemsController(mockContext);

            //Action
            var result = itemsController.Delete(actionLabel);

            //Assert
            Assert.Equal(1, mockContext.Items.Count());
            Assert.NotEqual(actionLabel, mockContext.Items.First().Label);
        }

        [Fact]
        public void ReturnAViewResult_OfNotFound_FromDeleteByLabel_WhenLabelDoesntExists()
        {

            //Arrange
            var actionLabel = "DeleteMe!";

            var itemToDelete = new Items
            {
                Label = actionLabel,
                Expiration = DateTime.Now.AddYears(1),
                ItemType = 1

            };
            var data = new List<Items>
            {
                itemToDelete,
                new Items { Label = "Don't Delete Me!", Expiration = DateTime.Now.AddYears(1), ItemType = 1 }
            }.AsQueryable();

            var mockContext = SetUpContext(data);
            var itemsController = new ItemsController(mockContext);

            //Action
            var result = itemsController.Delete("NonexistentLabel");

            //Assert
            //verify result of Not Found
            Assert.IsType<NotFoundResult>(result);
            //verify that item was not deleted from database
            Assert.Equal(2, mockContext.Items.Count());
        }

        [Fact]
        public void InsertANewItemToDB_FromPostNewItem()
        {
            //Arrange
            var itemToAdd = new Items
            {
                Label = "Add Me",
                Expiration = DateTime.Now.AddYears(1),
                ItemType = 1

            };

            var mockContext = SetUpContext(null);
            var itemsController = new ItemsController(mockContext);

            //Action
            var result = itemsController.Post(itemToAdd);

            //Assert
            Assert.Equal(1, mockContext.Items.Count());
            Assert.Equal(itemToAdd, mockContext.Items.Last());
        }

        [Fact]
        public void ReturnsStatusCode_ofCreated_FromPostNewItem_()
        {
            //Arrange
            var itemToAdd = new Items
            {
                Label = "Add Me",
                Expiration = DateTime.Now.AddDays(1),
                ItemType = 1

            };

            var mockContext = SetUpContext(null);
            var itemsController = new ItemsController(mockContext);

            //Action
            var result = itemsController.Post(itemToAdd);

            //Assert
            StatusCodeResult scr = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(scr.StatusCode, 201);
        }

        [Fact]
        public void ReturnStatusCode_OfConflict_FromPostNewItem_WhenLabelIsDuplicate()
        {
            //Arrange
            var itemToAdd = new Items
            {
                Label = "Add Me",
                Expiration = DateTime.Now.AddDays(1),
                ItemType = 1

            };

            var mockContext = SetUpContext(new List<Items> { itemToAdd });
            var itemsController = new ItemsController(mockContext);

            //Action
            var result = itemsController.Post(itemToAdd);

            //Assert
            StatusCodeResult scr = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(scr.StatusCode, 409);
            //verify that no records were added to database
            Assert.Equal(1, mockContext.Items.Count());
        }

        //Moved to Integration tests since Controller not correct context for monitoring expirations
        /*[Fact]
        public async Task ItemExpiring_Triggers_NotificationToConsole()
        {
            //Arrange
            var itemToAdd = new Items
            {
                Label = "Add Me",
                Expiration = DateTime.Now.AddSeconds(-1),
                ItemType = 1

            };

            var stExpectedConsoleOut = "Item " + itemToAdd.Label + " expired at ";

            var stringWriter = new StringWriter();
            var mockContext = SetUpContext(new List<Items> { itemToAdd });
            var itemsController = new ItemsController(mockContext, 2, stringWriter);

            //Action
            var result = itemsController.Post(itemToAdd);
            await Task.Delay(3000);

            //Assert
            Assert.Equal(0, mockContext.Items.Count());
            Assert.Equal(stExpectedConsoleOut, stringWriter.ToString().Substring(0, stExpectedConsoleOut.Length));
            
        }*/
    }
}
