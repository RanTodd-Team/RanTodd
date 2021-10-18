# Get started (Running the bot)

## Configuration
1. Clone the repository
2. Rename `config.example.json` to `config.json`.
3. Open the `config.json` file.
4. Modify the file to suit your needs
   - **Website:** Edit the `Domain` and `ClientId` JSON field to your website domain.
   - **Testing or one-server scenario:** Edit the `GuildId` JSON field to your server ID.
   - Command prefix.
5. Continue with [Docker](#docker) or [standalone program](#standalone).

## Docker
This method assumes you've already installed Docker Compose. If you haven't, visit [this link](https://docs.docker.com/compose/install/#install-compose) to find out how.

1. Rename `.env.example` to `.env`
2. Set the bot token in **step 8, Set up a bot account** in the `RANKER_TOKEN` field
3. Do the same with `RANKER_CLIENT_SECRET` but with your bot client secret.
4. Save your changes and run `docker-compose up -d`
   
   **Note:** Should you wish to rebuild the bot, add `--build` to the previous command.

## Standalone
You'll need .NET 5 SDK for this method. You can download the latest version [here](https://dotnet.microsoft.com/download/dotnet/5.0).

1. Add an environment variable named `RANKER_TOKEN` with a value of your bot token you received in **step 8, Set up a bot account**, do the same with `RANKER_CLIENT_SECRET` but with your bot client secret.
   Please note that you may need to restart or log out for this change to take effect.
2. Open a command line window and `cd` to Ranker. Make sure that `Ranker.csproj` exists in the selected folder.
3. Execute `dotnet build` and wait for the build to finish.
4. Execute `dotnet run` to run the bot.

Your bot is now up and running!
