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
    public class ItemsControllerTests
    {

        private InventoryApiContext SetUpContext(IEnumerable<Items> items)
        {

            var dbCOB = new DbContextOptionsBuilder<InventoryApiContext>();

            dbCOB.UseInMemoryDatabase("TestDB");

            var mockContext = new InventoryApiContext(dbCOB.Options);

            //delete any existing test items
            mockContext.Items.RemoveRange(mockContext.Items);
            mockContext.SaveChanges();
            if (!Object.ReferenceEquals(items, null))
            {
                mockContext.Items.AddRange(items);
            }
            mockContext.SaveChanges();

            return mockContext;

        }

        [Fact]
        public void DeleteByLabel_ReturnsAViewResult_OfOK_WhenLabelExists() 
        {

            //Arrange
            var actionLabel = "DeleteMe!";
            
            var itemToDelete = new Items
            {
                Label = actionLabel,
                Expiration = DateTime.UtcNow.AddYears(1),
                ItemType = 1

            };
            var data = new List<Items>
            {
                itemToDelete,

                new Items { Label = "Don't Delete Me!", Expiration = DateTime.UtcNow.AddYears(1), ItemType = 1 }
            }.AsQueryable();

            var stExpectedConsoleOut = "Item " + itemToDelete.Label + " removed from inventory at ";

            var stringWriter = new StringWriter();
            var mockContext = SetUpContext(data);
            var itemsController = new ItemsController(mockContext, 20000, stringWriter);

            //Action
            var result = itemsController.Delete(actionLabel);

            //Assert
            Assert.IsType<OkResult>(result);
            Assert.Equal(stExpectedConsoleOut, stringWriter.ToString().Substring(0, stExpectedConsoleOut.Length));
        }

        [Fact]
        public void DeleteByLabel_DeletesItem_WhenLabelExists()
        {

            //Arrange
            var actionLabel = "DeleteMe!";

            var itemToDelete = new Items
            {
                Label = actionLabel,
                Expiration = DateTime.UtcNow.AddYears(1),
                ItemType = 1

            };
            var data = new List<Items>
            {
                itemToDelete,

                new Items { Label = "Don't Delete Me!", Expiration = DateTime.UtcNow.AddYears(1), ItemType = 1 }
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
        public void DeleteByLabel_ReturnsAViewResult_OfNotFound_WhenLabelDoesntExists()
        {

            //Arrange
            var actionLabel = "DeleteMe!";

            var itemToDelete = new Items
            {
                Label = actionLabel,
                Expiration = DateTime.UtcNow.AddYears(1),
                ItemType = 1

            };
            var data = new List<Items>
            {
                itemToDelete,

                new Items { Label = "Don't Delete Me!", Expiration = DateTime.UtcNow.AddYears(1), ItemType = 1 }
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
        public void PostNewItem_InsertsNew_Item_toDB()
        {
            //Arrange
            var itemToAdd = new Items
            {
                Label = "Add Me",
                Expiration = DateTime.UtcNow.AddYears(1),
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
        public void PostNewItem_ReturnsAViewResult_ofOK()
        {
            //Arrange
            var itemToAdd = new Items
            {
                Label = "Add Me",
                Expiration = DateTime.UtcNow.AddYears(1),
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
        public void PostNewItemwithDuplicateLabel_Returns409()
        {
            //Arrange
            var itemToAdd = new Items
            {
                Label = "Add Me",
                Expiration = DateTime.UtcNow.AddYears(1),
                ItemType = 1

            };

            var mockContext = SetUpContext(new List<Items> { itemToAdd });
            var itemsController = new ItemsController(mockContext);

            //Action
            var result = itemsController.Post(itemToAdd);

            //Assert
            StatusCodeResult scr = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(scr.StatusCode, 409);
        }

        [Fact]
        public void PostNewItemwithDuplicateLabel_DoesNotInserttoDB()
        {
            //Arrange
            var itemToAdd = new Items
            {
                Label = "Add Me",
                Expiration = DateTime.UtcNow.AddSeconds(1),
                ItemType = 1

            };

            var mockContext = SetUpContext(new List<Items> { itemToAdd });
            var itemsController = new ItemsController(mockContext);

            //Action
            var result = itemsController.Post(itemToAdd);

            //Assert
            Assert.Equal(1, mockContext.Items.Count());
        }

        [Fact]
        public async Task ItemExpiring_Triggers_NotificationToConsole()
        {
            //Arrange
            var itemToAdd = new Items
            {
                Label = "Add Me",
                Expiration = DateTime.UtcNow.AddSeconds(-1),
                ItemType = 1

            };

            var stExpectedConsoleOut = "Item " + itemToAdd.Label + " expired at ";

            var stringWriter = new StringWriter();
            var mockContext = SetUpContext(new List<Items> { itemToAdd });
            var itemsController = new ItemsController(mockContext, 2, stringWriter);

            //Action
            var result = itemsController.Post(itemToAdd);
            await Task.Delay(250);

            //Assert
            Assert.Equal(0, mockContext.Items.Count());
            Assert.Equal(stExpectedConsoleOut, stringWriter.ToString().Substring(0, stExpectedConsoleOut.Length));
            
        }
    }
}
