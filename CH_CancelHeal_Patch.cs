using EFT;
using EFT.HealthSystem;
using SPT.Reflection.Patching;
using System.Reflection;

namespace ContinuousHealing
{
	internal class CH_CancelHeal_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(ActiveHealthController).GetMethod(nameof(ActiveHealthController.RemoveMedEffect));
		}

		[PatchPrefix]
		public static void Prefix(Player ___Player)
		{
#if DEBUG
			CH_Plugin.CH_Logger.LogWarning("Cancel requested!");
#endif
			if (___Player.IsYourPlayer)
			{
				CH_EndHeal_Patch.CancelRequsted = true;
			}
		}
	}
}
