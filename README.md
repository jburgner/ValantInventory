# ValantInventory

Environment:

ASP.NET Core

Entity Framework Core

InMemory database

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

The monitoring of expired items is handled in the ItemMonitor class, instantiated in the database
context using dependency injection.  Every monitoring period (currently configured for an interval
of 7 days), a timer is set for every item set to expire within the next interval + 2 days (currently
9 days).  When the timer is triggered, a message is written to the console.  Newly added items are
given their own timer if they fall within the configured interval.  Newly deleted items have their
timer cancelled if they have one.  Timers are tested by mocking timers through a timer factory.
Mock timers invoke their callback syncronously at instantiation. 

Depending on usage profile and the immediacy requirements of the expiration notification, it could
be better to replace the expiration timers with syncronous code triggered periodically by the
existing monitor.  That would possibly require an additional field added to the data model to track
whether a particular item has already triggered an expiration notification, but it may have some
performance advantage over the current implmentation, particularly if periodic notification of
expired items is acceptable.  However, the current implementation was chosen because it required
very infrequent asyncronous data context interaction, and only asyncronous reads, limiting the
possibility of concurrency issues.


