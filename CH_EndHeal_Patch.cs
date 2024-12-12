using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace ContinuousHealing
{
	internal class CH_EndHeal_Patch : ModulePatch
	{
		public static bool CancelRequsted = false;

		protected override MethodBase GetTargetMethod()
		{
			return typeof(Player.MedsController.Class1158).GetMethod(nameof(Player.MedsController.Class1158.method_2));
		}

		[PatchPrefix]
		public static bool Prefix(Player.MedsController.Class1158 __instance, Player.MedsController ___medsController_0, IEffect effect, Callback<IOnHandsUseCallback> ___callback_0)
		{
			if (CancelRequsted)
			{
				return true;
			}

			if (!(effect is GInterface337))
			{
				return false;
			}

#if DEBUG
			if (effect is ActiveHealthController.GClass2746 durEffect)
			{
				CH_Plugin.CH_Logger.LogWarning("It's a durEffect, delay: " + durEffect.DelayTime);
			} 
#endif

			Traverse traverse = Traverse.Create(___medsController_0);
			Player player = traverse.Field<Player>("_player").Value;
			int animationVariant = traverse.Field<int>("int_0").Value;
			if (player == null)
			{
				return true;
			}

			if (!player.IsYourPlayer)
			{
				return true;
			}

			if (!(___medsController_0.Item is MedsItemClass))
			{
				return true;
			}

			if (player.ActiveHealthController.CanApplyItem(___medsController_0.Item, EBodyPart.Common))
			{
#if DEBUG
				CH_Plugin.CH_Logger.LogWarning("Can apply again!"); 
#endif
				player.HealthController.EffectRemovedEvent -= __instance.method_2;
				float originalDelay = ActiveHealthController.GClass2746.GClass2756_0.MedEffect.MedKitStartDelay;
				ActiveHealthController.GClass2746.GClass2756_0.MedEffect.MedKitStartDelay = 0f;
				IEffect newEffect = player.ActiveHealthController.DoMedEffect(___medsController_0.Item, EBodyPart.Common, 1f);
				if (newEffect == null)
				{
					__instance.State = Player.EOperationState.Finished;
					___medsController_0.FailedToApply = true;
					Callback<IOnHandsUseCallback> callbackToRun = ___callback_0;
					___callback_0 = null;
					callbackToRun(___medsController_0);
					ActiveHealthController.GClass2746.GClass2756_0.MedEffect.MedKitStartDelay = originalDelay;
					return false;
				};
				player.HealthController.EffectRemovedEvent += __instance.method_2;
				ActiveHealthController.GClass2746.GClass2756_0.MedEffect.MedKitStartDelay = originalDelay;
				return false;
			}

			return true;
		}
	}
}
