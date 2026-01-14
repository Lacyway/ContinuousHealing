using System.Reflection;
using EFT;
using SPT.Reflection.Patching;

namespace ContinuousHealing.Patches;

internal class CH_CancelHeal_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GClass3010)
            .GetMethod(nameof(GClass3010.CancelApplyingItem));
    }

    [PatchPrefix]
    public static void Prefix(Player ___Player)
    {
#if DEBUG
        CH_Plugin.CH_Logger.LogWarning("Cancel requested!");
#endif
        if (___Player.IsYourPlayer)
        {
            CH_EndHeal_Patch.CancelRequested = true;
        }
    }
}
