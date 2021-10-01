# Get started (Running the bot)

1. Add an environment variable named `RANKER_TOKEN` with a value of your bot token you received in **step 8, Set up a bot account**.
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
