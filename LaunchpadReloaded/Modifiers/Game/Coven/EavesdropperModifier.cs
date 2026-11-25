using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;

namespace LaunchpadRevamped.Modifiers.Game.Coven;

/// <summary>
/// Ported from TOHO (Eavesdropper / Sleuth). When a meeting starts because a body was reported, you have a chance to learn
/// the role of the dead player. Only your own client shows the message.
/// </summary>
public sealed class EavesdropperModifier : ExtraModifier
{
    protected override string Key => "eavesdropper";
    protected override string DisplayName => "Eavesdropper";

    protected override string Description =>
        $"When a body is reported you have a {OptionGroupSingleton<ExtraCovenModifierOptions>.Instance.EavesdropperRevealChance:0}% chance to learn the dead player's role.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraCovenModifierOptions>.Instance.EavesdropperChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsCoven(role);

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent @event)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.Data.IsDead || !local.HasModifier<EavesdropperModifier>())
        {
            return;
        }

        var body = ModifierHelpers.ReportedBody();
        if (body == null || !body.Role)
        {
            return;
        }

        if (UnityEngine.Random.value * 100f >= OptionGroupSingleton<ExtraCovenModifierOptions>.Instance.EavesdropperRevealChance)
        {
            return;
        }

        var role = body.Role;
        var roleName = role is ICustomRole custom ? custom.RoleName : role.NiceName;
        RpcWelcomeMessage.ShowLocal($"You overheard it: {body.PlayerName} was a {roleName}.");
    }
}
