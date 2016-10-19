using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ValantInventoryExerciseCore.Tests
{
    public static class ServerWrapper
    {
        public static TestServer Server { get; set; }

        static ServerWrapper()
        {
            Server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
        }
    }
}
