# StreetWars

StreetWars is a .NET 10 MAUI card game built around territory domination and the CardGameEngine.

## Current base game

Two players fight for five city territories:

- Downtown
- Industrial
- Docks
- Old Highway
- Neon District

Each territory is represented by the same board slot on both CGE player boards. A car occupies a territory and contributes attack + life as its territory power. A territory is controlled by the player with the greater power. Three controlled territories win the match.

Cars are concrete CardGameEngine.MonsterCard types and their engine component derives from MonsterCardComponent.

The backend owns the authoritative StreetWarsGame state. MAUI clients send commands through ASP.NET Core SignalR:

- CreateGame
- JoinGame
- PlayCar
- Attack
- EndTurn

The backend can replace Player B with an IStreetWarsAi implementation registered through DI.

## Projects

- `src/StreetWars` — .NET MAUI client.
- `src/StreetWars.Backend` — ASP.NET Core Web API/SignalR game host.
- `lib/CGE` — CardGameEngine submodule.
- `lib/orbit` — Orbit submodule.

## Running locally

Start the backend with the HTTP profile:

```text
http://localhost:5062
```

The SignalR hub is:

```text
http://localhost:5062/gameHub
```

On the Android emulator the MAUI client uses `10.0.2.2:5062`.

The current base implementation keeps game sessions in memory. Restarting the backend ends all active games.

## SignalR

SignalR is used as the first real-time transport for the base implementation. ASP.NET Core registers the hub with `AddSignalR()` and maps it with `MapHub()`; the MAUI app uses `Microsoft.AspNetCore.SignalR.Client` 10.0.12. MQTT can be introduced later behind a transport abstraction if a deployment requires it.