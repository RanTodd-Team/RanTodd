# Ranker
[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](CODE_OF_CONDUCT.md)
![.NET workflow status](https://github.com/SapphireDisD/Ranker/actions/workflows/dotnet.yml/badge.svg)

Ranker is a Discord ranking bot originally designed for Microsoft Community. It offers from a simple rank card to a complex leaderboard.

# How to self-host?
It's pretty simple! Just follow these instructions:
1. Clone the repo.
2. In the Ranker folder, rename `config.example.json` to `config.json`
3. Configure the config.json, `Token` is not needed if you set `RANKER_TOKEN` environment variable. `GuildId` is needed only if you need the slash commands in only one server.
4. Open a terminal there.
5. `cd` to Ranker.
6. Build it with `dotnet build`.
7. Run it with `dotnet run`.

# Contributors
We'd like to thank our contributors for making this possible. 
- [@SapphireDisD](https://github.com/SapphireDisD): Authoring
- [@dongle-the-gadget](https://github.com/dongle-the-gadget): Coding most of the work
- [@zeealeid](https://github.com/zeealeid), [@KojiOddysey](https://github.com/KojiOddysey), [@dAKirby309](https://github.com/dAKirby309), [@itsWindows11](https://github.com/itsWindows11): Designing the default rank card
- [@Ahmed605](https://github.com/Ahmed605): Coding most of the default rank card
- Fleuron¹: Designing the second rank card

¹ from Microsoft Community Discord
