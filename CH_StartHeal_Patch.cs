using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace ContinuousHealing
{
	internal class CH_StartHeal_Patch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(Player.MedsController).GetMethod(nameof(Player.MedsController.smethod_6)).MakeGenericMethod(typeof(Player.MedsController));
		}

		[PatchPrefix]
		public static void Prefix(Player player)
		{
			if (player.IsYourPlayer)
			{
				CH_EndHeal_Patch.CancelRequsted = false;
			}
		}
	}
}
