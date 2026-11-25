using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Options.Modifiers;

public class NeutralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "launchpad.options.neutralModifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 4;
    public override Color GroupColor => Color.gray;
}
