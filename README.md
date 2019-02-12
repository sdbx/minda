![](logo.png)

# Minda

Minda is an online multiplayer [abalone](https://en.wikipedia.org/wiki/Abalone_(board_game)), available on Windows and mac os. It supports custom map, spectator mode and sharding.

## Build

### Client

Open /client in Unity 2018.

### Server

#### Lobby Server
```
cd lobby-server && go build
```
#### Game Server
```
cd game-server && cargo build
```

