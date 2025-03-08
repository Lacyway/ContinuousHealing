using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace ContinuousHealing.Patches
{
	internal class CH_StartHeal_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(Player.MedsController).GetMethod(nameof(Player.MedsController.Spawn));
		}

		[PatchPrefix]
		public static void Prefix(Player ____player)
		{
			if (____player.IsYourPlayer)
			{
				CH_EndHeal_Patch.CancelRequested = false;
                CH_EndHeal_Patch.Animation = 0;
			}
		}
	}
}
