#pragma warning disable CA1822
using HungerGames.Helpers;
using HungerGames.Player;

namespace HungerGames.Commands;

[SlashCommandGroup("hungergames", "Hunger Games commands")]
class SlashCommands : ApplicationCommandModule
{
    [SlashCommand("create", "Create a new game")]
    public async Task CreateGame(InteractionContext ctx)
    {
        if (GlobalGames.IsRunning(ctx.Guild.Id))
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new() { Content = "Game is already running!" });
            return;
        }
        var game = new Game.Game();
        GlobalGames.AddGameInstance(ctx.Guild.Id, game, ctx.Guild);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new() { Content = "Game created!" });
    }

    [SlashCommand("volunteer", "Volunteer as tribute")]
    public async Task Tribute(InteractionContext ctx,
    [Option("district", "District number")] long district)
    {
        var game = GlobalGames.GetGame(ctx.Guild.Id);
        try { GameStatusCheck(game, ctx.Guild.Id); }
        catch (ArgumentException e)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new() { Content = e.Message });
            return;
        }
        var player = new PlayerObject(ctx.Member, (int)district);
        try
        {
            game!.AddPlayer(player);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new() { Content = $"Volunteered as tribute for district {district}" });
        }
        catch (ArgumentException e)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new() { Content = e.Message });
            return;
        }
    }

    [SlashCommand("start", "Start the game")]
    public async Task StartGame(InteractionContext ctx)
    {
        var game = GlobalGames.GetGame(ctx.Guild.Id);
        try { GameStatusCheck(game, ctx.Guild.Id); }
        catch (ArgumentException e) 
        { 
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new() { Content = e.Message }); 
            return; 
        }
        GlobalGames.UpdateGame(ctx.Guild.Id, true);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new() { Content = "Game started!", IsEphemeral = true });
        game!.ProgressDay(ctx.Guild, ctx.Channel.Id, ctx.Client);
    }

    [SlashCommand("lottery", "Assign a member to a random district")]
    public async Task Lottery(InteractionContext ctx, [Option("user", "User to enter the lottery")] DiscordUser member = null!)
    {
        if (member is not null && !ctx.Member.Permissions.HasPermission(Permissions.ManageGuild))
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new() { Content = "You do not have permission to assign a member to a district!" });
            return;
        }
        member ??= ctx.Member;
        var game = GlobalGames.GetGame(ctx.Guild.Id);
        try { GameStatusCheck(game, ctx.Guild.Id); }
        catch (ArgumentException e) 
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new() { Content = e.Message });
            return;
        }
        var player = new PlayerObject(member, Random.Shared.Choice(game!.GetPopulatableDistricts()));
        try
        {
            game!.AddPlayer(player);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new() { Content = $"Assigned {member.Mention} to district {player.District}" });
        }
        catch (ArgumentException e)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new() { Content = e.Message });
            return;
        }
    }
    [SlashCommand("list", "Lists all members in the game")]
    public async static Task ListMems(InteractionContext ctx)
    {
        Game.Game? game = GlobalGames.GetGame(ctx.Guild.Id);
        if (game is null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new() { Content = "No game is running! Please have a moderator create a game." });
            return;
        }
        var players = game.Players;
        var eb = new DiscordEmbedBuilder().WithTitle("Players")
            .WithColor(DiscordColor.Goldenrod)
            .WithDescription("*May the odds be ever in your favour.*")
            .WithThumbnail("https://orig00.deviantart.net/fe9e/f/2016/085/d/9/the_hunger_games__mockingjay_emblem__by_thephoenixprod-d9wlt8m.png", 50, 50)
            .WithTimestamp(DateTime.Now);

        for (int i = 1; i <= 12; i++)
        {
            var d_players = (from p in players
                    where p.District == i
                    select p.User.Mention).ToList();

            while (d_players.Count < 2)
                d_players.Add("Empty");

            GlobalLogger.Log(LogLevel.Debug, "Players from {District}: {Players}", i, string.Join(", ", d_players));
            eb.AddField(
                $"District {i}",
                string.Join('\n', d_players),
                true
            );
        }

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
            new DiscordInteractionResponseBuilder().AddEmbed(eb.Build()));
    }

    private static void GameStatusCheck(Game.Game? game, ulong guildId)
    {
        if (game is null)
        {
            throw new ArgumentException("No game is running! Please have a moderator create a game.");
        }
        if (GlobalGames.IsRunning(guildId))
        {
            throw new ArgumentException("Game is already running!");
        }
    }
}
