# ValantInventory

Environment:

ASP.NET Core

Entity Framework Core

InMemory

xUnit


Running Solution:
With the .NET Core SLI (https://docs.microsoft.com/en-us/dotnet/articles/core/tools/index)
the ValantInventoryExerciseCore project can be run with the "dotnet run" command.  Can also
be debugged and tested from within VS 2015.

Endpoints:

POST    /api/items        - create a new item with a unique label

DELETE  /api/items/label  - delete an existing item by label

GET     /api/items/label  - not yet implemented per specification, but post should have a reference to return upon item creation


Notifications:
A message is logged to the console when a message is deleted by the /api/items/label endpoint or when
an item in the database expires (has an Expiration earlier than the current time).


Code Structure:

The application utilizes a  in-memory database (Microsoft.EntityFrameworkCore.InMemory)
to store the items.  This is a temporary measure and will require a more robust and
scalable datastore (e.g. Redis) as a long-term solution.  This change can be made by
modifying the services.AddDbContext call in Startup.cs.  The rest is handled by dependency
injection.

The ItemsController accepts an optional parameter for injecting a TextWriter, allowing the
interception of Console activity for the unit tests.  Other means of logging and notification
will need to be investigated.  The Entity Framework queries are conducted syncronously because of
in-memory database responsiveness.  Once final data store is implemented, testing will need to
be conducted to determine if async/await is appropriate for data store operations.


!!!!
The following is out of sync with the state of the code.  Will rectify soon:
!!!!

The monitoring of expired items...

is handled in Startup.cs.  This allows for the direct monitoring
of the dbContext without having to work around the life-cycle of the dbContext in the controller.
This may create issues in the long term as a dbContext instance is kept active over time.  I am
not satisfied with the present methodology.  It is not easily unit tested, and an alternate solution
will need to be investigated and implemented, preferably running on a different system for the sake
of scalability.  Depending on usage profile, a task scheduler with tasks scheduled upon item creation
to execute at the item expiration date may be more efficient and much easier to unit test.  The
monitoring itself is carried out in a recurring Task thread.  This could create concurrency issues
if the interval is too short, (or at any point when experiencing heavy traffic). This will need to
be addressed in order to have large data sets. In the short term, the interval can be tuned (default
is every 5 seconds). Manual testing indicates that the monitoring is working, but an automated
integration test using Microsoft.AspNetCore.TestHost.TestServer is still eluding me.


