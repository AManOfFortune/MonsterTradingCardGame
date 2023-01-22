# MonsterTradingCardGame

A API-based deckbuilding game. 

https://github.com/AManOfFortune/MonsterTradingCardGame.git

_Note: This is not a playable game, this is only the C# http-server for that game._

## Usage

### Server

To run the server, open the MonsterTradingCardGame.sln in your favourite IDE and choose the Build & Run option. That's it, the server should now be listening for http traffic on your port 10001. If you want to change said port, open the "Program.cs" file and change the value of the port and restart the server.

### Database

To start the database, make sure you follow these steps:

1. Clone the project.
2. Download binaries as zip from https://www.enterprisedb.com/download-postgresql-binaries
3. Unzip and Copy their content to the "postgre" folder.
4. You should now have 3 batch-files and a folder named "pgsql" inside the postgre folder.
5. Now you should be able to start the database by running the "start_db.cmd" file
6. After successfully starting the database, you can import all tables by running the "import_db.cmd" file while the database is running.

### Client & Unique feature

To test the server, a simple javascript client exists in the "/client" folder. The running server itself is capable of serving you these files, which is this project's unique feature. If you open your browser and type the correct url (e.g. "http://localhost:10001"), the server should return the client files. Test the different functions by clicking on the buttons. Keep in mind this client is temporary and therefore not very user friendly. Open the console to see a cleaner output of what the server returns. The server itself should also log expressive output.

**If this does not work and the server returns "Requested resource not found":** _It is probably a problem with the server not finding the "/client" folder on your machine. Go to the "Router.cs" file, there go to the "FileRoute()" function and change the "path" variable to a string with the absolute path to the "/client" folder._

## How it works

The basic logic is split into ServerLogic, RouteLogic, DatabaseLogic and StateLogic. Each folder represents a namespace and a Layer of the Dataflow. A request goes from the ServerLogic to the RouteLogic, then, if required, the DatabaseLogic. From there, it goes back to the RouteLogic, which deals with the results of the database query, creates a valid response which then gets returned and sent back by the ServerLogic.

#### ServerLogic

Deals with everything related to the actual Http Server. Contains the TCP Listener ("HttpServer" class), the HTTP-Parser ("HttpRequest" class), thread-management and the Http response generator ("HttpResponse" class).

#### RouteLogic

After a request has been parsed, it gets passed on to the "Router". This class deals with request validation and forwarding to the correct implemented "Route". The implemented route deals with all the functional logic associated with the request.

#### DatabaseLogic

To seperate the Database from the functional logic of the routes, it has it's own communication class called "DatabaseRequest". After the database query is done, a "DatabaseResponse" object is returned, that the route has to then further deal with. The Database itself has 4 tables:
+ users
+ cards
+ packs
+ tradingdeals
To see the exact columns and datatypes, check out the "db_copy.sql" file insde the "postgre" folder.

#### StateLogic

Contains two singleton classes that deal with remembering logged in users and the battle queue.

## Tests

The Unit tests present in the "Testing" project test critical code that every request has to pass through. Specifically, they test the HTTP-Parsing and the Router. Also, to make sure battles work as expected, the Card class is tested, which parses the card's name into different, battle-relevant properties.

## Lessons learnt

Generally, the most difficult part of this project was the initial definition of how to structure the whole project. I'm quite happy with the solution I arrived at, but it took several iterations and massive refractorings to get there. For the next projects, I will try to first assess all the requirements before actually starting to code. Realizing half-way in that your function-based approach was not flexible enough could probably have been avoded that way.

## Time spent

The time was not tracked perfectly, but it was something between **90 and 115 hours**.
