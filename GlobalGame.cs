using HungerGames.Game;

public static class GlobalGames
{
    private struct GameInstance
    {
        public Game? GameRef { get; init; }
        public DiscordGuild? Guild { get; init; }
        public bool IsRunning { get; set; }
    }

    private static readonly Dictionary<ulong, GameInstance> Games = new();
    public static bool IsRunning(ulong guildId)
    {
        if (Games.TryGetValue(guildId, out var gameInstance))
        {
            return gameInstance.IsRunning;
        }
        return false;
    }
    public static Game? GetGame(ulong guildId)
    {
        if (Games.TryGetValue(guildId, out var gameInstance))
        {
            return gameInstance.GameRef;
        }
        return null;
    }

    public static void UpdateGame(ulong guildId, bool active)
    {
        if (Games.TryGetValue(guildId, out var gameInstance))
        {
            gameInstance.IsRunning = active;
        }

        Games[guildId] = gameInstance;
    }

    public static void AddGameInstance(ulong guildId, Game game, DiscordGuild guild)
    {
        Games.Add(guildId, new() { GameRef = game, Guild = guild, IsRunning = false });
    }
}