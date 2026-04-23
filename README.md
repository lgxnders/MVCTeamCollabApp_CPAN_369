### Overview
This project is a team collaboration-based web application based in C#, and uses the Model-View-Controller software architecture pattern.

Leveraging SignalR for real-time communication, multiple, concurrent users are able to work simultaneously on a written document or Trello-like task board.


### Project Setup
.NET 9.0 SDK + Visual Studio 2022 is required. 

1. git clone https://github.com/lgxnders/MVCTeamCollabApp_CPAN_369
2. cd MVCTeamCollabApp_CPAN_369\TeamCollabApp
3. dotnet restore
4. In Visual Studio, right-click on the solution and select 'Configure Startup Projects'.
5. Click 'Multiple startup projects', then ensure that the TeamCollabApp, and its SearchApi and TasksApi are selected to start.
6. Go to to the url that opens in your browser, and try logging in with a test account, such as 'austin@gmail.com', with the password, 'Peanut!123!'
