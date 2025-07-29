# pnet-semestr-work

Bird-watching application for enthusiast written in C#.

## How to run locally

0. Make sure, you have installed dotnet SDK, version 8 (and 9 if you are using SetupViaCLI).
1. Delete file birdwatching.db from directory *Api*, if you don't want dummy data. Then in this dir run *dotnet ef database update*.
2. Run server - in *Api* directory go to CLI and run using *dotnet run*.
3. If you want to have all common czech birds in database, go to *SetupViaCLI* and run *dotnet run -- string string ../downloading_birds/czech_bird_taxonomy_full_local.cs* to automatically supply the database these birds.
4. Run client - in *Client* dir run dotnet via *dotnet run*. Open browser on http://localhost:5284/ if not opened automatically.

## How to presumably setup server

Get the whole solution on the server. Run the Api and route the traffic to the Client.

## How to update database

After updating the model in project *Shared*, you should also:

1. Add migration in *Api* project, by: *dotnet ef migrations add <name>*.
2. Update database in the same project by: *dotnet ef database update*.
3. Go to project *Shared* and run *dotnet build*, if it was a significant change.

## How to update an automatically generated client

When you do changes to controllers in the *Api* folder, which affects the api in a way that is not purely internal
(e.g. changes the output, introduces a new api endpoint, ...), you should:

- In the *Shared* project run *dotnet build*, to recreate the BirdApiClient.
