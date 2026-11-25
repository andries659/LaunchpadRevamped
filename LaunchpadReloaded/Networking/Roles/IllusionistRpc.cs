using LaunchpadRevamped.Roles.Coven;
using Reactor.Networking.Attributes;

namespace LaunchpadRevamped.Networking.Roles;

public static class IllusionistRpc
{
    [MethodRpc((uint)LaunchpadRpc.IllusionMark)]
    public static void RpcIllusion(this PlayerControl illusionist, byte targetId)
    {
        if (illusionist.Data.Role is not IllusionistRole)
        {
            illusionist.KickForCheating();
            return;
        }

        IllusionistRole.Illusioned[targetId] = illusionist.PlayerId;
    }
}
