using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ValantInventoryExerciseCore.Models;
using Xunit;

namespace ValantInventoryExerciseCore.Tests
{ 
    public class ValantInventoryExerciseCoreShould
    {
        private TestServer _server;
        private HttpClient _client;

        public ValantInventoryExerciseCoreShould()
        {
            Initialize();
        }

        private async void Initialize()
        {
            await Task.Run(() => _server = new TestServer(new WebHostBuilder().UseStartup<Startup>()));
            await Task.Run(() => _client = _server.CreateClient());
        }

        [Fact]
        public async Task DeleteExpiredItem()
        {
            //Arrange
            //Set up request to test posting a new item
            var request = "/api/Items/";

            var itemToAdd = new Items
            {
                Label = "Add Me",
                //expiration in the past
                Expiration = DateTime.UtcNow.AddYears(-1),
                ItemType = 1

            };

            //wait for server to initialize
            while (Object.ReferenceEquals(null, _client))
            {
                await Task.Delay(1000);
            }
            
            var stPayload = await Task.Run(() => JsonConvert.SerializeObject(itemToAdd));
            var httpContent = new StringContent(stPayload, Encoding.UTF8, "application/json");

            //Act
            var response = await _client.PostAsync(request, httpContent);

            //Retry post.  If proper item expiration has taken place, then it should return 201.
            //Otherwise, it will return 404.
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(1000);
                //Thread.Sleep(1000);

                //retry post
                httpContent = new StringContent(stPayload, Encoding.UTF8, "application/json");
                response = await _client.PostAsync(request, httpContent);

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    break;
                }
            }

            //Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}
