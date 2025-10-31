# CoPilotInfo

Date: 2025-10-31

This file is a short log to track what we've done in the "Monopoly" project and how to resume work next time. When I start a session, I read this file first to restore context quickly.

## Summary (what / why)
- Created a .NET 8 solution `Monopoly.sln` and these projects:
  - `Monopoly.Api` (ASP.NET Core Web API)
  - `Monopoly.Hubs` (SignalR hubs project)
  - `Monopoly.Core` (business logic / domain models)
  - `Monopoly.Data` (EF Core, PostgreSQL)
  - `Monopoly.Client` (Angular frontend)

- Installed/configured dependencies:
  - EF Core + Npgsql provider in `Monopoly.Data`
  - `Microsoft.AspNetCore.SignalR.Core` in `Monopoly.Hubs`
  - Angular CLI project in `Monopoly.Client`

- Created a local PostgreSQL database `monopoly` (empty for now).

- Kept the default example endpoint in `Monopoly.Api`: `/weatherforecast`.
- Configured CORS in `Monopoly.Api/Program.cs` to allow requests from `http://localhost:4200`.

## Code changes (important files)
- `Monopoly.Api/Program.cs`
  - Added CORS (AllowAnyHeader, AllowAnyMethod) for origin `http://localhost:4200`.

- `Monopoly.Client/src/app`
  - `app.config.ts` - added `provideHttpClient()` to enable HttpClient usage.
  - `models/weather-forecast.ts` - `WeatherForecast` interface.
  - `services/weather.service.ts` - service that calls `http://localhost:5262/weatherforecast`.
  - `app.ts` (root component) - added `toggleWeather()` and fields `weatherForecasts`, `showWeather`.
  - `app.html` - added UI: a pill/button "Weather Forecast (backend)" and a section to display forecasts.

## Quick start (dev)
1. Start API (from `Monopoly/Monopoly.Api`):
```bash
cd /home/dimitar-yotov/Desktop/MonopolyRepo/Monopoly/Monopoly.Api
dotnet run
```
The API listens on: http://localhost:5262 (and HTTPS if configured).

2. Start Angular client (from `Monopoly/Monopoly.Client`):
```bash
cd /home/dimitar-yotov/Desktop/MonopolyRepo/Monopoly/Monopoly.Client
ng serve
```
Client runs at: http://localhost:4200

3. Test API endpoint (example):
```bash
curl http://localhost:5262/weatherforecast
```

Note: CORS is configured for development, so the frontend can call the backend at `http://localhost:5262` directly.

## Current decisions and rationale
- For quick development we used a hard-coded `baseUrl` in `WeatherService` (`http://localhost:5262`). We will later switch to a proxy (`proxy.conf.json`) or environment-based `apiUrl`.
- The Angular app was created with `--defaults` and a minimal placeholder UI. The priority was to establish an end-to-end connection first.

## Open tasks / Next steps (suggested)
1. (Small) Add loading spinner and error handling in the UI for `WeatherService`.
2. (Small) Replace `WeatherService.baseUrl` with a proxy or environment variable.
3. (Medium) Create a separate `WeatherComponent` instead of using the root template.
4. (High) Add core domain models in `Monopoly.Core` (Player, Property, GameState).
5. (High) Create `Monopoly.Data/Context/MonopolyDbContext.cs` and EF Core migrations; add basic tables.
6. (High) Implement `GameHub` (SignalR) in `Monopoly.Hubs` and add a client SignalR service in Angular.
7. (Later) Authentication & Authorization (decide on JWT or ASP.NET Identity).

## Quick steps to continue after resuming work
1. Verify both servers are running: `dotnet run` and `ng serve`, then open the two URLs and confirm the "Weather Forecast" panel returns data.
2. If adding UI, create `src/app/weather` component and move logic there.
3. For database work: create `Monopoly.Data/Context/MonopolyDbContext.cs`, entities in `Monopoly.Data/Entities`, then run migrations:
```bash
cd Monopoly/Monopoly.Data
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Notes / Assumptions
- Development environment: Linux, Node + Angular CLI + .NET 8 installed.
- PostgreSQL is installed locally and a `monopoly` database exists.
- All comments and documentation should be written in English going forward. The chat will remain in Bulgarian when you (the user) write to me.

---

If you want, I can also maintain this file as a machine-readable TODO (JSON) so I can automatically update it during sessions. Let me know if you prefer that.
