# StreetWars

StreetWars is a .NET 10 MAUI multiplayer card game built around territory domination and the CardGameEngine.

## Current base game

StreetWars supports five city territories: Downtown, Industrial, Docks, Old Highway and Neon District.

Each territory is represented by the corresponding board slot for each CGE player. A car contributes attack + life as its territory power. The player with the highest uncontested power controls the territory. Three controlled territories win the match.

The backend supports:
- PvP rooms with up to four players.
- AI rooms with Player B controlled by `IStreetWarsAi`.
- An `IPVPService`/`PvpService` abstraction for player-driven game actions.
- In-memory room/session storage.
- `GET /api/rooms` for available PvP rooms.
- `POST /api/rooms` to create a PvP room.
- `POST /api/rooms/ai` to create an AI room.
- `POST /api/rooms/{sessionId}/join` to join a room.
- Authenticated SignalR communication through `/hub/game`.

## Authentication flow

The room API issues an opaque cryptographically random credential containing `SessionId`, `PlayerId` and `AccessToken`. The MAUI client receives credentials when creating or joining a room and supplies the access token through SignalR's `AccessTokenProvider`.

The SignalR hub requires authentication and derives the session/player identity from authenticated claims. Game commands no longer accept arbitrary session/player IDs from the client.

Credentials and game sessions are intentionally stored in memory in this base implementation. Restarting the backend ends active rooms and invalidates credentials.

## Projects

- `src/StreetWars` — .NET MAUI client.
- `src/StreetWars.Backend` — ASP.NET Core Web API/SignalR game host.
- `lib/CGE` — CardGameEngine submodule.
- `lib/orbit` — Orbit submodule.

## Running locally

The backend listens on `http://0.0.0.0:7000`. The current physical Android development client uses `http://192.168.3.2:7000` and the SignalR hub is `http://192.168.3.2:7000/hub/game`.

The Android device and backend host must communicate over the LAN, and TCP port 7000 must be reachable from the device.

## Room UI

- **Criar** creates a PvP room, receives Player A credentials, connects to it and refreshes the available-room list.
- **Entrar** joins the selected room from the backend list and receives that player's credentials.
- **Atualizar** refreshes the available-room list.
- **Vs IA** creates an AI room, receives Player A credentials and connects to the authenticated room.

The room list contains only PvP rooms that still have capacity.

## SignalR

SignalR is the current real-time transport. MQTT can be introduced later behind a transport abstraction if required.
