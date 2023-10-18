using HungerGames.Helpers;

namespace HungerGames.Player;

public sealed class PlayerObject
{
    public DiscordUser User { get; init; }
    public bool Alive { get; private set; } = true;
    private int district;
    public int District
    {
        get { return district; }
        set
        {
            if (value < 1 || value > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(District), "District must be between 1 and 12");
            }
            district = value;
        }
    }
    public PlayerObject(DiscordUser user, int district)
    {
        User = user;
        District = district;
    }
    private void Kill() => Alive = false;
    public string TriggerRandomEvent(Game.Game game)
    {
        (string eventStr, bool death) = Random.Shared.Choice(Game.Game.Events);
        if (death)
            Kill();

        if (eventStr.Contains("{1}"))
        {
            return string.Format(eventStr, User.Mention, Random.Shared.ChoiceNotVal(
                (from p in game.Players select p.User).ToList(),
                User).Mention);
        }
        return string.Format(eventStr, User.Mention);
    }

}