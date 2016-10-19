using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace ValantInventoryExerciseCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            //change this line in order to use a more suitable long-term data store (e.g. Redis)
            services.AddDbContext<InventoryApiContext>(opt => opt.UseInMemoryDatabase());

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            app.UseApplicationInsightsExceptionTelemetry();

            var context = app.ApplicationServices.GetService<InventoryApiContext>();

            //SeedTestData(context);

            MonitorItemsForExpired(context);

            app.UseMvc();
        }

        
        private static void SeedTestData(InventoryApiContext context)
        {
            var testItem1 = new Models.Items
            {
                Label = "TestLabel1",
                Expiration = DateTime.UtcNow.AddYears(1),
                ItemType = 1
            };
            context.Items.Add(testItem1);

            var testItem2 = new Models.Items
            {
                Label = "TestLabel2",
                Expiration = DateTime.UtcNow.AddYears(1),
                ItemType = 1
            };
            context.Items.Add(testItem2);
            context.SaveChanges();
        }


        //monitor item expirations and delete any expired items
        private static void MonitorItemsForExpired(InventoryApiContext context)
        {
            //The interval on which to check for expired items.
            //There could be concurrency issues if the interval is too short.
            //If no concurrency checking is implemented, then this interval
            //may need to be increased as the dataset grows.
            const int _expirationCheckInterval = 5000;

            Timer timer = new Timer(async (callbackState) => {

                var callbackContext = (InventoryApiContext)callbackState;

                var itemsToRemove = await callbackContext.Items.Where(i => i.Expiration < DateTime.UtcNow).ToListAsync();

                callbackContext.Items.RemoveRange(itemsToRemove);
                itemsToRemove
                    .ForEach(i =>
                        Console.WriteLine("Item " + i.Label + " expired at " + i.Expiration.ToString())
                    );
                await callbackContext.SaveChangesAsync();


            }, context, _expirationCheckInterval, _expirationCheckInterval);

        }

    }
}
