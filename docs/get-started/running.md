# Get started (Running the bot)

You can choose to either host the bot in [Docker](#docker), or as a [standalone program](#standalone).

## Docker
This method assumes you've already installed Docker Compose. If you haven't, visit [this link](https://docs.docker.com/compose/install/#install-compose) to find out how.

1. Clone this repository and `cd` to the repository folder.
2. Rename `.env.example` to `.env`
3. Set the bot token in **step 8, Set up a bot account** in the `RANKER_TOKEN` field
4. Do the same with `RANKER_CLIENT_SECRET` but with your bot client secret.
5. Save your changes and run `docker-compose up -d`
   
   **Note:** Should you wish to rebuild the bot, add `--build` to the previous command.

## Standalone
You'll need .NET 5 SDK for this method. You can download the latest version [here](https://dotnet.microsoft.com/download/dotnet/5.0).

1. Add an environment variable named `RANKER_TOKEN` with a value of your bot token you received in **step 8, Set up a bot account**, do the same with `RANKER_CLIENT_SECRET` but with your bot client secret.
   Please note that you may need to restart or log out for this change to take effect.
2. Clone this repository.
3. Rename `config.example.json` to `config.json`.
4. Open the `config.json` file.
5. (Optionally, complete only if the website is configured) Edit the `Domain` JSON field to your website domain.
5. (Optionally but highly recommended for testing scenarios or one-server setup) Edit the `GuildId` JSON field to your server ID.
6. Open a command line window and `cd` to Ranker. Make sure that `Ranker.csproj` exists in the selected folder.
7. Execute `dotnet build` and wait for the build to finish.
8. Execute `dotnet run` to run the bot.

Your bot is now up and running!
