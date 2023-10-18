using dotenv.net;
using HungerGames.Game;
using HungerGames.Player;

DotEnv.Load();
var env = DotEnv.Read();
if (GlobalLogger.IsEnabled(LogLevel.Debug))
    GlobalLogger.Log(LogLevel.Debug, "Env Loaded with {Count} variables", env.Count);

DiscordClient client = new(new()
{
    Token = env["TOKEN"],
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildPresences | DiscordIntents.MessageContents
});

var slash = client.UseSlashCommands();
slash.RegisterCommands<SlashCommands>();

await client.ConnectAsync();
await Task.Delay(Timeout.Infinite);