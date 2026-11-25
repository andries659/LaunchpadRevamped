using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Ported from TOHO (Quota). The Impostor only wins with the team if they made enough kills.
/// </summary>
public sealed class QuotaModifier : ExtraModifier
{
    protected override string Key => "quota";
    protected override string DisplayName => "Quota";

    protected override string Description =>
        $"You must kill {(int)OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.QuotaKills} player(s) to win with your team. Without them you lose.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.QuotaChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsImpostorTeam(role);

    public int Kills { get; private set; }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent @event)
    {
        var quota = @event.Source ? @event.Source.GetModifier<QuotaModifier>() : null;
        if (quota != null)
        {
            quota.Kills++;
        }
    }

    // Returning null keeps the normal win condition, false takes the win away.
    public override bool? DidWin(GameOverReason reason)
    {
        return Kills >= (int)OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.QuotaKills ? null : false;
    }
}
