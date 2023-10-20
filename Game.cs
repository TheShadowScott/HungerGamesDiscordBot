namespace HungerGames.Game;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using DSharpPlus.Interactivity.Extensions;
using HungerGames.Player;

public class Game
{
    private const int MAX_TIMEOUT_MINUTES = 5;
    private const int NEEDED_REACTIONS = 5;
    public List<PlayerObject> Players { get; init; } = new();

    // TODO: Add more events
    /// <summary>
    /// The first element is the event descriptor, the second element dictates if this is a death event
    /// </summary>
    public static readonly (string, bool)[] Events =
    {
        ("{0} was killed by {1}", true),
        ("{0} died of dysentary.", true),
        ("{0} recieved a gift from an unknown sponsor", false),
        ("{0} teamed up with {1}", false),
        ("{0} stepped on a landmine", true),
        ("{0} cried.", false),
        ("{0} was felled by their own hand.", true),
        ("{0} was stung by a tracker jacker and died", true),
        ("{0} was stung by a tracker jacker but miraculously lived thanks to the help of {1}", false),
        ("{0} ate poisonous berries. Those weren't food friend", true),
        ("{0} picked up supplies from the cornucopia", false),
        ("{0} went for a swim", false),
        ("{0} went to bed hungry", false),
        ("{0} teeters on the brink of dehydration", false),
        ("{0} found a pack of supplies", false),
        ("{1} robbed {0}", false),
        ("{1} was robbed by {0}", false),
        ("{0} was attacked by wolf muttations. Hope you didn't need your throat", true)
    };
    public void AddPlayer(PlayerObject player)
    {
        var idMatch = from p in Players
                      where p.User.Id == player.User.Id
                      select p;
        if (idMatch.Any())
            throw new ArgumentException("Player already exists in game");
        if (GetDistrictCount(player.District) >= 2)
            throw new ArgumentException("District is full");
        Players.Add(player);
    }
    private int GetDistrictCount(int district)
    {
        var districtMatch = from p in Players
                            where p.District == district
                            select p;
        return districtMatch.Count();
    }

    public List<int> GetPopulatableDistricts() => Enumerable.Range(1, 12)
                                                       .AsEnumerable()
                                                       .Where(i => GetDistrictCount(i) < 2)
                                                       .ToList();

    public (int, PlayerObject[])[] GetPlayersByDistrict()
    {
        var districts = from p in Players
                        group p by p.District into g
                        select g;
        return (from d in districts
                select (d.Key, d.ToArray())).ToArray();
    }

    public void Reset(ulong guildId)
    {
        GlobalGames.UpdateGame(guildId, false);
        foreach (var player in Players)
            player.Reset();
        // Players.Clear();
    }
    public async void ProgressDay(DiscordGuild guild, ulong channelId, DiscordClient client)
    {
        var channel = guild.GetChannel(channelId);
        await channel.SendMessageAsync("Welcome to the Hunger Games! May the odds be ever in your favor.");
        Console.WriteLine((from player in Players select player.User.Username).ToList().ToDisplayString());
        while (GlobalGames.IsRunning(guild.Id))
        {
            StringBuilder sb = new();
            foreach (var player in Players)
            {
                if (!player.Alive)
                    continue;
                var retVal = player.TriggerRandomEvent(this);
                sb.AppendLine(retVal);
            }
            await channel.SendMessageAsync(sb.ToString());

            var alivePlayers = from p in Players
                               where p.Alive
                               select p;
            if (alivePlayers.Count() == 1)
            {
                await channel.SendMessageAsync($"{alivePlayers.First().User.Mention} has won!");
                Reset(guild.Id);
                return;
            }
            else if (!alivePlayers.Any())
            {
                await channel.SendMessageAsync("Everyone died! No winner.");
                Reset(guild.Id);
                return;
            }
            if (!await NextDayReaction(channelId, client)) return;
        }
    }

    public async Task<bool> NextDayReaction(ulong channelId, DiscordClient client)
    {
        var emoji = DiscordEmoji.FromName(client, ":arrow_forward:");

        var message = await client.GetChannelAsync(channelId).Result.SendMessageAsync($"React with {emoji} to advance the day.");
        await message.CreateReactionAsync(emoji);

        int i = 0;
        var start = DateTime.Now;
        while (i < NEEDED_REACTIONS + 1)
        {
            var rx = await message.GetReactionsAsync(emoji);
            i = rx.Count;
            await Task.Delay(2000);
            if (DateTime.Now - start > TimeSpan.FromMinutes(MAX_TIMEOUT_MINUTES))
                return false;
        }
        return true;
    }
}
