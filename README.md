# ValantInventory

Running Solution:
More information forthcoming.  Can be debugged and tested from within VS 2015.  Will add command line
dotnet run command information after some further testing.

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
will need to be investigated.

The monitoring of expired items is handled in Startup.cs.  This allows for the direct monitoring
of the dbContext without having to work around the life-cycle of the dbContext in the controller.
This may create issues in the long term as a dbContext instance is kept active over time.  I am
not satisfied with the present methodology.  It is not easily unit tested, and an alternate solution
will need to be investigated and implemented.  The monitoring itself is carried out in a recurring 
Task thread.  This could create concurrency issues if the interval is too short, (or at any point
when experiencing heavy traffic).  This will need to be addressed in order to have large data sets.
In the short term, the interval can be tuned (default is every 5 seconds). Manual testing indicates
that the monitoring is working, but an automated integration test using 
Microsoft.AspNetCore.TestHost.TestServer is still eluding me.

