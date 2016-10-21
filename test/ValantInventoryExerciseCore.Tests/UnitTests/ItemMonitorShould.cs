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
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Tests
{
    public class ItemMonitorShould
    {

        private InventoryApiContext SetUpContext(IEnumerable<Items> items)
        {

            //set up the mock context for new tests
            var dbCOB = new DbContextOptionsBuilder<InventoryApiContext>();

            //use a different database for testing than the default in memory database
            dbCOB.UseInMemoryDatabase("ItemMonitorTestDB");

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

        private ItemMonitor SetUpMonitor(InventoryApiContext context, ILoggerFactory loggerFactory)
        {
            return new ItemMonitor(context, new TestTimerFactory(), loggerFactory);
        }


        [Fact]
        public void ScheduleATask_ToWriteToConsole_When_ScheduleExpiration_IsCalled() 
        {

            //Arrange
            var actionLabel = "NotifyMe!";
            
            var itemToExpire = new Items
            {
                Label = actionLabel,
                Expiration = DateTime.Now.AddDays(-1),
                ItemType = 1

            };
            var itemToNotExpire = new Items { Label = "Don't Notify Me!", Expiration = DateTime.Now.AddYears(1), ItemType = 1 };
            var data = new List<Items>
            {
                itemToExpire,
                itemToNotExpire
            }.AsQueryable();

            var stExpectedConsoleOut = "Item " + itemToExpire.Label + " expired at ";

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            var mockContext = SetUpContext(data);
            var TimersDict = new Dictionary<string, Timer>();
            var mockMonitor = SetUpMonitor(mockContext, new LoggerFactory());

            //Action
            mockMonitor.ScheduleExpiration(itemToExpire);
            mockMonitor.ScheduleExpiration(itemToNotExpire);

            //Assert
            Assert.True(mockMonitor.TimerReferences.ContainsKey(itemToExpire.Label));
            Assert.Equal(stExpectedConsoleOut, stringWriter.ToString().Substring(0, stExpectedConsoleOut.Length));
            Assert.DoesNotContain(itemToNotExpire.Label, stringWriter.ToString());
            Assert.Equal(1, mockContext.Items.Count());
        }

        [Fact]
        public void RemoveATask_When_RemoveScheduledExpiration_IsCalled()
        {

            //Arrange
            var actionLabel = "NotifyMe!";

            var itemToExpire = new Items
            {
                Label = actionLabel,
                Expiration = DateTime.Now.AddDays(-1),
                ItemType = 1

            };
            var data = new List<Items>
            {
                itemToExpire,
                new Items { Label = "Don't Notify Me!", Expiration = DateTime.Now.AddYears(1), ItemType = 1 }
            }.AsQueryable();

            var mockContext = SetUpContext(data);
            var TimersDict = new Dictionary<string, Timer>();
            var mockMonitor = SetUpMonitor(mockContext, new LoggerFactory());

            //Action
            mockMonitor.ScheduleExpiration(itemToExpire);
            mockMonitor.RemoveScheduledExpiration(itemToExpire);

            //Assert
            Assert.False(TimersDict.ContainsKey(itemToExpire.Label));
        }

        [Fact]
        public void ScheduleAllRelevantItems_ForExpiration_When_Instantiated()
        {

            //Arrange
            var actionLabel = "NotifyMe!";

            var itemToExpire1 = new Items
            {
                Label = actionLabel,
                Expiration = DateTime.Now.AddDays(-1),
                ItemType = 1

            };
            var itemToExpire2 = new Items { Label = "Also Notify Me!", Expiration = DateTime.Now.AddDays(1), ItemType = 1 };
            var itemToNotExpire = new Items { Label = "Don't Notify Me!", Expiration = DateTime.Now.AddYears(1), ItemType = 1 };
            var data = new List<Items>
            {
                itemToExpire1,
                itemToExpire2,
                itemToNotExpire
            }.AsQueryable();

            var stExpectedConsoleOut1 = "Item " + itemToExpire1.Label + " expired at ";
            var stExpectedConsoleOut2 = "Item " + itemToExpire2.Label + " expired at ";
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            var mockContext = SetUpContext(data);
            var TimersDict = new Dictionary<string, Timer>();

            //Action
            var mockMonitor = SetUpMonitor(mockContext, new LoggerFactory());
            var stOutput = stringWriter.ToString();

            //Assert
            Assert.DoesNotContain(stExpectedConsoleOut1, stOutput);
            Assert.Contains(stExpectedConsoleOut2, stOutput);
            Assert.DoesNotContain(itemToNotExpire.Label, stOutput);
            Assert.Equal(2, mockContext.Items.Count());
        }

        /*[Fact]
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
        }*/

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
