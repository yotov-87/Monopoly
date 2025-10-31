# Monopoly

Monopoly is a sample full-stack project implemented with .NET 8 (ASP.NET Core) on the backend
and Angular for the frontend. The repository is organized into separate projects for API, real-time
SignalR hubs, business core, data access (EF Core + PostgreSQL), and an Angular client.

This README contains quick start instructions, project layout, and notes for developers.

Status
------
- Basic solution and projects created
- Backend `Monopoly.Api` provides a sample `/weatherforecast` endpoint
- Angular client (`Monopoly.Client`) consumes the backend and displays the example data
- EF Core (Npgsql) and SignalR packages added; PostgreSQL database `monopoly` was created locally

Prerequisites
-------------
- .NET 8 SDK
- Node.js (recommended to use nvm)
- Angular CLI (global) or use `npx @angular/cli`
- PostgreSQL (local or remote)

Quick start (development)
-------------------------
Open two terminals.

1) Start the backend API

```bash
cd /home/dimitar-yotov/Desktop/MonopolyRepo/Monopoly/Monopoly.Api
dotnet run
# API default URL: http://localhost:5262
```

2) Start the Angular client

```bash
cd /home/dimitar-yotov/Desktop/MonopolyRepo/Monopoly/Monopoly.Client
npm install   # first time only
ng serve      # serves on http://localhost:4200
```

3) Verify example endpoint

```bash
curl http://localhost:5262/weatherforecast
```

Notes
-----
- CORS is configured in `Monopoly.Api/Program.cs` to allow requests from `http://localhost:4200`.
- The Angular `WeatherService` currently uses a development base URL pointing to `http://localhost:5262`.
	Consider switching to a proxy (`proxy.conf.json`) or environment-based `apiUrl` for production-like setups.

Project layout
--------------
Top-level `Monopoly/` folder contains:

- `Monopoly.sln` — Visual Studio / dotnet solution.
- `Monopoly.Api/` — ASP.NET Core Web API (controllers, Program.cs). Example endpoint: `/weatherforecast`.
- `Monopoly.Hubs/` — SignalR hubs project (real-time multiplayer hub will go here).
- `Monopoly.Core/` — Business logic and domain models (Player, Property, GameState — to be implemented).
- `Monopoly.Data/` — Data access layer (EF Core + Npgsql). Add DbContext and Entities here.
- `Monopoly.Client/` — Angular application. Basic example components and a `WeatherService` already added.
- `CoPilotInfo.md` — Session log and notes for the assistant (read this first when resuming work).

Developer conventions
---------------------
- All code comments and documentation must be in English. The chat with the maintainers can be in Bulgarian
	but in-repo text (comments, READMEs) must remain English.
- Keep the backend API and frontend running on the default development ports (5262 and 4200) unless you
	intentionally change them.

Next steps (recommended)
------------------------
1. Add loading and error handling to the Angular weather UI and replace the hard-coded base URL with a proxy.
2. Implement core domain models in `Monopoly.Core` (Player, Property, GameState).
3. Create `Monopoly.Data/Context/MonopolyDbContext.cs` and EF Core migrations; add basic tables.
4. Implement `GameHub` in `Monopoly.Hubs` and add a SignalR client service in Angular.
5. Add authentication & authorization (JWT or ASP.NET Identity) before any real multiplayer game data.

Contributing
------------
If you'd like to contribute, please open issues or PRs. Follow the existing code style and write English
comments. Consider creating small, focused changes so they are easy to review.

Troubleshooting
---------------
- If the Angular dev server reports "port in use", either stop the process that uses that port or run
	`ng serve --port 4201` to use an alternate port.
- If the backend does not start, run `dotnet build` to see compilation errors.

License
-------
See the `LICENSE` file in the repository root.

Contact / notes
---------------
Use `CoPilotInfo.md` for an up-to-date session log and to quickly resume development context.
